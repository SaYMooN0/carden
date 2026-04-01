module WebApi.Requests

open System
open Domain
open Domain.CardContentItem
open Domain.Email
open Domain.PlantName
open Domain.Plants
open Domain.Users
open WebApi.BackendResponse
open WebApi.RepositoriesShared
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
            | Email.IncorrectFormat _ -> [ BackendResponseErr.create $"{rawEmail} is not a valid email" ])

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
            | Email.IncorrectFormat _ -> [ BackendResponseErr.create "Invalid email format" ])

    let private validatePassword raw =
        tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | NoValue -> [ BackendResponseErr.create "Password is required" ]
            | TooShort -> [ BackendResponseErr.create "Password too short" ]
            | TooLong -> [ BackendResponseErr.create "Password too long" ])

    let parse (req: RawLoginRequest) =
        map2
            (fun email password -> { Email = email; Password = password })
            (validateEmail req.Email)
            (validatePassword req.Password)


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

    let private validateConfirmationCode
        (rawConfirmationCode: string)
        : Result<ConfirmationCode, BackendResponseErr list> =
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
    let parse
        (sortByRaw: string option)
        (directionRaw: string option)
        : Result<ParsedGetMyDecksRequest, BackendResponseErr list> =
        map2
            (fun sortBy direction ->
                { SortBy = sortBy
                  Direction = direction })
            (DecksSortBy.tryCreate sortByRaw)
            (SortDirection.tryCreate directionRaw)


type RawCreatePlantDeckRequest =
    { Name: string
      PlantSpecie: string
      PotType: string }

type ParsedCreatePlantDeckRequest =
    { Name: PlantName
      PlantSpecie: PlantSpecieName
      PotType: PotTypeName }

module RawCreatePlantDeckRequest =
    let private validateName (raw: string) : Result<PlantName, BackendResponseErr list> =
        PlantName.tryCreate raw
        |> Result.mapError (fun e ->
            match e with
            | PlantNameCreationErr.NoValue -> [ BackendResponseErr.create "Plant name is required" ]
            | PlantNameCreationErr.TooLong ->
                [ BackendResponseErr.create $"Plant name cannot be longer than {PlantName.MaxLength} characters" ])

    let private validatePlantSpecie (raw: string) : Result<PlantSpecieName, BackendResponseErr list> =
        PlantSpecieName.tryCreate raw
        |> Result.mapError (fun _ -> [ BackendResponseErr.create "Invalid plant specie" ])

    let private validatePotType (raw: string) : Result<PotTypeName, BackendResponseErr list> =
        PotTypeName.tryCreate raw
        |> Result.mapError (fun _ -> [ BackendResponseErr.create "Invalid pot type" ])

    let parse (req: RawCreatePlantDeckRequest) : Result<ParsedCreatePlantDeckRequest, BackendResponseErr list> =
        map3
            (fun name plantSpecie potType ->
                { Name = name
                  PlantSpecie = plantSpecie
                  PotType = potType })
            (validateName req.Name)
            (validatePlantSpecie req.PlantSpecie)
            (validatePotType req.PotType)

type RawCardContentItem = { text: string }

type RawUpsertPlantCardRequest =
    { CardId: string option
      ContentFront: RawCardContentItem list
      ContentBack: RawCardContentItem list }

type ParsedCardTarget =
    | CreateNewCard
    | UpdateExistingCard of cardId: Guid

type ParsedUpsertPlantCardRequest =
    { TargetCard: ParsedCardTarget
      ContentFront: CardContentItem list
      ContentBack: CardContentItem list }

module RawUpsertPlantCardRequest =

    let private parseCardTarget (cardIdRaw: string option) : Result<ParsedCardTarget, BackendResponseErr list> =
        match
            cardIdRaw
            |> Option.bind (fun x -> if String.IsNullOrWhiteSpace x then None else Some x)
        with
        | None -> Ok CreateNewCard
        | Some strId ->
            match Guid.TryParse strId with
            | true, guid -> Ok(UpdateExistingCard guid)
            | false, _ -> Error [ BackendResponseErr.create "Invalid cardId format." ]

    let private parseContentItem (item: RawCardContentItem) : Result<CardContentItem option, BackendResponseErr list> =
        if String.IsNullOrWhiteSpace item.text then
            Ok None
        else
            match CardContentItem.tryCreate item.text with
            | Ok parsed -> Ok(Some parsed)
            | Error CardContentItemCreationErr.Empty -> Ok None
            | Error CardContentItemCreationErr.TooLong ->
                Error
                    [ BackendResponseErr.create
                          $"Content item text must be at most {CardContentItem.TextMaxLength} characters long" ]

    let private parseContentList
        (items: RawCardContentItem list)
        : Result<CardContentItem list, BackendResponseErr list> =
        let folder acc next =
            match acc, parseContentItem next with
            | Ok parsed, Ok(Some parsedItem) -> Ok(parsedItem :: parsed)
            | Ok parsed, Ok None -> Ok parsed
            | Error errs1, Error errs2 -> Error(errs1 @ errs2)
            | Error errs, Ok _ -> Error errs
            | Ok _, Error errs -> Error errs

        List.fold folder (Ok []) items |> Result.map List.rev

    let private ensureSideNotEmpty
        (sideName: string)
        (items: CardContentItem list)
        : Result<CardContentItem list, BackendResponseErr list> =
        if List.isEmpty items then
            Error [ BackendResponseErr.create $"{sideName} must contain at least one non-empty content item" ]
        else
            Ok items

    let parse (raw: RawUpsertPlantCardRequest) : Result<ParsedUpsertPlantCardRequest, BackendResponseErr list> =
        let targetRes = parseCardTarget raw.CardId

        let frontRes =
            raw.ContentFront
            |> parseContentList
            |> Result.bind (ensureSideNotEmpty "Front side")

        let backRes =
            raw.ContentBack
            |> parseContentList
            |> Result.bind (ensureSideNotEmpty "Back side")

        match targetRes, frontRes, backRes with
        | Ok target, Ok front, Ok back ->
            Ok
                { TargetCard = target
                  ContentFront = front
                  ContentBack = back }
        | _ ->
            [ match targetRes with
              | Error errs -> yield! errs
              | _ -> ()

              match frontRes with
              | Error errs -> yield! errs
              | _ -> ()

              match backRes with
              | Error errs -> yield! errs
              | _ -> () ]
            |> Error
