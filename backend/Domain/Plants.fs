module Domain.Plants

open System
open Domain.CardContentItem
open Domain.PlantName


type CardId = CardId of Guid

type Card =
    private
        { Id: CardId
          ContentFront: CardContentItem list
          ContentBack: CardContentItem list
          LastTimeEdited: DateTimeOffset
          CreationTime: DateTimeOffset }

module Card =
    [<RequireQualifiedAccess>]
    type CardCreationErr =
        | EmptyFrontContent
        | EmptyBackContent

    let tryCreate
        (id: CardId)
        (contentFront: CardContentItem list)
        (contentBack: CardContentItem list)
        (now: DateTimeOffset)
        : Result<Card, CardCreationErr> =
        if List.isEmpty contentFront then
            Error CardCreationErr.EmptyFrontContent
        elif List.isEmpty contentBack then
            Error CardCreationErr.EmptyBackContent
        else
            Ok
                { Id = id
                  ContentFront = contentFront
                  ContentBack = contentBack
                  CreationTime = now
                  LastTimeEdited = now }

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

type Plant =
    { Id: PlantId
      OwnerId: AppUserId
      Name: PlantName
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
          Deck =
            { Id = DeckId(Guid.CreateVersion7())
              Cards = []
              LastTimeEdited = now }
          CreationDate = now
          PotType = potType
          PlantSpecie = plantSpecie }
