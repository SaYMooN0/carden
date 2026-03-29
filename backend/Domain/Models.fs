module Domain.Models

open System
open System.Security.Cryptography
open System.Text.RegularExpressions
open Domain.CardContentItem


type CardId = CardId of Guid

type Card =
    { Id: CardId
      ContentFront: CardContentItem list //обязательно Ordered
      ContentBack: CardContentItem list //обязательно Ordered
      LastTimeEdited: DateTimeOffset
      CreationTime: DateTimeOffset }

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
    | CeramicWithSun
    | PVZ

module PotTypeName =
    let value =
        function
        | CeramicWithSun -> "CeramicWithSun"
        | PVZ -> "PVZ"

    let tryCreate (raw: string | null) : Result<PotTypeName, unit> =
        let value = if isNull raw then "" else raw.Trim()

        match value with
        | "CeramicWithSun" -> Ok CeramicWithSun
        | "PVZ" -> Ok PVZ
        | _ -> Error()

type PlantId = PlantId of Guid

module PlantId =
    let value (PlantId value) = value

type AppUserId = AppUserId of Guid

module AppUserId =
    let value (AppUserId g) = g

type PlantDescription = private PlantDescription of string

type PlantDescriptionCreationErr = | TooLong

module PlantDescription =
    let MaxLength = 500

    let tryCreate (valueToValidate: string) : Result<PlantDescription, PlantDescriptionCreationErr> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then
            Ok(PlantDescription "")
        elif value.Length > MaxLength then
            Error TooLong
        else
            Ok(PlantDescription value)

    let value (PlantDescription value) = value
    let empty = PlantDescription ""

type PlantName = private PlantName of string

type PlantNameCreationErr =
    | NoValue
    | TooLong


module PlantName =
    let MaxLength = 100

    let tryCreate (valueToValidate: string) : Result<PlantName, PlantNameCreationErr> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then Error NoValue
        elif value.Length > MaxLength then Error TooLong
        else Ok(PlantName value)

    let value (PlantName value) = value

type Plant =
    { Id: PlantId
      OwnerId: AppUserId
      Name: PlantName
      Description: PlantDescription
      Deck: Deck
      CreationDate: DateTimeOffset
      PotType: PotTypeName
      PlantSpecie: PlantSpecieName
    // StudyState: DeckStudyState
    }

module Plant =
    let createNew ownerId name (now: DateTimeOffset) potType plantSpecie : Plant =
        { Id = PlantId(Guid.CreateVersion7())
          OwnerId = ownerId
          Name = name
          Description = PlantDescription.empty
          Deck =
            { Id = DeckId(Guid.CreateVersion7())
              Cards = []
              LastTimeEdited = now }
          CreationDate = now
          PotType = potType
          PlantSpecie = plantSpecie }

module Email =
    type Email = private Email of string

    type EmailCreationErr =
        | NoValue
        | IncorrectFormat of {| Value: string |}

    let private emailRegex = Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled)

    let tryCreate (value: string) : Result<Email, EmailCreationErr> =
        if String.IsNullOrWhiteSpace value then Error NoValue
        else if emailRegex.IsMatch value then Ok(Email value)
        else Error(IncorrectFormat {| Value = value |})


    let value (Email v) = v

type PasswordHash = PasswordHash of string

module PasswordHash =
    let value (PasswordHash h) = h

type AppUser =
    { Id: AppUserId
      Email: Email.Email
      PasswordHash: PasswordHash
      RegistrationDate: DateTimeOffset }

type UnconfirmedUserId = UnconfirmedUserId of Guid

module UnconfirmedUserId =
    let value (UnconfirmedUserId value) = value


type ConfirmationCode = private ConfirmationCode of string

module ConfirmationCode =
    let value (ConfirmationCode value) = value

    let tryCreate (valueToValidate: string) : Result<ConfirmationCode, string> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then
            Error "Confirmation code is required"
        elif value.Length > 128 then
            Error "Confirmation code is too long"
        else
            Ok(ConfirmationCode value)

    let generate () : ConfirmationCode =
        RandomNumberGenerator.GetBytes(32) |> Convert.ToHexString |> ConfirmationCode


type UnconfirmedUser =
    { Id: UnconfirmedUserId
      Email: Email.Email
      PasswordHash: PasswordHash
      ConfirmationCode: ConfirmationCode }

module UnconfirmedUser =
    let toConfirmedUser (registrationDate: DateTimeOffset) (user: UnconfirmedUser) : AppUser =
        { Id = AppUserId(UnconfirmedUserId.value user.Id)
          Email = user.Email
          PasswordHash = user.PasswordHash
          RegistrationDate = registrationDate }
