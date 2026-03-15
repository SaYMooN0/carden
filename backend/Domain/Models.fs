module Domain.Models

open System
open System.Security.Cryptography
open System.Text.RegularExpressions
open Domain.Errs




// type DeckStudyState = {} //мб отдельная сущность мб внутри колоды


type CardContentItem =
    | TextContentItem
    | ImageContentItem
    | MathAjaxContentItem
//https://docs.mathjax.org/en/latest/basic/mathematics.html#:~:text=TeX%20mathematics%20(see%20the-,MathJax%20Web%20Demos%20Repository%20for%20more,-).

type CardId = System.Guid

type Card =
    { Id: CardId
      ContentFront: CardContentItem list //обязательно Ordered
      ContentBack: CardContentItem list //обязательно Ordered
      LastTimeEdited: DateTime
      CreationTime: DateTime }

type DeckId = DeckId of Guid

type Deck =
    { Id: DeckId
      Cards: Card list
      LastTimeEdited: DateTimeOffset }

type SimplePlantStageSprite = string
type AttachmentSprite = string //размер спрайта аттачмента такой же как и у растения

type Attachment = { Sp: AttachmentSprite[] }

type Stage5Sprite =
    { Sprite: SimplePlantStageSprite
      //Attchment читаем свреху вниз, слева направа
      Attachment1: Attachment
      Attachment2: Attachment
      Attachment3: Attachment
      Attachment4: Attachment

    //View(days count)=> determined by day. убедиться что чередуем. скорее всего через остатки от деления  (и инедекс в массиве если массив)

    }
// AttachmentPosition: SimpleStageSprite по умолчанию 256 на 256 можно договориться о другом. и
// x и y это позиция куда мы влепим верхний левый угол attchmemt
//не массив чтобы нельзя было ошибиться с количеством, + все равно задаем 1 раз + хранить легче (отдельный столбец в бд)
//если у разных спишиз может быть разное колво аттачментсов то массивом естественно

type PlantSpecieName =
    | Венеринамухоловка
    | Арбузарбуз
    | Хэловинпамкин

module PlantSpecieName =
    let value =
        function
        | Венеринамухоловка -> "Венеринамухоловка"
        | Арбузарбуз -> "Арбузарбуз"
        | Хэловинпамкин -> "Хэловинпамкин"

    let tryCreate (raw: string) : Result<PlantSpecieName, unit> =
        let value = if isNull raw then "" else raw.Trim()

        match value with
        | "Венеринамухоловка" -> Ok Венеринамухоловка
        | "Арбузарбуз" -> Ok Арбузарбуз
        | "Хэловинпамкин" -> Ok Хэловинпамкин
        | _ -> Error()

type PotTypeName =
    | Керамикссолнышокм
    | Черныйпластик

module PotTypeName =
    let value =
        function
        | Керамикссолнышокм -> "Керамикссолнышокм"
        | Черныйпластик -> "Черныйпластик"

    let tryCreate (raw: string) : Result<PotTypeName, unit> =
        let value = if isNull raw then "" else raw.Trim()

        match value with
        | "Керамикссолнышокм" -> Ok Керамикссолнышокм
        | "Черныйпластик" -> Ok Черныйпластик
        | _ -> Error()

type PlantId = PlantId of Guid

module PlantId =
    let value (PlantId value) = value

type AppUserId = AppUserId of Guid

module AppUserId =
    let value (AppUserId g) = g

type PlantDescription = string

type PlantName = private PlantName of string

type PlantNameCreationErr =
    | NoValue
    | TooLong

module PlantName =
    let tryCreate (valueToValidate: string) : Result<PlantName, PlantNameCreationErr> =
        let value =
            if isNull valueToValidate then
                ""
            else
                valueToValidate.Trim()

        if String.IsNullOrWhiteSpace value then Error NoValue
        elif value.Length > 100 then Error TooLong
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
    // View()=> currentplantstage (got from current StudyState & PlantSpecie )+ current pot lvl (lvl + deck)
    }

module Plant =
    let createNew ownerId name description (now: DateTimeOffset) potType plantSpecie : Plant =
        { Id = PlantId(Guid.CreateVersion7())
          OwnerId = ownerId
          Name = name
          Description = description
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
