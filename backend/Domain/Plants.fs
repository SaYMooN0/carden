module Domain.Plants

open System
open Domain.CardContentItem
open Domain.PlantName
open Domain.Study


[<RequireQualifiedAccess>]
type CardStudyAnswer =
    | Again
    | Hard
    | Good
    | Easy


type LearningCardStudyState =
    { DueAt: DateTimeOffset
      LearningStepIndex: int
      StartedAt: DateTimeOffset }


type ReviewCardStudyState =
    { DueAt: DateTimeOffset
      ReviewInterval: TimeSpan
      LastReviewedAt: DateTimeOffset }


type CardStudyState =
    | Learning of LearningCardStudyState
    | Review of ReviewCardStudyState

module CardStudyState =
    let createLearning (dueAt: DateTimeOffset) (learningStepIndex: int) (startedAt: DateTimeOffset) : CardStudyState =
        Learning
            { DueAt = dueAt
              LearningStepIndex = learningStepIndex
              StartedAt = startedAt }

    let createReview
        (dueAt: DateTimeOffset)
        (reviewInterval: TimeSpan)
        (lastReviewedAt: DateTimeOffset)
        : CardStudyState =
        Review
            { DueAt = dueAt
              ReviewInterval = reviewInterval
              LastReviewedAt = lastReviewedAt }

    let dueAt =
        function
        | Learning state -> state.DueAt
        | Review state -> state.DueAt

    let isDue (now: DateTimeOffset) (studyState: CardStudyState) : bool = dueAt studyState <= now





type CardId = CardId of Guid

module CardId =
    let value (CardId value) = value


type Card =
    private
        { Id: CardId
          ContentFront: CardContentItem list
          ContentBack: CardContentItem list
          StudyState: CardStudyState option
          LastTimeEdited: DateTimeOffset
          CreationTime: DateTimeOffset }

module Card =
    [<RequireQualifiedAccess>]
    type CardCreationErr =
        | EmptyFrontContent
        | EmptyBackContent

    let tryCreate
        (id: CardId)
        (contentFront: CardContentItem list)
        (contentBack: CardContentItem list)
        (now: DateTimeOffset)
        : Result<Card, CardCreationErr> =
        if List.isEmpty contentFront then
            Error CardCreationErr.EmptyFrontContent
        elif List.isEmpty contentBack then
            Error CardCreationErr.EmptyBackContent
        else
            Ok
                { Id = id
                  ContentFront = contentFront
                  ContentBack = contentBack
                  StudyState = None
                  CreationTime = now
                  LastTimeEdited = now }

    let tryUpdateContent
        (contentFront: CardContentItem list)
        (contentBack: CardContentItem list)
        (now: DateTimeOffset)
        (card: Card)
        : Result<Card, CardCreationErr> =
        if List.isEmpty contentFront then
            Error CardCreationErr.EmptyFrontContent
        elif List.isEmpty contentBack then
            Error CardCreationErr.EmptyBackContent
        else
            Ok
                { card with
                    ContentFront = contentFront
                    ContentBack = contentBack
                    LastTimeEdited = now }

    let withStudyState (studyState: CardStudyState option) (card: Card) : Card = { card with StudyState = studyState }

    let id (card: Card) = card.Id
    let contentFront (card: Card) = card.ContentFront
    let contentBack (card: Card) = card.ContentBack
    let studyState (card: Card) = card.StudyState
    let lastTimeEdited (card: Card) = card.LastTimeEdited
    let creationTime (card: Card) = card.CreationTime


type DeckId = DeckId of Guid

module DeckId =
    let value (DeckId value) = value


type Deck =
    { Id: DeckId
      Cards: Card list
      LastTimeEdited: DateTimeOffset }


type PlantSpecieName =
    | Cactus
    | McPitcherPlant

module PlantSpecieName =
    let value =
        function
        | Cactus -> "Cactus"
        | McPitcherPlant -> "McPitcherPlant"

    let tryCreate (raw: string | null) : Result<PlantSpecieName, unit> =
        let value = if isNull raw then "" else raw.Trim()

        match value with
        | "Cactus" -> Ok Cactus
        | "McPitcherPlant" -> Ok McPitcherPlant
        | _ -> Error()


type PotTypeName =
    | Basic
    | PVZ

module PotTypeName =
    let value =
        function
        | Basic -> "Basic"
        | PVZ -> "PVZ"

    let tryCreate (raw: string | null) : Result<PotTypeName, unit> =
        let value = if isNull raw then "" else raw.Trim()

        match value with
        | "Basic" -> Ok Basic
        | "PVZ" -> Ok PVZ
        | _ -> Error()

type PlantId = PlantId of Guid

module PlantId =
    let value (PlantId value) = value


type AppUserId = AppUserId of Guid

module AppUserId =
    let value (AppUserId g) = g




type StudySessionCards =
    { ReviewCards: Card list
      NewCards: Card list }


type Plant =
    { Id: PlantId
      OwnerId: AppUserId
      Name: PlantName
      Deck: Deck
      CreationDate: DateTimeOffset
      PotType: PotTypeName
      PlantSpecie: PlantSpecieName
      }

module Plant =
    let createNew ownerId name (now: DateTimeOffset) potType plantSpecie : Plant =
        { Id = PlantId(Guid.CreateVersion7())
          OwnerId = ownerId
          Name = name
          Deck =
            { Id = DeckId(Guid.CreateVersion7())
              Cards = []
              LastTimeEdited = now }
          CreationDate = now
          PotType = potType
          PlantSpecie = plantSpecie }


    let private getCardsDueForReview (now: DateTimeOffset) (plant: Plant) : Card list =
        plant.Deck.Cards
        |> List.choose (fun card ->
            match Card.studyState card with
            | Some studyState when CardStudyState.isDue now studyState -> Some(card, CardStudyState.dueAt studyState)
            | _ -> None)
        |> List.sortBy snd
        |> List.map fst
        |> List.truncate StudyConstants.ReviewCardsPerSession

    let private getNewCardsForStudySession (plant: Plant) : Card list =
        plant.Deck.Cards
        |> List.filter (fun card -> Card.studyState card |> Option.isNone)
        |> List.sortBy Card.creationTime
        |> List.truncate StudyConstants.NewCardsPerSession

    let tryGetStudySessionCards
        (now: DateTimeOffset)
        (plant: Plant)
        : StudySessionCards =
        { ReviewCards = getCardsDueForReview now plant
          NewCards = getNewCardsForStudySession plant }
