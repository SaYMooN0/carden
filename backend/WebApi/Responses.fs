module WebApi.Responses

open System
open Domain.PlantName
open Domain.Plants
open Giraffe
open WebApi.PlantsRepositories

type CardContentItemResponse = { Text: string }

module CardContentItemResponse =
    let fromText (text: string) : CardContentItemResponse = { Text = text }

type CardResponse =
    { Id: Guid
      ContentFront: CardContentItemResponse list
      ContentBack: CardContentItemResponse list
      LastTimeEdited: string
      CreationTime: string }

module CardResponse =
    let fromDbDto (dto: PlantCardDbDto) : CardResponse =
        { Id = dto.Id
          ContentFront = dto.ContentFront |> Array.toList |> List.map CardContentItemResponse.fromText
          ContentBack = dto.ContentBack |> Array.toList |> List.map CardContentItemResponse.fromText
          LastTimeEdited = dto.LastTimeEdited.ToIsoString()
          CreationTime = dto.CreationTime.ToIsoString() }

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


    type DeckResponse =
        { Id: Guid
          Cards: CardResponse list
          LastTimeEdited: string }

        type PlantResponse =
            { Id: Guid
              Name: string
              Deck: DeckResponse
              CreationDate: string
              PotType: string
              PlantSpecie: string }

    module PlantResponse =
        let fromDbDto (dto: PlantWithCardsDbDto) : PlantResponse =
            { Id = dto.Plant.Id
              Name = dto.Plant.Name
              Deck =
                { Id = dto.Plant.DeckId
                  LastTimeEdited = dto.Plant.DeckLastTimeEdited.ToIsoString()
                  Cards = dto.Cards |> List.map CardResponse.fromDbDto }
              CreationDate = dto.Plant.CreationDate.ToIsoString()
              PotType = dto.Plant.PotTypeName
              PlantSpecie = dto.Plant.PlantSpecieName }