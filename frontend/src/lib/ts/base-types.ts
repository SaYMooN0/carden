export type PlantId = string
export const AllPlantSpicies = ["Cactus", "McPitcherPlant"] as const
export type PlantSpecie = typeof AllPlantSpicies[number];
export const AllPotTypes = ["CeramicWithSun", "PVZ"] as const
export type PotType = typeof AllPotTypes[number];

export type PlantPreview = {
    id: PlantId
    name: string
    plantSpecie: PlantSpecie
    potType: PotType
    creationDate: string
    cardsCount: number
    studyProgress: number
}
export type CardContentItem =
    | { type: "TextContentItem", text: string }
    | { type: "ImageContentItem", image: string }
    | { type: "MathAjaxContentItem", expression: string }
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

export type Plant = {
    id: PlantId
    name: string
    description: string
    deck: Deck
    creationDate: string
    potType: PotType
    plantSpecie: PlantSpecie
}