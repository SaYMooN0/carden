import { Backend, RJO, type BackendResponse } from "$lib/ts/backend";
import type { Card, CardContentItem, Plant } from "$lib/ts/base-types";
import { StringUtils } from "$lib/ts/utils/string-utils";

type CardEditingState =
    | { state: "NoCardSelected"; }
    | { state: "ExpectedCardNotFound"; cardId: string; }
    | { state: "CardEditing"; card: CardViewToEdit; }
    | { state: "CardReloading"; cardId: string; }

export class EditPlantPageState {

    #plant: Plant = $state()!;
    #cardEditingState: CardEditingState = $state({ state: "NoCardSelected" });
    get plantName() {
        return this.#plant.name;
    }
    get cardsCount() {
        return this.#plant.deck.cards.length;
    }
    #getTextPreview(text: string | null | undefined): string {
        const trimmed = text?.trim();
        return !trimmed ?
            'Empty side'
            : trimmed.length > 72
                ? `${trimmed.slice(0, 72)}`
                : trimmed;
    }

    get plantDeckCardsPreview(): CardPreview[] {
        return this.#plant.deck.cards.map((c, i) => {
            const frontTextPreview = this.#getTextPreview(c.contentFront?.[0]?.text);
            const backTextPreview = this.#getTextPreview(c.contentBack?.[0]?.text);
            return { frontTextPreview, backTextPreview, id: c.id, number: i + 1 };
        });
    }
    get firstCardId() {
        return this.#plant.deck.cards[0].id;
    }
    get cardEditingState() {
        return this.#cardEditingState;
    }
    constructor(plant: Plant) {
        this.#plant = plant;
        this.#cardEditingState = { state: "NoCardSelected" };
    }
    anyUnsavedChangesOnTheCurrentCard: boolean = $derived.by(() => {
        if (this.#cardEditingState.state !== "CardEditing") {
            return false;
        }
        const card = this.#cardEditingState.card;
        const originalCard = this.#plant.deck.cards.find(c => c.id === card.id);
        if (!originalCard) {
            return false;
        }
        return !(this.checkIfCardContentItemsListsEqual(card.contentFront, originalCard.contentFront)
            && this.checkIfCardContentItemsListsEqual(card.contentBack, originalCard.contentBack));
    });
    selectCard(cardId: string) {
        const card = this.#plant.deck.cards.find(c => c.id === cardId);
        if (!card) {
            this.#cardEditingState = { state: "ExpectedCardNotFound", cardId };
            return;
        }
        this.#cardEditingState = {
            state: "CardEditing",
            card: {
                ...card,
                contentFront: card.contentFront.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cf-${i}-`, 3)
                }
                )),
                contentBack: card.contentBack.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cb-${i}-`, 3)
                }))
            }
        };
    }

    async saveCurrentCardChanges(): Promise<{ isSuccess: true; } | { isSuccess: false; errMsg: string; }> {
        if (this.#cardEditingState.state !== "CardEditing") {
            return { isSuccess: false, errMsg: "No card is selected" };
        }
        const cardToSave = this.#cardEditingState.card;
        const response = await Backend.fetchJsonResponse<Card>(
            `/plants/${this.#plant.id}/upsert-card`,
            RJO.PATCH({
                cardId: cardToSave.id,
                contentFront: cardToSave.contentFront.map(c => ({ text: c.text })),
                contentBack: cardToSave.contentBack.map(c => ({ text: c.text })),
            })
        );
        if (!response.isSuccess) {
            return { isSuccess: false, errMsg: response.errs[0].msg };
        }
        const card = response.data;
        this.#cardEditingState = {
            state: "CardEditing",
            card: {
                ...card,
                contentFront: card.contentFront.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cf-${i}-`, 3)
                }
                )),
                contentBack: card.contentBack.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cb-${i}-`, 3)
                }))
            }
        };
        return { isSuccess: true };
    }
    resetCurrentCardChanges() {
        if (this.#cardEditingState.state !== "CardEditing") {
            return;
        }
        const cardId = this.#cardEditingState.card.id;
        const originalCard = this.#plant.deck.cards.find(c => c.id === cardId);
        const cardToSet: CardViewToEdit = originalCard
            ? {
                ...originalCard,
                contentFront: originalCard.contentFront.map((c, i) => ({ ...c, stringId: StringUtils.rndStrWithPref(`cf-${i}-`, 3) })),
                contentBack: originalCard.contentBack.map((c, i) => ({ ...c, stringId: StringUtils.rndStrWithPref(`cb-${i}-`, 3) }))
            }
            : {
                id: cardId,
                contentFront: [],
                contentBack: [],
                lastTimeEdited: new Date().toISOString(),
                creationTime: new Date().toISOString(),
            };

        this.#cardEditingState = {
            state: "CardEditing",
            card: cardToSet
        };
    }
    checkIfCardContentItemsListsEqual(card1: CardContentItem[], card2: CardContentItem[]): boolean {
        return card1.length === card2.length && card1.every((c, i) => {
            const c2 = card2[i];
            return c.text === c2.text;
        });
    }
    addNewCard(): any {
        const newCard: Card = {
            id: StringUtils.rndStrWithPref("c-", 3),
            contentFront: [],
            contentBack: [],
            lastTimeEdited: new Date().toISOString(),
            creationTime: new Date().toISOString(),
        };
        this.#plant.deck.cards.push(newCard);
        this.selectCard(newCard.id);
    }
    async reloadCard(cardId: string) {
        this.#cardEditingState = { state: "CardReloading", cardId };
        const response = await Backend.fetchJsonResponse<Card>(
            `/plants/${this.#plant.id}/cards/${cardId}/load`,
            { method: "GET" }
        );
        if (!response.isSuccess) {
            this.#cardEditingState = { state: "ExpectedCardNotFound", cardId };
            return;
        }
        const card = response.data;
        this.#cardEditingState = {
            state: "CardEditing",
            card: {
                ...card,
                contentFront: card.contentFront.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cf-${i}-`, 3)
                }
                )),
                contentBack: card.contentBack.map((c, i) => ({
                    ...c,
                    stringId: StringUtils.rndStrWithPref(`cb-${i}-`, 3)
                }))
            }
        };
    }
}
export type CardContentSide = 'contentFront' | 'contentBack';

export type CardContentWithStringId = (CardContentItem & { stringId: string });

export type CardViewToEdit = Omit<Card, CardContentSide> & {
    contentFront: CardContentWithStringId[]
    contentBack: CardContentWithStringId[]
}

export type CardPreview = {
    frontTextPreview: string
    backTextPreview: string
    id: string
    number: number
}