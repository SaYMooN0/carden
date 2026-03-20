export type PlantId = string
export type PlantSpecie = "Cactus" | "McPitcherPlant";
export type PotType = "CeramicWithSun" | "PVZ";

export type PlantPreview = {
    id: PlantId
    name: string
    plantSpecie: PlantSpecie
    potType: PotType
    cardsCount: number
    creationDate: string
    plantGrothPoints: number
}