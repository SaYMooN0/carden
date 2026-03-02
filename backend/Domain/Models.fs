module Domain.Models

open System
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

type DeckId = Guid

type Deck =
    { Id: DeckId
      Cards: Card list
      LastTimeEdited: DateTime }

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
type PotTypeName =
    | Керамикссолнышокм
    | Черныйпластик
type PlantId = PlantId of Guid
type AppUserId = AppUserId of Guid
module AppUserId =
    let value (AppUserId g) = g
type PlantDescription = string

type Plant =
    { Id: PlantId
      OwnerId: AppUserId
      Description: PlantDescription
      Deck: Deck
      CreationDate: DateTime
      PotType: PotTypeName
      PlantSpecie: PlantSpecieName
    // StudyState: DeckStudyState
    // View()=> currentplantstage (got from current StudyState & PlantSpecie )+ current pot lvl (lvl + deck)
    }
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
