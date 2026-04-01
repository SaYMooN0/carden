module WebApi.PlantsHandlers

open System
open System.Net
open Domain.CardContentItem
open Domain.Plants
open Microsoft.AspNetCore.Http
open WebApi.AppUsersRepository
open WebApi.BackendResponse
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
                    | Error msg ->
                        constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx
            }))

let handleReloadPlantCard (plantId: Guid) (cardId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<PlantsRepository>()

            let! cardOpt = repo.GetCardByIdForPlantOwner dbConn userId (PlantId plantId) cardId

            match cardOpt with
            | None ->
                return!
                    constructFailure
                        HttpStatusCode.NotFound
                        [ BackendResponseErr.create "Card not found or access denied." ]
                        next
                        ctx
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
                    | Error msg ->
                        constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx

                    | Ok SavePlantCardResult.PlantNotFoundOrAccessDenied ->
                        constructFailure
                            HttpStatusCode.NotFound
                            [ BackendResponseErr.create "Plant not found or access denied." ]
                            next
                            ctx

                    | Ok SavePlantCardResult.CardNotFoundOrAccessDenied ->
                        constructFailure
                            HttpStatusCode.NotFound
                            [ BackendResponseErr.create "Card not found or access denied." ]
                            next
                            ctx

                    | Ok(SavePlantCardResult.Saved savedCard) ->
                        constructSuccess (CardResponse.fromDbDto savedCard) HttpStatusCode.OK next ctx
            }))

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET >=> route "/load-all" >=> handleLoadMyPlants
          POST >=> route "/create" >=> handleCreatePlant
          GET >=> routef "/%O/load" (fun plantId -> handleLoadMyPlant plantId)
          GET
          >=> routef "/%O/cards/%O/load" (fun (plantId, cardId) -> handleReloadPlantCard plantId cardId)
          PATCH >=> routef "/%O/save-card" (fun plantId -> handleSavePlantCard plantId) ]
