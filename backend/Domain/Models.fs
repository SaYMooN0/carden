module Domain.Models
open System




// type DeckStudyState = {} //мб отдельная сущность мб внутри колоды


type CardContentItem = 
    | TextContentItem
    | ImageContentItem
    | MathAjaxContentItem
//https://docs.mathjax.org/en/latest/basic/mathematics.html#:~:text=TeX%20mathematics%20(see%20the-,MathJax%20Web%20Demos%20Repository%20for%20more,-).
 
type CardId = System.Guid

type Card = {
     Id: CardId
     ContentFront: CardContentItem list //обязательно Ordered
     ContentBack: CardContentItem list //обязательно Ordered
     LastTimeEdited: DateTime
     CreationTime: DateTime
} 
 
type DeckId = System.Guid

type Deck ={
     Id: DeckId
     Cards: Card list
     LastTimeEdited: DateTime
}
type SimplePlantStageSprite = string 
type AttachmentSprite = string //размер спрайта аттачмента такой же как и у растения

type Attachment = {
     Sp: AttachmentSprite[] 
}

type Stage5Sprite = {
     Sprite: SimplePlantStageSprite
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

type PlantSpecieId = 
     | Венеринамухоловка 
     | Арбузарбуз 
     | Хэловинпамкин


type PlantSpecie = {
     Id: PlantSpecieId
     SpriteStage1: SimplePlantStageSprite
     SpriteStage2: SimplePlantStageSprite
     SpriteStage3: SimplePlantStageSprite
     SpriteStage4: SimplePlantStageSprite
     Stage5Sprite: Stage5Sprite
}

type PotLvlSprite = {
     Image: string
     PlantSpriteStartPosition: int //кол-во пикселей сверху
}
type PotTypeId =
     | Керамикссолнышокм 
     | Черныйпластик
type PotType = {
     Id: PotTypeId
     Lvl1Sprite :PotLvlSprite
     Lvl2Sprite :PotLvlSprite
     Lvl3Sprite :PotLvlSprite
     Lvl4Sprite :PotLvlSprite
     Lvl5Sprite :PotLvlSprite
}
     

type PlantId = System.Guid
type UserId = {Value: System.Guid}

type PlantDescription = string
type Plant = {
     Id: PlantId
     OwnerId: UserId
     Description: PlantDescription
     Deck: Deck
     PotType: PotType
     CreationDate: DateTime
     PlantSpecie: PlantSpecie
     StudyState: DeckStudyState
     // View()=> currentplantstage (got from current StudyState & PlantSpecie )+ current pot lvl (lvl + deck)
}

type Email = string
type User  = {
     Id : UserId
     Email : Email
     PasswordHash : string
     RegistrationDate : DateTime
}
