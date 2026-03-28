import type { Card, CardContentItem, Plant } from "$lib/ts/base-types";
import { StringUtils } from "$lib/ts/utils/string-utils";

export class EditPlantPageState {

    plant: Plant = $state()!;
    #cardEditingState: CardEditingState = $state({ state: "NoCardSelected" });

    get cardEditingState() {
        return this.#cardEditingState;
    }
    constructor(plant: Plant) {
        this.plant = plant;
        this.#cardEditingState = { state: "NoCardSelected" };
    }
    selectCard(cardId: string) {
        const card = this.plant.deck.cards.find(c => c.id === cardId);
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
    anyUnsavedChangesOnTheCurrentCard(): boolean {
        if (this.#cardEditingState.state !== "CardEditing") {
            return false;
        }
        const card = this.#cardEditingState.card;
        const originalCard = this.plant.deck.cards.find(c => c.id === card.id);
        if (!originalCard) {
            return false;
        }
        return this.checkIfCardContentItemsListsEqual(card.contentFront, originalCard.contentFront)
            && this.checkIfCardContentItemsListsEqual(card.contentBack, originalCard.contentBack);
    }
    saveCurrentCardChanges(): { unexpectedErr: string | null } {
        return { unexpectedErr: null };
    }
    checkIfCardContentItemsListsEqual(card1: CardContentItem[], card2: CardContentItem[]): boolean {
        return card1.length === card2.length && card1.every((c, i) => {
            const c2 = card2[i];
            if (c.type !== c2.type) {
                return false;
            }
            if (c.type === "TextContentItem" && c2.type === "TextContentItem") {
                return c.text === c2.text;
            }
            if (c.type === "ImageContentItem" && c2.type === "ImageContentItem") {
                return c.image === c2.image;
            }
            if (c.type === "MathAjaxContentItem" && c2.type === "MathAjaxContentItem") {
                return c.expression === c2.expression;
            }
            return false;
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
        this.plant.deck.cards.push(newCard);
        this.selectCard(newCard.id);
    }
}
type CardEditingState =
    | { state: "NoCardSelected"; }
    | { state: "ExpectedCardNotFound"; cardId: string; }
    | { state: "CardEditing"; card: CardViewToEdit; }
    | { state: "CardReloading"; cardId: string; }
export type CardContentWithStringId = (CardContentItem & { stringId: string });

export type CardViewToEdit = Omit<Card, 'contentFront' | 'contentBack'> & {
    contentFront: CardContentWithStringId[]
    contentBack: CardContentWithStringId[]
}
