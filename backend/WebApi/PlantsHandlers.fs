module WebApi.PlantsHandlers

open System
open System.Net
open Domain.CardContentItem
open Domain.Plants
open Domain.Study
open Microsoft.AspNetCore.Http
open WebApi.AppUsersRepository
open WebApi.BackendResponse
open WebApi.BackendResponse.BackendResponseErr
open WebApi.JwtToken
open WebApi.PlantsRepositories
open Giraffe
open WebApi.RepositoriesShared
open WebApi.Requests
open WebApi.Responses
open WebApi.Validation

let private tokenCookieName = "user_tkn"

let private getAuthCookie (ctx: HttpContext) : Result<JwtToken, unit> =
    let exist, cookie = ctx.Request.Cookies.TryGetValue tokenCookieName
    if not exist then Error() else Ok(JwtToken cookie)

let private withAuthenticatedUser (handler: AppUserId -> HttpHandler) : HttpHandler =
    fun next ctx ->
        match getAuthCookie ctx with
        | Error _ ->
            constructFailure
                HttpStatusCode.Unauthorized
                [ BackendResponseErr.create "Authentication token missing. Please log in again." ]
                next
                ctx

        | Ok token ->
            match ctx.GetService<JwtTokenService>().UserIdFromJwtToken token with
            | Error InvalidToken ->
                constructFailure
                    HttpStatusCode.Unauthorized
                    [ BackendResponseErr.create "Invalid authentication token" ]
                    next
                    ctx

            | Error Unexpected ->
                constructFailure
                    HttpStatusCode.InternalServerError
                    [ BackendResponseErr.create "Unexpected token parsing error" ]
                    next
                    ctx

            | Ok userId ->
                task {
                    use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                    let usersRepo = ctx.GetService<AppUsersRepository>()
                    let! exists = usersRepo.AnyUserWithId dbConn userId

                    if exists then
                        return! handler userId next ctx
                    else
                        return!
                            constructFailure
                                HttpStatusCode.Unauthorized
                                [ BackendResponseErr.create "Account not found. Please log in again." ]
                                next
                                ctx
                }

let private tryGetQueryValue (ctx: HttpContext) (key: string) : string option =
    match ctx.Request.Query.TryGetValue key with
    | true, value ->
        let valueStr = string value

        if String.IsNullOrWhiteSpace valueStr then
            None
        else
            Some valueStr
    | _ -> None

let private toIsoString (value: DateTimeOffset) = value.ToString("O")

let private nullableDateTimeOffsetToIsoString (value: Nullable<DateTimeOffset>) =
    if value.HasValue then value.Value.ToString("O") else null

let private nullableDateOnlyToString (value: Nullable<DateOnly>) =
    if value.HasValue then
        value.Value.ToString("yyyy-MM-dd")
    else
        null

let private studyCardDbDtoToResponse (card: StudyPlantCardDbDto) =
    {| Id = card.Id
       ContentFront = card.ContentFront
       ContentBack = card.ContentBack
       StudyStateType = card.StudyStateType
       StudyDueAt = nullableDateTimeOffsetToIsoString card.StudyDueAt
       StudyLearningStepIndex =
        if card.StudyLearningStepIndex.HasValue then
            Nullable(card.StudyLearningStepIndex.Value)
        else
            Nullable()
       StudyStartedAt = nullableDateTimeOffsetToIsoString card.StudyStartedAt
       StudyReviewIntervalSeconds =
        if card.StudyReviewIntervalSeconds.HasValue then
            Nullable(int64 card.StudyReviewIntervalSeconds.Value)
        else
            Nullable()
       StudyLastReviewedAt = nullableDateTimeOffsetToIsoString card.StudyLastReviewedAt
       LastTimeEdited = toIsoString card.LastTimeEdited
       CreationTime = toIsoString card.CreationTime |}

let private mapStudyPlant (plant: StudyPlantDetailsDbDto) =
    {| Id = plant.Id
       Name = plant.Name
       DeckId = plant.DeckId
       DeckLastTimeEdited = toIsoString plant.DeckLastTimeEdited
       CreationDate = toIsoString plant.CreationDate
       PotTypeName = plant.PotTypeName
       PlantSpecieName = plant.PlantSpecieName
       CardsCount = plant.CardsCount
       CompletedStudySessionsCount = plant.CompletedStudySessionsCount |}

let private mapStudyCard (card: StudyPlantCardDbDto) =
    {| Id = card.Id
       ContentFront = card.ContentFront
       ContentBack = card.ContentBack
       StudyStateType = card.StudyStateType
       StudyDueAt = nullableDateTimeOffsetToIsoString card.StudyDueAt
       StudyLearningStepIndex =
        if card.StudyLearningStepIndex.HasValue then
            Nullable(card.StudyLearningStepIndex.Value)
        else
            Nullable()
       StudyStartedAt = nullableDateTimeOffsetToIsoString card.StudyStartedAt
       StudyReviewIntervalSeconds =
        if card.StudyReviewIntervalSeconds.HasValue then
            Nullable(int64 card.StudyReviewIntervalSeconds.Value)
        else
            Nullable()
       StudyLastReviewedAt = nullableDateTimeOffsetToIsoString card.StudyLastReviewedAt
       LastTimeEdited = toIsoString card.LastTimeEdited
       CreationTime = toIsoString card.CreationTime |}

let studySettingsResponse =
    {| learningAgainDelaySeconds = StudySettings.LearningAgainDelaySeconds
       learningEasyIntervalSeconds = StudySettings.LearningEasyIntervalSeconds
       learningGoodDelaySeconds = StudySettings.LearningGoodDelaySeconds
       learningHardDelaySeconds = StudySettings.LearningHardDelaySeconds
       newCardsPerSession = StudySettings.NewCardsPerSession
       reviewAgainDelaySeconds = StudySettings.ReviewAgainDelaySeconds
       reviewCardsPerSession = StudySettings.ReviewCardsPerSession
       reviewEasyIntervalMultiplier = StudySettings.ReviewEasyIntervalMultiplier
       reviewGoodIntervalMultiplier = StudySettings.ReviewGoodIntervalMultiplier
       reviewHardIntervalMultiplier = StudySettings.ReviewHardIntervalMultiplier |}

let studyLoadResponse (now: DateTimeOffset) (dto: LoadStudySessionDbDto) =
    {| serverNow = now.ToString("O")
       plant = mapStudyPlant dto.Plant
       reviewCards = dto.ReviewCards |> List.map mapStudyCard
       newCards = dto.NewCards |> List.map mapStudyCard
       studySettings = studySettingsResponse
       loadedAtCompletedStudySessionsCount = dto.Plant.CompletedStudySessionsCount |}


let handleLoadMyPlants: HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            let sortByRaw = tryGetQueryValue ctx "sortBy"
            let directionRaw = tryGetQueryValue ctx "direction"

            match ParsedGetMyDecksRequest.parse sortByRaw directionRaw with
            | Error errs -> return! constructFailure HttpStatusCode.BadRequest errs next ctx
            | Ok parsed ->
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()
                let! items = repo.GetPreviewsByOwner dbConn userId parsed.SortBy parsed.Direction

                let response = items |> List.map PlantPreviewResponse.fromDomain
                return! constructSuccess response HttpStatusCode.OK next ctx
        })

let handleLoadMyPlant (plantId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<PlantsRepository>()

            let! plantOpt = repo.GetByIdForOwner dbConn userId (PlantId plantId)

            match plantOpt with
            | None ->
                return!
                    constructFailure
                        HttpStatusCode.NotFound
                        [ BackendResponseErr.create "Plant not found or access denied." ]
                        next
                        ctx
            | Some plant ->
                let response = PlantResponse.fromDbDto plant
                return! constructSuccess response HttpStatusCode.OK next ctx
        })

let handleLoadStudy (plantId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<PlantsRepository>()
            let now = DateTimeOffset.UtcNow

            let! result = repo.GetStudySessionForOwner dbConn userId (PlantId plantId) now

            match result with
            | LoadStudySessionResult.PlantNotFoundOrAccessDenied ->
                return!
                    constructFailure HttpStatusCode.NotFound [ create "Plant not found or access denied." ] next ctx

            | LoadStudySessionResult.NotEnoughCardsToStudy(cardsCount, plantId) ->
                return!
                    constructFailure
                        HttpStatusCode.Conflict
                        [ create "Not enough cards to study."
                          |> SetExtraData.notEnoughCardsToStudy cardsCount plantId ]
                        next
                        ctx
            | LoadStudySessionResult.Loaded dto ->
                return! constructSuccess (studyLoadResponse now dto) HttpStatusCode.OK next ctx
        })

let handleCreatePlant: HttpHandler =
    withAuthenticatedUser (fun userId ->
        withValidatedBody RawCreatePlantDeckRequest.parse (fun req next ctx ->
            task {
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()

                let plant: Plant =
                    Plant.createNew userId req.Name DateTimeOffset.UtcNow req.PotType req.PlantSpecie

                let! createRes = repo.InsertNewPlant dbConn plant

                return!
                    match createRes with
                    | Ok() -> constructSuccess {| Id = PlantId.value plant.Id |} HttpStatusCode.OK next ctx
                    | Error msg -> constructFailure HttpStatusCode.InternalServerError [ create msg ] next ctx
            }))

let handleReloadPlantCard (plantId: Guid) (cardId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<PlantsRepository>()

            let! cardOpt = repo.GetCardByIdForPlantOwner dbConn userId (PlantId plantId) cardId

            match cardOpt with
            | None ->
                return! constructFailure HttpStatusCode.NotFound [ create "Card not found or access denied." ] next ctx
            | Some card -> return! constructSuccess (CardResponse.fromDbDto card) HttpStatusCode.OK next ctx
        })

let handleSavePlantCard (plantId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId ->
        withValidatedBody RawUpsertPlantCardRequest.parse (fun req next ctx ->
            task {
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()

                let saveReq =
                    { TargetCardId =
                        match req.TargetCard with
                        | CreateNewCard -> None
                        | UpdateExistingCard cardId -> Some cardId
                      ContentFront = req.ContentFront |> List.map CardContentItem.value |> List.toArray
                      ContentBack = req.ContentBack |> List.map CardContentItem.value |> List.toArray
                      Now = DateTimeOffset.UtcNow }

                let! saveRes = repo.SaveCardForPlantOwner dbConn userId (PlantId plantId) saveReq

                return!
                    match saveRes with
                    | Error msg -> constructFailure HttpStatusCode.InternalServerError [ create msg ] next ctx

                    | Ok SavePlantCardResult.PlantNotFoundOrAccessDenied ->
                        constructFailure
                            HttpStatusCode.NotFound
                            [ create "Plant not found or access denied." ]
                            next
                            ctx

                    | Ok SavePlantCardResult.CardNotFoundOrAccessDenied ->
                        constructFailure
                            HttpStatusCode.NotFound
                            [ create "Card not found or access denied." ]
                            next
                            ctx

                    | Ok(SavePlantCardResult.Saved savedCard) ->
                        constructSuccess (CardResponse.fromDbDto savedCard) HttpStatusCode.OK next ctx
            }))

let handleDeletePlantCard (plantId: Guid) (cardId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<PlantsRepository>()
            let now = DateTimeOffset.UtcNow

            let! deleteRes = repo.DeleteCardForPlantOwner dbConn userId (PlantId plantId) cardId now

            return!
                match deleteRes with
                | Error msg -> constructFailure HttpStatusCode.InternalServerError [ create msg ] next ctx
                | Ok DeletePlantCardResult.PlantNotFoundOrAccessDenied ->
                    constructFailure HttpStatusCode.NotFound [ create "Plant not found or access denied." ] next ctx
                | Ok DeletePlantCardResult.CardNotFoundOrAccessDenied ->
                    constructFailure HttpStatusCode.NotFound [ create "Card not found or access denied." ] next ctx
                | Ok DeletePlantCardResult.Deleted ->
                    constructSuccess {| DeletedCardId = cardId |} HttpStatusCode.OK next ctx
        })

type CompleteStudySessionResponse = { CompletedStudySessionsCount: int }

let handleCompleteStudySession (plantId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        withValidatedBody
            RawCompleteStudySessionRequest.parse
            (fun request next ctx ->
                task {
                    use conn = ctx.GetService<ConnectionFactory>().CreateConnection()
                    let repo = ctx.GetService<PlantsRepository>()

                    try
                        do! conn.OpenAsync()
                        let! tx = conn.BeginTransactionAsync()
                        use tx = tx

                        let distinctCardIds = request.AnswerEvents |> List.map _.CardId |> List.distinct

                        let! loadResult =
                            repo.GetStudyCardsForCompletionForUpdate conn tx userId (PlantId plantId) distinctCardIds

                        match loadResult with
                        | Error err ->
                            do! tx.RollbackAsync()
                            return! constructFailure HttpStatusCode.InternalServerError [ create err ] next ctx

                        | Ok PlantNotFoundOrAccessDenied ->
                            do! tx.RollbackAsync()

                            return!
                                constructFailure
                                    HttpStatusCode.NotFound
                                    [ create "Plant not found or access denied." ]
                                    next
                                    ctx

                        | Ok(Success cardsFromDb) ->
                            let statesByCardId =
                                cardsFromDb
                                |> List.map (fun card ->
                                    card.Id, PersistedCardStudyState.fromDb request.SessionStartedAt card)
                                |> Map.ofList

                            let answeredCardIds = request.AnswerEvents |> List.map _.CardId |> Set.ofList

                            let missingCardIds =
                                answeredCardIds
                                |> Set.filter (fun cardId -> not (statesByCardId.ContainsKey cardId))
                                |> Set.toList

                            if not missingCardIds.IsEmpty then
                                do! tx.RollbackAsync()

                                return!
                                    constructFailure
                                        HttpStatusCode.BadRequest
                                        [ create "Some answered cards were not found or do not belong to this plant." ]
                                        next
                                        ctx
                            else
                                let orderedEvents = request.AnswerEvents |> List.sortBy _.AnsweredAtOffsetMs

                                let finalStates =
                                    orderedEvents
                                    |> List.fold
                                        (fun (states: Map<Guid, PersistedCardStudyState>) event ->
                                            let previousState = states |> Map.find event.CardId

                                            let answeredAt =
                                                request.SessionStartedAt.AddMilliseconds(
                                                    float event.AnsweredAtOffsetMs
                                                )

                                            let nextState =
                                                StudyStateTransitions.calculateNextState
                                                    previousState
                                                    event.Difficulty
                                                    answeredAt

                                            states |> Map.add event.CardId nextState)
                                        statesByCardId

                                let updatedCards =
                                    finalStates
                                    |> Map.toList
                                    |> List.choose (fun (cardId, state) ->
                                        if answeredCardIds.Contains cardId then
                                            Some(PersistedCardStudyState.toDbDto cardId state)
                                        else
                                            None)

                                let! saveResult =
                                    repo.SaveCompletedStudySession conn tx userId (PlantId plantId) updatedCards

                                match saveResult with
                                | Error err ->
                                    do! tx.RollbackAsync()

                                    return!
                                        constructFailure HttpStatusCode.InternalServerError [ create err ] next ctx

                                | Ok completedStudySessionsCount ->
                                    do! tx.CommitAsync()

                                    return!
                                        constructSuccess
                                            { CompletedStudySessionsCount = completedStudySessionsCount }
                                            HttpStatusCode.OK
                                            next
                                            ctx
                    with ex ->
                        return! constructFailure HttpStatusCode.InternalServerError [ create ex.Message ] next ctx
                })
            next
            ctx)

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET >=> route "/load-all" >=> handleLoadMyPlants
          POST >=> route "/create" >=> handleCreatePlant
          GET >=> routef "/%O/load-study" (fun plantId -> handleLoadStudy plantId)
          GET >=> routef "/%O/load" (fun plantId -> handleLoadMyPlant plantId)
          GET
          >=> routef "/%O/cards/%O/load" (fun (plantId, cardId) -> handleReloadPlantCard plantId cardId)
          PATCH >=> routef "/%O/save-card" (fun plantId -> handleSavePlantCard plantId)
          DELETE
          >=> routef "/%O/cards/%O/delete" (fun (plantId, cardId) -> handleDeletePlantCard plantId cardId)
          POST >=> routef "/%O/study/complete" handleCompleteStudySession ]
