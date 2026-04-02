import { Backend, RJO } from '$lib/ts/backend';
import type { Card, CardContentItem, Deck, Plant } from '$lib/ts/base-types';
import { StringUtils } from '$lib/ts/utils/string-utils';

type CardEditingState =
    | { state: 'NoCardSelected' }
    | { state: 'ExpectedCardNotFound'; cardId: string }
    | { state: 'CardEditing'; card: CardInEditing }
    | { state: 'CardReloading'; cardId: string };

export class EditPlantPageState {
    #plant: PlantInEditing = $state()!;
    #cardEditingState: CardEditingState = $state({ state: 'NoCardSelected' });
    #newCardSelectionWhenUnsavedChangesGuard: { activate: (cardId: string) => void } = $state()!;

    private newCardIdPrefix = 'new-card-';

    get plantName() {
        return this.#plant.name;
    }

    get cardsCount() {
        return this.#plant.deck.cards.length;
    }

    get firstCardId() {
        return this.#plant.deck.cards[0]?.id ?? null;
    }

    get cardEditingState() {
        return this.#cardEditingState;
    }

    constructor(plant: Plant, guard: { activate: (cardId: string) => void }) {
        this.#plant = {
            ...plant,
            deck: {
                ...plant.deck,
                cards: plant.deck.cards.map((card) => this.createEditingCard(card))
            }
        };
        this.#cardEditingState = { state: 'NoCardSelected' };
        this.#newCardSelectionWhenUnsavedChangesGuard = guard;
    }

    #getTextPreview(text: string | null | undefined): string {
        const trimmed = text?.trim();
        return !trimmed ? 'Empty side' : trimmed.length > 72 ? `${trimmed.slice(0, 72)}` : trimmed;
    }

    get plantDeckCardsPreview(): CardPreview[] {
        return this.#plant.deck.cards.map((card, index) => {
            const frontTextPreview = this.#getTextPreview(card.contentFront?.[0]?.text);
            const backTextPreview = this.#getTextPreview(card.contentBack?.[0]?.text);
            return { frontTextPreview, backTextPreview, id: card.id, number: index + 1 };
        });
    }

    anyUnsavedChangesOnTheCurrentCard: boolean = $derived.by(() => {
        if (this.#cardEditingState.state !== 'CardEditing') {
            return false;
        }

        const card = this.#cardEditingState.card;
        const originalCard = this.#plant.deck.cards.find((currentCard) => currentCard.id === card.id);
        if (!originalCard) {
            return false;
        }

        const currentFrontLength = card.contentFront.length;
        const currentBackLength = card.contentBack.length;
        const originalFrontLength = originalCard.contentFront.length;
        const originalBackLength = originalCard.contentBack.length;

        return !(
            currentFrontLength === originalFrontLength &&
            currentBackLength === originalBackLength &&
            this.checkIfCardContentItemsListsEqual(card.contentFront, originalCard.contentFront) &&
            this.checkIfCardContentItemsListsEqual(card.contentBack, originalCard.contentBack)
        );
    });

    selectCard(cardId: string, options: { ignoreUnsavedChangesGuard: boolean }) {
        if (this.#cardEditingState.state === 'CardEditing' && cardId === this.#cardEditingState.card.id) {
            return;
        }
        if (!options.ignoreUnsavedChangesGuard && this.anyUnsavedChangesOnTheCurrentCard) {
            this.#newCardSelectionWhenUnsavedChangesGuard.activate(cardId);
            return;
        }
        const card = this.#plant.deck.cards.find((currentCard) => currentCard.id === cardId);
        if (!card) {
            this.#cardEditingState = { state: 'ExpectedCardNotFound', cardId };
            return;
        }

        this.#cardEditingState = {
            state: 'CardEditing',
            card: this.cloneEditingCard(card)
        };
    }

    async saveCurrentCardChanges(): Promise<{ isSuccess: true } | { isSuccess: false; errMsg: string }> {
        if (this.#cardEditingState.state !== 'CardEditing') {
            return { isSuccess: false, errMsg: 'No card is selected' };
        }

        const cardToSave = this.#cardEditingState.card;
        const obj = {
            cardId: this.isNewCard(cardToSave) ? null : cardToSave.id,
            contentFront: cardToSave.contentFront.map((item) => ({ text: item.text })),
            contentBack: cardToSave.contentBack.map((item) => ({ text: item.text }))
        };
        const response = await Backend.fetchJsonResponse<Card>(
            `/plants/${this.#plant.id}/save-card`,
            RJO.PATCH(obj)
        );

        if (!response.isSuccess) {
            return { isSuccess: false, errMsg: response.errs[0].msg };
        }

        const savedCard = this.mergeCardWithEditingKeys(response.data, cardToSave);
        this.upsertCardInPlant(savedCard, cardToSave.id);
        this.#cardEditingState = {
            state: 'CardEditing',
            card: this.cloneEditingCard(savedCard)
        };
        return { isSuccess: true };
    }

    resetCurrentCardChanges() {
        if (this.#cardEditingState.state !== 'CardEditing') {
            return;
        }

        const cardId = this.#cardEditingState.card.id;
        const originalCard = this.#plant.deck.cards.find((currentCard) => currentCard.id === cardId);
        const cardToSet: CardInEditing = originalCard
            ? this.cloneEditingCard(originalCard)
            : {
                id: cardId,
                contentFront: [],
                contentBack: [],
                lastTimeEdited: new Date().toISOString(),
                creationTime: new Date().toISOString()
            };

        this.#cardEditingState = {
            state: 'CardEditing',
            card: cardToSet
        };
    }

    checkIfCardContentItemsListsEqual(card1: CardContentItem[], card2: CardContentItem[]): boolean {
        return (
            card1.length === card2.length &&
            card1.every((item, index) => {
                const secondItem = card2[index];
                return item.text === secondItem?.text;
            })
        );
    }

    addNewCard() {
        const newCard: CardInEditing = {
            id: this.newCardIdPrefix + StringUtils.rndStr(12),
            contentFront: [],
            contentBack: [],
            lastTimeEdited: new Date().toISOString(),
            creationTime: new Date().toISOString()
        };
        this.#plant.deck.cards.push(newCard);
        if (!this.anyUnsavedChangesOnTheCurrentCard) {
            this.selectCard(newCard.id, { ignoreUnsavedChangesGuard: false });
        }
    }

    async reloadCard(cardId: string) {
        this.#cardEditingState = { state: 'CardReloading', cardId };
        const response = await Backend.fetchJsonResponse<Card>(
            `/plants/${this.#plant.id}/cards/${cardId}/load`,
            { method: 'GET' }
        );
        if (!response.isSuccess) {
            this.#cardEditingState = { state: 'ExpectedCardNotFound', cardId };
            return;
        }

        const reloadedCard = this.createEditingCard(response.data);
        this.upsertCardInPlant(reloadedCard, cardId);
        this.#cardEditingState = {
            state: 'CardEditing',
            card: this.cloneEditingCard(reloadedCard)
        };
    }

    private createEditingCard(card: Card): CardInEditing {
        return {
            ...card,
            contentFront: this.createEditingSide(card.contentFront, 'contentFront'),
            contentBack: this.createEditingSide(card.contentBack, 'contentBack')
        };
    }

    private cloneEditingCard(card: CardInEditing): CardInEditing {
        return {
            ...card,
            contentFront: card.contentFront.map((item) => ({ ...item })),
            contentBack: card.contentBack.map((item) => ({ ...item }))
        };
    }

    private createEditingSide(
        contentItems: CardContentItem[],
        contentSide: CardContentSide
    ): CardContentWithStringId[] {
        const sideChar = contentSide === 'contentFront' ? 'cf' : 'cb';
        return contentItems.map((item) => ({
            ...item,
            stringId: StringUtils.rndStrWithPref(`${sideChar}-`, 12)
        }));
    }

    private mergeCardWithEditingKeys(savedCard: Card, previousEditingCard: CardInEditing): CardInEditing {
        return {
            ...savedCard,
            contentFront: this.mergeSideWithKeys(
                savedCard.contentFront,
                previousEditingCard.contentFront,
                'contentFront'
            ),
            contentBack: this.mergeSideWithKeys(
                savedCard.contentBack,
                previousEditingCard.contentBack,
                'contentBack'
            )
        };
    }

    private mergeSideWithKeys(
        persistedItems: CardContentItem[],
        previousItems: CardContentWithStringId[],
        contentSide: CardContentSide
    ): CardContentWithStringId[] {
        const sideChar = contentSide === 'contentFront' ? 'cf' : 'cb';
        const availableKeys = previousItems.map((item) => item.stringId);
        return persistedItems.map((item, index) => ({
            ...item,
            stringId: availableKeys[index] ?? StringUtils.rndStrWithPref(`${sideChar}-`, 12)
        }));
    }

    private upsertCardInPlant(card: CardInEditing, oldCardId?: string) {
        const currentId = oldCardId ?? card.id;
        const existingCardIndex = this.#plant.deck.cards.findIndex(
            (existingCard) => existingCard.id === currentId || existingCard.id === card.id
        );
        if (existingCardIndex === -1) {
            this.#plant.deck.cards.push(card);
            return;
        }

        this.#plant.deck.cards[existingCardIndex] = this.cloneEditingCard(card);
    }

    private isNewCard(card: CardInEditing): boolean {
        return card.id.startsWith(this.newCardIdPrefix);
    }
}

export type PlantInEditing = Omit<Plant, 'deck'> & {
    deck: Omit<Deck, 'cards'> & {
        cards: CardInEditing[];
    };
};

export type CardContentSide = 'contentFront' | 'contentBack';

export type CardContentWithStringId = CardContentItem & { stringId: string };

export type CardInEditing = Omit<Card, CardContentSide> & {
    contentFront: CardContentWithStringId[];
    contentBack: CardContentWithStringId[];
};

export type CardPreview = {
    id: string;
    frontTextPreview: string;
    backTextPreview: string;
    number: number;
};
