module Domain.Study

open System

type StudyAnswerDifficulty =
    | Again
    | Hard
    | Good
    | Easy

module StudySettings =
    [<Literal>]
    let LearningAgainDelaySeconds = 60

    [<Literal>]
    let LearningHardDelaySeconds = 5 * 60

    [<Literal>]
    let LearningGoodDelaySeconds = 10 * 60

    [<Literal>]
    let LearningEasyIntervalSeconds = 2 * 24 * 60 * 60

    [<Literal>]
    let ReviewAgainDelaySeconds = 10 * 60

    [<Literal>]
    let ReviewHardIntervalMultiplier = 1.2

    [<Literal>]
    let ReviewGoodIntervalMultiplier = 2.0

    [<Literal>]
    let ReviewEasyIntervalMultiplier = 3.0

    [<Literal>]
    let NewCardsPerSession = 20

    [<Literal>]
    let ReviewCardsPerSession = 100



module StudyConstants =
    [<Literal>]
    let NewCardsPerSession = 10

    [<Literal>]
    let ReviewCardsPerSession = 20

    let LearningAgainDelay: TimeSpan = TimeSpan.FromMinutes(1.)
    let LearningHardDelay = TimeSpan.FromMinutes(5.)
    let LearningGoodDelay = TimeSpan.FromMinutes(10.)
    let LearningEasyInterval = TimeSpan.FromDays(3.)

    let ReviewAgainDelay = TimeSpan.FromMinutes(10.)
    let ReviewHardIntervalMultiplier = 1.2
    let ReviewGoodIntervalMultiplier = 2.0
    let ReviewEasyIntervalMultiplier = 3.0
