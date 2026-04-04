module Domain.StudySettings

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
    let NewCardsPerDay = 20

    [<Literal>]
    let ReviewCardsPerDay = 100

    type StudyAnswerDifficulty =
        | Again
        | Hard
        | Good
        | Easy
