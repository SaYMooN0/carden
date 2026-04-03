import type { PotType, PlantSpecie } from "$lib/ts/base-types"

export type CardContentItem = { text: string }
export type Card = {
    id: string
    contentFront: CardContentItem[]
    contentBack: CardContentItem[]
    lastTimeEdited: string
    creationTime: string
}
export type Deck = {
    id: string
    cards: Card[]
    lastTimeEdited: string
}

export type PlantToEdit = {
    id: string
    name: string
    description: string
    deck: Deck
    creationDate: string
    potType: PotType
    plantSpecie: PlantSpecie
}