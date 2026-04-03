
export type StudyDeckLoadResponse = {
    newCards: StudyLoadedCardResponse[];
    plant: StudyPlantResponse;
    reviewCards: StudyLoadedCardResponse[];
    serverNow: string;
    studySettings: StudySettings;
};

export type StudyPlantResponse = {
    id: string;
    name: string;
    deckId: string;
    deckLastTimeEdited: string;
    creationDate: string;
    potTypeName: string;
    plantSpecieName: string;
    lastCompletedStudyDay: string | null;
};

export type StudyLoadedCardResponse = {
    id: string;
    contentFront: string[];
    contentBack: string[];
    creationTime: string;
    lastTimeEdited: string;
    studyDueAt: string | null;
    studyLastReviewedAt: string | null;
    studyLearningStepIndex: number | null;
    studyReviewIntervalSeconds: number | null;
    studyStartedAt: string | null;
    studyStateType: 'Learning' | 'Review' | null;
};

export type StudySettings = {
    learningAgainDelaySeconds: number;
    learningEasyIntervalSeconds: number;
    learningGoodDelaySeconds: number;
    learningHardDelaySeconds: number;
    newCardsPerDay: number;
    reviewAgainDelaySeconds: number;
    reviewCardsPerDay: number;
    reviewEasyIntervalMultiplier: number;
    reviewGoodIntervalMultiplier: number;
    reviewHardIntervalMultiplier: number;
};
