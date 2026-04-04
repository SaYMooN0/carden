import type { StudyDeckLoadResponse, StudyLoadedCardResponse, StudySettings } from "../shared_types";

export type CurrentCardSide = 'Front' | 'Back';
export type CardDifficulty = 'Again' | 'Hard' | 'Good' | 'Easy';



export type LocalCardStudyState =
    | { kind: 'New' }
    | {
        kind: 'Learning';
        dueAtMs: number;
        learningStepIndex: number;
        startedAtMs: number;
    }
    | {
        kind: 'Review';
        dueAtMs: number;
        reviewIntervalSeconds: number;
        lastReviewedAtMs: number;
    };

export type StudyCard = {
    id: string;
    contentFront: string[];
    contentBack: string[];
    creationTime: string;
    lastTimeEdited: string;
    studyState: LocalCardStudyState;
};

export type StudyAnswerEvent = {
    cardId: string;
    difficulty: CardDifficulty;
    shownAtOffsetMs: number;
    answeredAtOffsetMs: number;
};

export type DeckStudyState =
    | {
        state: 'Card';
        currentSide: CurrentCardSide,
        currentCard: StudyCard
    }
    | {
        state: 'Finished';
        newCardsLeft: number;
        reviewCardsLeft: number;
        totalAnswersCount: number;
        uniqueCardsSeenCount: number;
    };

export class StudyDeckPageState {
    #plant: { id: string, name: string } = $state()!;
    #deckStudyState: DeckStudyState = $state()!;
    #onError: (msg: string) => void = $state()!;
    #settings: StudySettings = $state()!;

    #cardsById: Record<string, StudyCard> = $state({});
    #availableQueue: string[] = $state([]);
    #delayedQueue: { cardId: string; dueAtMs: number }[] = $state([]);
    #answerEvents: StudyAnswerEvent[] = $state([]);

    #sessionStartedClientMs = $state(0);
    #sessionStartedServerMs = $state(0);
    #currentCardShownAtClientMs = $state(0);

    private initialNewCardIds: string[] = [];
    private initialReviewCardIds: string[] = [];

    constructor(data: StudyDeckLoadResponse, onError: (msg: string) => void) {
        this.#plant = { id: data.plant.id, name: data.plant.name };
        this.#settings = data.studySettings;
        this.#onError = onError;
        this.#sessionStartedClientMs = Date.now();
        this.#sessionStartedServerMs = Date.parse(data.serverNow);

        const normalizedReviewCards = data.reviewCards.map((card) => this.normalizeCard(card));
        const normalizedNewCards = data.newCards.map((card) => this.normalizeCard(card));

        this.initialReviewCardIds = normalizedReviewCards.map((card) => card.id);
        this.initialNewCardIds = normalizedNewCards.map((card) => card.id);

        for (const card of [...normalizedReviewCards, ...normalizedNewCards]) {
            this.#cardsById[card.id] = card;
        }

        this.#availableQueue = [
            ...normalizedReviewCards.map((card) => card.id),
            ...normalizedNewCards.map((card) => card.id)
        ];

        this.#delayedQueue = [];
        this.#answerEvents = [];

        this.goToNextCardOrFinish();
    }

    get plantName() {
        return this.#plant.name;
    }

    get plantId() {
        return this.#plant.id;
    }

    get deckStudyState() {
        return this.#deckStudyState;
    }

    get answerEvents() {
        return this.#answerEvents;
    }

    get completionPayload() {
        return {
            answerEvents: this.#answerEvents.map((event) => ({ ...event }))
        };
    }

    totalAnswersCount = $derived(this.#answerEvents.length);

    uniqueCardsSeenCount: number = $derived.by(() => {
        return new Set(this.#answerEvents.map((event) => event.cardId)).size;
    });

    newCardsLeft: number = $derived.by(() => {
        return this.countRemainingCardsByIds(this.initialNewCardIds);
    });

    reviewCardsLeft: number = $derived.by(() => {
        return this.countRemainingCardsByIds(this.initialReviewCardIds);
    });

    cardsStillInSessionCount: number = $derived.by(() => {
        const currentCardCount = this.#deckStudyState.state === 'Card' ? 1 : 0;
        return currentCardCount + this.#availableQueue.length + this.#delayedQueue.length;
    });

    flipCurrentCardToBack() {
        if (this.#deckStudyState.state !== 'Card') {
            this.#onError('No card is currently selected.');
            return;
        }

        if (this.#deckStudyState.currentSide === 'Back') {
            return;
        }

        this.#deckStudyState = {
            ...this.#deckStudyState,
            currentSide: 'Back'
        };
    }

    rateCurrentCardDifficulty(difficulty: CardDifficulty) {
        if (this.#deckStudyState.state !== 'Card') {
            this.#onError('No card is currently selected.');
            return;
        }

        if (this.#deckStudyState.currentSide !== 'Back') {
            this.#onError('Flip the card to the back before rating it.');
            return;
        }

        const currentCard = this.#deckStudyState.currentCard;

        const answeredAtClientMs = Date.now();

        this.#answerEvents.push({
            cardId: currentCard.id,
            difficulty,
            shownAtOffsetMs: this.#currentCardShownAtClientMs - this.#sessionStartedClientMs,
            answeredAtOffsetMs: answeredAtClientMs - this.#sessionStartedClientMs
        });

        const nextStudyState = this.calculateNextStudyState(
            currentCard.studyState,
            difficulty,
            answeredAtClientMs
        );

        this.#cardsById[currentCard.id] = {
            ...currentCard,
            studyState: nextStudyState
        };

        if (nextStudyState.kind === 'Learning') {
            this.enqueueDelayedCard(currentCard.id, nextStudyState.dueAtMs);
        }

        this.goToNextCardOrFinish();
    }

    private normalizeCard(card: StudyLoadedCardResponse): StudyCard {
        return {
            id: card.id,
            contentFront: [...card.contentFront],
            contentBack: [...card.contentBack],
            creationTime: card.creationTime,
            lastTimeEdited: card.lastTimeEdited,
            studyState: this.normalizeStudyState(card)
        };
    }

    private normalizeStudyState(card: StudyLoadedCardResponse): LocalCardStudyState {
        if (card.studyStateType === 'Learning') {
            return {
                kind: 'Learning',
                dueAtMs: card.studyDueAt ? Date.parse(card.studyDueAt) : this.#sessionStartedServerMs,
                learningStepIndex: card.studyLearningStepIndex ?? 0,
                startedAtMs: card.studyStartedAt ? Date.parse(card.studyStartedAt) : this.#sessionStartedServerMs
            };
        }

        if (card.studyStateType === 'Review') {
            return {
                kind: 'Review',
                dueAtMs: card.studyDueAt ? Date.parse(card.studyDueAt) : this.#sessionStartedServerMs,
                reviewIntervalSeconds: card.studyReviewIntervalSeconds ?? this.#settings.learningEasyIntervalSeconds,
                lastReviewedAtMs: card.studyLastReviewedAt
                    ? Date.parse(card.studyLastReviewedAt)
                    : this.#sessionStartedServerMs
            };
        }

        return { kind: 'New' };
    }

    private goToNextCardOrFinish() {
        if (this.#availableQueue.length > 0) {
            const nextCardId = this.#availableQueue.shift()!;
            this.setCurrentCard(nextCardId);
            return;
        }

        if (this.#delayedQueue.length > 0) {
            this.#delayedQueue.sort((a, b) => a.dueAtMs - b.dueAtMs);
            const nextDelayed = this.#delayedQueue.shift()!;
            this.setCurrentCard(nextDelayed.cardId);
            return;
        }

        this.#deckStudyState = {
            state: 'Finished',
            newCardsLeft: this.countRemainingCardsByIds(this.initialNewCardIds),
            reviewCardsLeft: this.countRemainingCardsByIds(this.initialReviewCardIds),
            totalAnswersCount: this.#answerEvents.length,
            uniqueCardsSeenCount: new Set(this.#answerEvents.map((event) => event.cardId)).size
        };
    }

    private setCurrentCard(cardId: string) {
        this.#currentCardShownAtClientMs = Date.now();
        this.#deckStudyState = {
            state: 'Card',
            currentSide: 'Front',
            currentCard: this.#cardsById[cardId]
        };
    }

    private enqueueDelayedCard(cardId: string, dueAtMs: number) {
        this.#delayedQueue = this.#delayedQueue.filter((item) => item.cardId !== cardId);
        this.#delayedQueue.push({ cardId, dueAtMs });
        this.#delayedQueue.sort((a, b) => a.dueAtMs - b.dueAtMs);
    }

    private calculateNextStudyState(
        previousState: LocalCardStudyState,
        difficulty: CardDifficulty,
        nowClientMs: number
    ): LocalCardStudyState {
        switch (previousState.kind) {
            case 'New':
                return this.calculateNextStateFromNew(difficulty, nowClientMs);

            case 'Learning':
                return this.calculateNextStateFromLearning(previousState, difficulty, nowClientMs);

            case 'Review':
                return this.calculateNextStateFromReview(previousState, difficulty, nowClientMs);
        }
    }

    private calculateNextStateFromNew(
        difficulty: CardDifficulty,
        nowClientMs: number
    ): LocalCardStudyState {
        switch (difficulty) {
            case 'Again':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningAgainDelaySeconds * 1000,
                    learningStepIndex: 0,
                    startedAtMs: nowClientMs
                };

            case 'Hard':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningHardDelaySeconds * 1000,
                    learningStepIndex: 1,
                    startedAtMs: nowClientMs
                };

            case 'Good':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningGoodDelaySeconds * 1000,
                    learningStepIndex: 2,
                    startedAtMs: nowClientMs
                };

            case 'Easy':
                return {
                    kind: 'Review',
                    dueAtMs: nowClientMs + this.#settings.learningEasyIntervalSeconds * 1000,
                    reviewIntervalSeconds: this.#settings.learningEasyIntervalSeconds,
                    lastReviewedAtMs: nowClientMs
                };
        }
    }

    private calculateNextStateFromLearning(
        previousState: Extract<LocalCardStudyState, { kind: 'Learning' }>,
        difficulty: CardDifficulty,
        nowClientMs: number
    ): LocalCardStudyState {
        switch (difficulty) {
            case 'Again':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningAgainDelaySeconds * 1000,
                    learningStepIndex: 0,
                    startedAtMs: previousState.startedAtMs
                };

            case 'Hard':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningHardDelaySeconds * 1000,
                    learningStepIndex: 1,
                    startedAtMs: previousState.startedAtMs
                };

            case 'Good':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.learningGoodDelaySeconds * 1000,
                    learningStepIndex: Math.max(previousState.learningStepIndex, 2),
                    startedAtMs: previousState.startedAtMs
                };

            case 'Easy':
                return {
                    kind: 'Review',
                    dueAtMs: nowClientMs + this.#settings.learningEasyIntervalSeconds * 1000,
                    reviewIntervalSeconds: this.#settings.learningEasyIntervalSeconds,
                    lastReviewedAtMs: nowClientMs
                };
        }
    }

    private calculateNextStateFromReview(
        previousState: Extract<LocalCardStudyState, { kind: 'Review' }>,
        difficulty: CardDifficulty,
        nowClientMs: number
    ): LocalCardStudyState {
        switch (difficulty) {
            case 'Again':
                return {
                    kind: 'Learning',
                    dueAtMs: nowClientMs + this.#settings.reviewAgainDelaySeconds * 1000,
                    learningStepIndex: 0,
                    startedAtMs: nowClientMs
                };

            case 'Hard': {
                const reviewIntervalSeconds = this.multiplyIntervalSeconds(
                    previousState.reviewIntervalSeconds,
                    this.#settings.reviewHardIntervalMultiplier
                );

                return {
                    kind: 'Review',
                    dueAtMs: nowClientMs + reviewIntervalSeconds * 1000,
                    reviewIntervalSeconds,
                    lastReviewedAtMs: nowClientMs
                };
            }

            case 'Good': {
                const reviewIntervalSeconds = this.multiplyIntervalSeconds(
                    previousState.reviewIntervalSeconds,
                    this.#settings.reviewGoodIntervalMultiplier
                );

                return {
                    kind: 'Review',
                    dueAtMs: nowClientMs + reviewIntervalSeconds * 1000,
                    reviewIntervalSeconds,
                    lastReviewedAtMs: nowClientMs
                };
            }

            case 'Easy': {
                const reviewIntervalSeconds = this.multiplyIntervalSeconds(
                    previousState.reviewIntervalSeconds,
                    this.#settings.reviewEasyIntervalMultiplier
                );

                return {
                    kind: 'Review',
                    dueAtMs: nowClientMs + reviewIntervalSeconds * 1000,
                    reviewIntervalSeconds,
                    lastReviewedAtMs: nowClientMs
                };
            }
        }
    }

    private multiplyIntervalSeconds(currentIntervalSeconds: number, multiplier: number): number {
        return Math.max(1, Math.round(currentIntervalSeconds * multiplier));
    }

    private countRemainingCardsByIds(ids: string[]): number {
        const currentCardId = this.#deckStudyState.state === 'Card' ? this.#deckStudyState.currentCard.id : null;
        const available = new Set(this.#availableQueue);
        const delayed = new Set(this.#delayedQueue.map((item) => item.cardId));

        return ids.filter((id) => id === currentCardId || available.has(id) || delayed.has(id)).length;
    }
}