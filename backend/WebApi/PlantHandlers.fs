module WebApi.PlantHandlers

open System
open System.Net
open Domain.Models
open Microsoft.AspNetCore.Http
open WebApi.BackendResponse
open WebApi.JwtToken
open WebApi.Repositories
open Giraffe
open WebApi.Contracts
open WebApi.Validation

type DeckSummaryResponse =
    { Id: Guid
      Name: string
      PlantSpecie: string
      PotType: string
      CardsCount: int
      CreationDate: DateTime }

module DeckSummaryResponse =
    let fromDomain (item: PlantDeckSummary) : DeckSummaryResponse =
        { Id = PlantId.value item.Id
          Name = PlantName.value item.Name
          PlantSpecie = PlantSpecieName.value item.PlantSpecie
          PotType = PotTypeName.value item.PotType
          CardsCount = item.CardsCount
          CreationDate = item.CreationDate }

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
            let jwtService = ctx.GetService<JwtTokenService>()

            match jwtService.UserIdFromJwtToken token with
            | Error InvalidToken ->
                constructFailure HttpStatusCode.Unauthorized [ BackendResponseErr.create "Invalid authentication token" ] next ctx

            | Error Unexpected ->
                constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create "Unexpected token parsing error" ] next ctx

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

let handleGetMyDecks: HttpHandler =
    withAuthenticatedUser (fun userId next ctx ->
        task {
            let sortByRaw = tryGetQueryValue ctx "sortBy"
            let directionRaw = tryGetQueryValue ctx "direction"

            match ParsedGetMyDecksRequest.parse sortByRaw directionRaw with
            | Error errs -> return! constructFailure HttpStatusCode.BadRequest errs next ctx

            | Ok parsed ->
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()

                let! items = repo.GetDeckSummariesByOwner dbConn userId parsed.SortBy parsed.Direction

                let response = items |> List.map DeckSummaryResponse.fromDomain

                return! constructSuccess response HttpStatusCode.OK next ctx
        })

let handleCreatePlantDeck: HttpHandler =
    withAuthenticatedUser (fun userId ->
        withValidatedBody RawCreatePlantDeckRequest.parse (fun req next ctx ->
            task {
                use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<PlantsRepository>()

                let plant: Plant =
                    Plant.createNew userId req.Name req.Description DateTimeOffset.UtcNow req.PotType req.PlantSpecie

                let! createRes = repo.InsertNewPlant dbConn plant
                return!
                    match createRes with
                    | Ok plantId -> constructSuccess {| Id = PlantId.value plantId |} HttpStatusCode.OK next ctx
                    | Error msg -> constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx
            }))
