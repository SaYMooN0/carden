import type { PlantSpecie, PotType } from "./base-types";

export namespace SpritesManager {
    const plantGrowthLevels = [1, 2, 3, 4, 5] as const;
    export function getLvl5PlantSprite(specie: PlantSpecie): string {
        return `/sprites/plants/${specie}/${specie}_5.png`;
    }
    function getPlantSpriteForLvl(specie: PlantSpecie, level: typeof plantGrowthLevels[number]): string {
        return `/sprites/plants/${specie}/${specie}_${level}.png`;
    }

    const plantAttachmentIndexes = [1, 2, 3, 4, 5] as const;
    function getPlantAttachmentSpriteForLvl(specie: PlantSpecie, level: typeof plantAttachmentIndexes[number]): string {
        return `/sprites/plants/${specie}/${specie}_att_${level}.png`;
    }



    const potLevels = [1, 2, 3] as const;
    function getPotSpriteForLvl(potType: PotType, level: typeof potLevels[number]): string {
        return `/sprites/pots/${potType}/${potType}_${level}.png`;
    }
    export function getLvl3PotSprite(potType: PotType): string {
        return `/sprites/pots/${potType}/${potType}_3.png`;
    }
    function calculatePotSpriteLvlBasedOnCardsCount(cardsCount: number): typeof potLevels[number] {
        const lvl = Math.ceil(cardsCount / 15);
        if (lvl > 3) {
            return 3;
        }
        if (lvl < 1) {
            return 1;
        }
        return lvl as typeof potLevels[number];
    }

    export const potsYOffset: Record<PotType, Record<typeof potLevels[number], number>> = {
        "Coral": { 1: 12, 2: 12, 3: 12 },
        "PVZ": { 1: 35, 2: 35, 3: 35 }
    }
    export function getPotYOffsetBasedOnCardsCount(potType: PotType, cardsCount: number): number {
        const lvl = calculatePotSpriteLvlBasedOnCardsCount(cardsCount);
        return potsYOffset[potType][lvl];
    }



    export function calculatePlantSpritesBasedOnStudyProgress(
        specie: PlantSpecie,
        studyProgress: number
    ): { mainSprite: string, attachments: string[] } {
        if (studyProgress < 5) {
            const level = (studyProgress + 1) as typeof plantGrowthLevels[number];
            return { mainSprite: getPlantSpriteForLvl(specie, level), attachments: [] };
        }
        const level: typeof plantGrowthLevels[number] = 5;
        const attachments: string[] = [];
        const attachmentFlags = studyProgress - 5;

        plantAttachmentIndexes.forEach((attIndex, i) => {
            if ((attachmentFlags >> i) & 1) {
                attachments.push(getPlantAttachmentSpriteForLvl(specie, attIndex));
            }
        });

        return { mainSprite: getPlantSpriteForLvl(specie, level), attachments: attachments };
    }

    export function calculatePotSpryteBasedOnCardsCount(potType: PotType, cardsCount: number): string {
        const lvl = calculatePotSpriteLvlBasedOnCardsCount(cardsCount);
        return getPotSpriteForLvl(potType, lvl);
    }

}