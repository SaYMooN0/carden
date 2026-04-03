export const AllPlantSpicies = [
    "Cactus",
    "McPitcherPlant"
] as const
export type PlantSpecie = typeof AllPlantSpicies[number];

export const AllPotTypes = [
    "CeramicWithSun",
    "PVZ"
] as const
export type PotType = typeof AllPotTypes[number];


