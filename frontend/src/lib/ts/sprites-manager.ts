import type { PlantSpecie, PotType } from "./base-types";

export namespace SpritesManager {
    export function getLvl5PlantSprite(specie: PlantSpecie): string {
        return `/sprites/plants/${specie}/${specie}_5.png`;
    }
    export function getPotSprite(potType: PotType): string {
        return `/sprites/pots/${potType}.png`;
    }
}