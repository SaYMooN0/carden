module WebApi.PlantsHandlers

open System
open System.Net
open System.Text.Json
open Domain.Models
open Microsoft.AspNetCore.Http
open WebApi.BackendResponse
open WebApi.JwtToken
open WebApi.Repositories
open Giraffe
open WebApi.Contracts
open WebApi.Validation

type PlantPreviewResponse =
    { Id: Guid
      Name: string
      PlantSpecie: string
      PotType: string
      CardsCount: int
      CreationDate: string
      StudyProgress: int }

module PlantPreviewResponse =
    let fromDomain (item: PlantPreviewDto) : PlantPreviewResponse =
        { Id = PlantId.value item.Id
          Name = PlantName.value item.Name
          PlantSpecie = PlantSpecieName.value item.PlantSpecie
          PotType = PotTypeName.value item.PotType
          CardsCount = item.CardsCount
          CreationDate = item.CreationDate.ToIsoString()
          StudyProgress = 1 }

type CardResponse =
    { Id: Guid
      ContentFront: JsonElement
      ContentBack: JsonElement
      LastTimeEdited: string
      CreationTime: string }

type DeckResponse =
    { Id: Guid
      Cards: CardResponse list
      LastTimeEdited: string }

type PlantResponse =
    { Id: Guid
      Name: string
      Description: string
      Deck: DeckResponse
      CreationDate: string
      PotType: string
      PlantSpecie: string }

module PlantResponse =
    let private parseJsonElement (json: string) : JsonElement =
        use doc = JsonDocument.Parse(json)
        doc.RootElement.Clone()

    let fromDbDto (dto: PlantWithCardsDbDto) : PlantResponse =
        { Id = dto.Plant.Id
          Name = dto.Plant.Name
          Description = dto.Plant.Description
          Deck =
            { Id = dto.Plant.DeckId
              LastTimeEdited = dto.Plant.DeckLastTimeEdited.ToIsoString()
              Cards =
                dto.Cards
                |> List.map (fun c ->
                    { Id = c.Id
                      ContentFront = parseJsonElement c.ContentFrontJson
                      ContentBack = parseJsonElement c.ContentBackJson
                      LastTimeEdited = c.LastTimeEdited.ToIsoString()
                      CreationTime = c.CreationTime.ToIsoString() }) }
          CreationDate = dto.Plant.CreationDate.ToIsoString()
          PotType = dto.Plant.PotTypeName
          PlantSpecie = dto.Plant.PlantSpecieName }

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
                    let usersRepo = ctx.GetService<UsersRepository>()
                    let! exists = usersRepo.AnyUserWithId dbConn userId

                    if exists then
                        return! handler userId next ctx
                    else
                        return!
                            constructFailure
                                HttpStatusCode.NotFound
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
let handleUpsertPlantCard (plantId: Guid) : HttpHandler =
    withAuthenticatedUser (fun userId ->
        withValidatedBody RawUpsertPlantCardRequest.parse (fun req next ctx ->
            task {
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()

                let! saveRes = repo.UpsertCardForPlantOwner dbConn userId (PlantId plantId) req

                return!
                    match saveRes with
                    | Error msg ->
                        constructFailure
                            HttpStatusCode.InternalServerError
                            [ BackendResponseErr.create msg ]
                            next
                            ctx

                    | Ok None ->
                        constructFailure
                            HttpStatusCode.NotFound
                            [ BackendResponseErr.create "Plant not found or access denied." ]
                            next
                            ctx

                    | Ok(Some savedCard) ->
                        let response = UpsertPlantCardResponse.fromDbResult savedCard req
                        constructSuccess response HttpStatusCode.OK next ctx
            }))
let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET >=> route "/load-all" >=> handleLoadMyPlants
          POST >=> route "/create" >=> handleCreatePlant
          GET >=> routef "/%O/load" (fun plantId -> handleLoadMyPlant plantId)
          POST >=> routef "/%O/upsert-card" (fun plantId -> handleUpsertPlantCard plantId) ]
