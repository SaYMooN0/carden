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
    cardsCount: number
    creationDate: string
    studyProgress: number
}