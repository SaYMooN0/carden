module Types

type DeckStudyState = {} //мб отдельная сущность мб внутри колоды


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
} 
 
type DeckId = System.Guid

type Deck ={
     Id: DeckId
     Cards: Card list
}
//мб назвать Attachment sprout
type AttachmentSprite = string
//Attchment читаем свреху вниз, слева направа
type SimplePlantStageSprite = string

type AttachmentPosition = {
     x: int
     y: int
}

type Attachment = {
     Pos: AttachmentPosition 
     Sp: AttachmentSprite[]
}

type Stage5Sprite = {
     Sprite: SimplePlantStageSprite
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

type PlantInPoPosition = {//позиция левого нижнего края
     x: int
     y: int
}

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
     PlantPosition: PlantInPoPosition
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


type Plant = {
     Id: PlantId
     OwnerId: UserId
     Deck: Deck
     PotType: PotType
     CreationDate: System.DateTime
     PlantSpecie: PlantSpecie
     StudyState: DeckStudyState
     // View()=> currentplantstage (got from current StudyState & PlantSpecie )+ current pot lvl (lvl + deck)
}

type User  = {
     Id : UserId
}
