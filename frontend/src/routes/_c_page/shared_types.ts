import type { PlantSpecie, PotType } from "$lib/ts/base-types"

export type PlantPreview = {
    id: string
    name: string
    plantSpecie: PlantSpecie
    potType: PotType
    creationDate: string
    cardsCount: number
    studyProgress: number
}
