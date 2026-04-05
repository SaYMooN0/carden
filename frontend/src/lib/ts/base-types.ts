export const AllPlantSpicies = [
    "Cactus",
    "McPitcherPlant"
] as const
export type PlantSpecie = typeof AllPlantSpicies[number];

export const AllPotTypes = [
    "Coral",
    "PVZ"
] as const
export type PotType = typeof AllPotTypes[number];


