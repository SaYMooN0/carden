module WebApi.Contracts

open System
open Domain.Models
open Domain.Models.Email
open WebApi.BackendResponse
open WebApi.Repositories
open WebApi.UserPassword
open WebApi.Validation

type RawSignUpRequest =
    { Email: string
      Password: string
      ConfirmPassword: string }

type ParsedSignUpRequest =
    { Email: Email; Password: UserPassword }

module RawSignUpRequest =
    let private validateEmail (rawEmail: string) : Result<Email, BackendResponseErr list> =
        Email.tryCreate rawEmail
        |> Result.mapError (fun e ->
            match e with
            | Email.NoValue -> [ BackendResponseErr.create "No email provided" ]
            | IncorrectFormat _ -> [ BackendResponseErr.create $"{rawEmail} is not a valid email" ])

    let private validatePassword password confirm =
        map2
            (fun pwd _ -> pwd)
            (tryCreate password
             |> Result.mapError (fun e ->
                 match e with
                 | NoValue -> [ BackendResponseErr.create "Password is required" ]
                 | TooShort -> [ BackendResponseErr.create $"Password must be at least {MinLength} characters" ]
                 | TooLong -> [ BackendResponseErr.create $"Password must be at most {MaxLength} characters" ]))
            (if password = confirm then
                 Ok()
             else
                 Error [ BackendResponseErr.create "Passwords do not match" ])

    let parse (req: RawSignUpRequest) : Result<ParsedSignUpRequest, BackendResponseErr list> =
        map2
            (fun email password -> { Email = email; Password = password })
            (validateEmail req.Email)
            (validatePassword req.Password req.ConfirmPassword)


type RawLoginRequest = { Email: string; Password: string }

type ParsedLoginRequest =
    { Email: Email; Password: UserPassword }

module RawLoginRequest =
    let private validateEmail raw =
        Email.tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | Email.NoValue -> [ BackendResponseErr.create "No email provided" ]
            | IncorrectFormat _ -> [ BackendResponseErr.create "Invalid email format" ])

    let private validatePassword raw =
        tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | NoValue -> [ BackendResponseErr.create "Password is required" ]
            | TooShort -> [ BackendResponseErr.create "Password too short" ]
            | TooLong -> [ BackendResponseErr.create "Password too long" ])

    let parse (req: RawLoginRequest) =
        map2 (fun email password -> { Email = email; Password = password }) (validateEmail req.Email) (validatePassword req.Password)


type RawConfirmRegistrationRequest =
    { UserId: Guid
      ConfirmationCode: string }

type ParsedConfirmRegistrationRequest =
    { UserId: UnconfirmedUserId
      ConfirmationCode: ConfirmationCode }

module RawConfirmRegistrationRequest =
    let private validateUserId (rawUserId: Guid) : Result<UnconfirmedUserId, BackendResponseErr list> =
        if rawUserId = Guid.Empty then
            Error [ BackendResponseErr.create "User id is required" ]
        else
            Ok(UnconfirmedUserId rawUserId)

    let private validateConfirmationCode (rawConfirmationCode: string) : Result<ConfirmationCode, BackendResponseErr list> =
        ConfirmationCode.tryCreate rawConfirmationCode
        |> Result.mapError (fun _ -> [ BackendResponseErr.create "Confirmation code is invalid" ])

    let parse (raw: RawConfirmRegistrationRequest) : Result<ParsedConfirmRegistrationRequest, BackendResponseErr list> =
        map2
            (fun userId confirmationCode ->
                { UserId = userId
                  ConfirmationCode = confirmationCode })
            (validateUserId raw.UserId)
            (validateConfirmationCode raw.ConfirmationCode)



module DecksSortBy =
    let tryCreate (raw: string option) : Result<DecksSortBy, BackendResponseErr list> =
        match raw |> Option.map _.Trim().ToLowerInvariant() with
        | None
        | Some ""
        | Some "name" -> Ok Name
        | Some "creation-date" -> Ok CreationDate
        | Some _ -> Error [ BackendResponseErr.create "sortBy must be either 'name' or 'creationDate'" ]

module SortDirection =
    let tryCreate (raw: string option) : Result<SortDirection, BackendResponseErr list> =
        match raw |> Option.map _.Trim().ToLowerInvariant() with
        | None
        | Some ""
        | Some "asc" -> Ok Asc
        | Some "desc" -> Ok Desc
        | Some _ -> Error [ BackendResponseErr.create "direction must be either 'asc' or 'desc'" ]


type ParsedGetMyDecksRequest =
    { SortBy: DecksSortBy
      Direction: SortDirection }

module ParsedGetMyDecksRequest =
    let parse (sortByRaw: string option) (directionRaw: string option) : Result<ParsedGetMyDecksRequest, BackendResponseErr list> =
        map2
            (fun sortBy direction ->
                { SortBy = sortBy
                  Direction = direction })
            (DecksSortBy.tryCreate sortByRaw)
            (SortDirection.tryCreate directionRaw)


type RawCreatePlantDeckRequest =
    { Name: string
      Description: string
      PlantSpecie: string
      PotType: string }

type ParsedCreatePlantDeckRequest =
    { Name: PlantName
      Description: PlantDescription
      PlantSpecie: PlantSpecieName
      PotType: PotTypeName }

module RawCreatePlantDeckRequest =
    let private validateName (raw: string) : Result<PlantName, BackendResponseErr list> =
        PlantName.tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | PlantNameCreationErr.NoValue -> [ BackendResponseErr.create "Plant name is required" ]
            | PlantNameCreationErr.TooLong -> [ BackendResponseErr.create "Plant name must be at most 100 characters" ])

    let private validateDescription (raw: string) : Result<PlantDescription, BackendResponseErr list> =
        let value = if isNull raw then "" else raw.Trim()

        Ok value

    let private validatePlantSpecie (raw: string) : Result<PlantSpecieName, BackendResponseErr list> =
        PlantSpecieName.tryCreate raw
        |> Result.mapError (fun _ -> [ BackendResponseErr.create "Invalid plant specie" ])

    let private validatePotType (raw: string) : Result<PotTypeName, BackendResponseErr list> =
        PotTypeName.tryCreate raw
        |> Result.mapError (fun _ -> [ BackendResponseErr.create "Invalid pot type" ])

    let parse (req: RawCreatePlantDeckRequest) : Result<ParsedCreatePlantDeckRequest, BackendResponseErr list> =
        map2
            (fun (name, description) (plantSpecie, potType) ->
                { Name = name
                  Description = description
                  PlantSpecie = plantSpecie
                  PotType = potType })
            (map2 (fun name description -> name, description) (validateName req.Name) (validateDescription req.Description))
            (map2 (fun plantSpecie potType -> plantSpecie, potType) (validatePlantSpecie req.PlantSpecie) (validatePotType req.PotType))
