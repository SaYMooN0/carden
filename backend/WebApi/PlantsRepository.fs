module WebApi.PlantsRepositories

open System
open System.Threading.Tasks
open Dapper
open Npgsql
open Domain.Plants
open Domain.PlantName
open WebApi.RepositoriesShared

type PlantPreviewDto =
    { Id: PlantId
      Name: PlantName
      PlantSpecie: PlantSpecieName
      PotType: PotTypeName
      CardsCount: int
      CreationDate: DateTimeOffset }

[<CLIMutable>]
type PlantPreviewDbDto =
    { Id: Guid
      Name: string
      PlantSpecieName: string
      PotTypeName: string
      CardsCount: int
      CreationDate: DateTimeOffset }

module PlantPreviewDbDto =
    let private parsePlantName (raw: string) =
        match PlantName.tryCreate raw with
        | Ok value -> value
        | Error err -> failwith $"Invalid plant name in DB: {raw}. Err: {err}"

    let private parsePlantSpecieName (raw: string) =
        match PlantSpecieName.tryCreate raw with
        | Ok value -> value
        | Error _ -> failwith $"Invalid plant specie name in DB: {raw}"

    let private parsePotTypeName (raw: string) =
        match PotTypeName.tryCreate raw with
        | Ok value -> value
        | Error _ -> failwith $"Invalid pot type name in DB: {raw}"

    let toDomain (dto: PlantPreviewDbDto) : PlantPreviewDto =
        { Id = PlantId dto.Id
          Name = parsePlantName dto.Name
          PlantSpecie = parsePlantSpecieName dto.PlantSpecieName
          PotType = parsePotTypeName dto.PotTypeName
          CardsCount = dto.CardsCount
          CreationDate = dto.CreationDate }

[<CLIMutable>]
type PlantDetailsDbDto =
    { Id: Guid
      Name: string
      DeckId: Guid
      DeckLastTimeEdited: DateTimeOffset
      CreationDate: DateTimeOffset
      PotTypeName: string
      PlantSpecieName: string }

[<CLIMutable>]
type PlantCardDbDto =
    { Id: Guid
      ContentFront: string array
      ContentBack: string array
      LastTimeEdited: DateTimeOffset
      CreationTime: DateTimeOffset }

type PlantWithCardsDbDto =
    { Plant: PlantDetailsDbDto
      Cards: PlantCardDbDto list }

[<CLIMutable>]
type StudyPlantDetailsDbDto =
    { Id: Guid
      Name: string
      DeckId: Guid
      DeckLastTimeEdited: DateTimeOffset
      CreationDate: DateTimeOffset
      PotTypeName: string
      PlantSpecieName: string
      LastCompletedStudyDay: Nullable<DateOnly> }

[<CLIMutable>]
type StudyPlantCardDbDto =
    { Id: Guid
      ContentFront: string array
      ContentBack: string array
      StudyStateType: string
      StudyDueAt: Nullable<DateTimeOffset>
      StudyLearningStepIndex: Nullable<int>
      StudyStartedAt: Nullable<DateTimeOffset>
      StudyReviewInterval: Nullable<TimeSpan>
      StudyLastReviewedAt: Nullable<DateTimeOffset>
      LastTimeEdited: DateTimeOffset
      CreationTime: DateTimeOffset }

type LoadStudySessionDbDto =
    { Plant: StudyPlantDetailsDbDto
      ReviewCards: StudyPlantCardDbDto list
      NewCards: StudyPlantCardDbDto list }

type SavePlantCardDbRequest =
    { TargetCardId: Guid option
      ContentFront: string array
      ContentBack: string array
      Now: DateTimeOffset }

type SavePlantCardResult =
    | PlantNotFoundOrAccessDenied
    | CardNotFoundOrAccessDenied
    | Saved of PlantCardDbDto

type LoadStudySessionResult =
    | PlantNotFoundOrAccessDenied
    | AlreadyCompletedToday
    | Loaded of LoadStudySessionDbDto

type PlantsRepository() =

    member _.GetPreviewsByOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (sortBy: DecksSortBy)
        (direction: SortDirection)
        : Task<PlantPreviewDto list> =
        task {
            let sortColumn =
                match sortBy with
                | DecksSortBy.Name -> """ p."Name" """
                | DecksSortBy.CreationDate -> """ p."CreationDate" """

            let sortDirection =
                match direction with
                | SortDirection.Asc -> "ASC"
                | SortDirection.Desc -> "DESC"

            let sql =
                $"""
                    SELECT
                        p."Id",
                        p."Name",
                        p."PlantSpecieName",
                        p."PotTypeName",
                        COUNT(c."Id")::int AS "CardsCount",
                        p."CreationDate"
                    FROM plant p
                    INNER JOIN deck d ON d."Id" = p."DeckId"
                    LEFT JOIN card c ON c."DeckId" = d."Id"
                    WHERE p."OwnerId" = @OwnerId
                    GROUP BY p."Id", p."Name", p."PlantSpecieName", p."PotTypeName", p."CreationDate"
                    ORDER BY {sortColumn} {sortDirection}, p."Id" ASC
                """

            let! dtos = conn.QueryAsync<PlantPreviewDbDto>(sql, {| OwnerId = AppUserId.value ownerId |})

            return dtos |> Seq.map PlantPreviewDbDto.toDomain |> Seq.toList
        }

    member _.GetByIdForOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        : Task<PlantWithCardsDbDto option> =
        task {
            let sql =
                """
                    SELECT
                        p."Id",
                        p."Name",
                        d."Id" AS "DeckId",
                        d."LastTimeEdited" AS "DeckLastTimeEdited",
                        p."CreationDate",
                        p."PotTypeName",
                        p."PlantSpecieName"
                    FROM plant p
                    INNER JOIN deck d ON d."Id" = p."DeckId"
                    WHERE p."Id" = @PlantId AND p."OwnerId" = @OwnerId;

                    SELECT
                        c."Id",
                        c."ContentFront",
                        c."ContentBack",
                        c."LastTimeEdited",
                        c."CreationTime"
                    FROM card c
                    INNER JOIN plant p ON p."DeckId" = c."DeckId"
                    WHERE p."Id" = @PlantId AND p."OwnerId" = @OwnerId
                    ORDER BY c."CreationTime" ASC, c."Id" ASC;
                """

            let args =
                {| PlantId = PlantId.value plantId
                   OwnerId = AppUserId.value ownerId |}

            let! grid = conn.QueryMultipleAsync(sql, args)
            use grid = grid

            let! plantDto = grid.ReadSingleOrDefaultAsync<PlantDetailsDbDto>()

            match plantDto |> Option.ofObj with
            | None -> return None
            | Some plant ->
                let! cardDtos = grid.ReadAsync<PlantCardDbDto>()

                return
                    Some
                        { Plant = plant
                          Cards = cardDtos |> Seq.toList }
        }

    member _.InsertNewPlant (conn: NpgsqlConnection) (plant: Plant) : Task<Result<unit, string>> =
        task {
            try
                do! conn.OpenAsync()
                let! tx = conn.BeginTransactionAsync()
                use tx = tx

                let insertDeckSql =
                    """
                    INSERT INTO deck ("Id", "LastTimeEdited")
                    VALUES (@Id, @LastTimeEdited)
                    """

                let deckDto =
                    {| Id = DeckId.value plant.Deck.Id
                       LastTimeEdited = plant.Deck.LastTimeEdited |}

                let! deckRows = conn.ExecuteAsync(insertDeckSql, deckDto, tx)

                if deckRows <> 1 then
                    do! tx.RollbackAsync()
                    return Error "Deck insert into db failed"
                else
                    let insertPlantSql =
                        """
                        INSERT INTO plant
                            ("Id", "OwnerId", "Name", "DeckId", "CreationDate", "PotTypeName", "PlantSpecieName")
                        VALUES
                            (@Id, @OwnerId, @Name, @DeckId, @CreationDate, @PotType, @PlantSpecie)
                        """

                    let plantDto =
                        {| Id = PlantId.value plant.Id
                           OwnerId = AppUserId.value plant.OwnerId
                           Name = PlantName.value plant.Name
                           DeckId = DeckId.value plant.Deck.Id
                           CreationDate = plant.CreationDate
                           PotType = PotTypeName.value plant.PotType
                           PlantSpecie = PlantSpecieName.value plant.PlantSpecie |}

                    let! plantRows = conn.ExecuteAsync(insertPlantSql, plantDto, tx)

                    if plantRows <> 1 then
                        do! tx.RollbackAsync()
                        return Error "Plant insert into db failed"
                    else
                        do! tx.CommitAsync()
                        return Ok()

            with ex ->
                return Error $"Plant creation failed: {ex.Message}"
        }

    member private _.TryGetOwnedDeckId
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (tx: NpgsqlTransaction)
        : Task<Guid option> =
        task {
            let sql =
                """
                SELECT p."DeckId"
                FROM plant p
                WHERE p."Id" = @PlantId
                  AND p."OwnerId" = @OwnerId
                """

            let! ids =
                conn.QueryAsync<Guid>(
                    sql,
                    {| PlantId = PlantId.value plantId
                       OwnerId = AppUserId.value ownerId |},
                    tx
                )

            return ids |> Seq.tryHead
        }

    member _.GetCardByIdForPlantOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (cardId: Guid)
        : Task<PlantCardDbDto option> =
        task {
            let sql =
                """
                SELECT
                    c."Id",
                    c."ContentFront",
                    c."ContentBack",
                    c."LastTimeEdited",
                    c."CreationTime"
                FROM card c
                INNER JOIN plant p ON p."DeckId" = c."DeckId"
                WHERE p."Id" = @PlantId
                  AND p."OwnerId" = @OwnerId
                  AND c."Id" = @CardId
                """

            let! rows =
                conn.QueryAsync<PlantCardDbDto>(
                    sql,
                    {| PlantId = PlantId.value plantId
                       OwnerId = AppUserId.value ownerId
                       CardId = cardId |}
                )

            return rows |> Seq.tryHead
        }

    member this.SaveCardForPlantOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (req: SavePlantCardDbRequest)
        : Task<Result<SavePlantCardResult, string>> =
        task {
            try
                do! conn.OpenAsync()
                let! tx = conn.BeginTransactionAsync()
                use tx = tx

                let! ownedDeckIdOpt = this.TryGetOwnedDeckId conn ownerId plantId tx

                match ownedDeckIdOpt with
                | None ->
                    do! tx.RollbackAsync()
                    return Ok SavePlantCardResult.PlantNotFoundOrAccessDenied

                | Some deckId ->
                    match req.TargetCardId with
                    | None ->
                        let newCardId = Guid.CreateVersion7()

                        let insertSql =
                            """
                            INSERT INTO card
                                ("Id", "DeckId", "ContentFront", "ContentBack", "LastTimeEdited", "CreationTime")
                            VALUES
                                (@Id, @DeckId, @ContentFront, @ContentBack, @Now, @Now)
                            RETURNING
                                "Id",
                                "ContentFront",
                                "ContentBack",
                                "LastTimeEdited",
                                "CreationTime"
                            """

                        let! insertedRows =
                            conn.QueryAsync<PlantCardDbDto>(
                                insertSql,
                                {| Id = newCardId
                                   DeckId = deckId
                                   ContentFront = req.ContentFront
                                   ContentBack = req.ContentBack
                                   Now = req.Now |},
                                tx
                            )

                        match insertedRows |> Seq.tryHead with
                        | None ->
                            do! tx.RollbackAsync()
                            return Error "Card insert failed."
                        | Some savedCard ->
                            let! deckRows =
                                conn.ExecuteAsync(
                                    """UPDATE deck SET "LastTimeEdited" = @Now WHERE "Id" = @DeckId""",
                                    {| Now = req.Now; DeckId = deckId |},
                                    tx
                                )

                            if deckRows <> 1 then
                                do! tx.RollbackAsync()
                                return Error "Deck timestamp update failed"
                            else
                                do! tx.CommitAsync()
                                return Ok(Saved savedCard)

                    | Some cardId ->
                        let updateSql =
                            """
                            UPDATE card
                            SET
                                "ContentFront" = @ContentFront,
                                "ContentBack" = @ContentBack,
                                "LastTimeEdited" = @Now
                            WHERE "Id" = @CardId
                              AND "DeckId" = @DeckId
                            RETURNING
                                "Id",
                                "ContentFront",
                                "ContentBack",
                                "LastTimeEdited",
                                "CreationTime"
                            """

                        let! updatedRows =
                            conn.QueryAsync<PlantCardDbDto>(
                                updateSql,
                                {| CardId = cardId
                                   DeckId = deckId
                                   ContentFront = req.ContentFront
                                   ContentBack = req.ContentBack
                                   Now = req.Now |},
                                tx
                            )

                        match updatedRows |> Seq.tryHead with
                        | None ->
                            do! tx.RollbackAsync()
                            return Ok CardNotFoundOrAccessDenied
                        | Some savedCard ->
                            let! deckRows =
                                conn.ExecuteAsync(
                                    """UPDATE deck SET "LastTimeEdited" = @Now WHERE "Id" = @DeckId""",
                                    {| Now = req.Now; DeckId = deckId |},
                                    tx
                                )

                            if deckRows <> 1 then
                                do! tx.RollbackAsync()
                                return Error "Deck timestamp update failed."
                            else
                                do! tx.CommitAsync()
                                return Ok(Saved savedCard)

            with ex ->
                return Error $"Saving plant card failed: {ex.Message}"
        }

    member _.GetStudySessionForOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (today: DateOnly)
        (now: DateTimeOffset)
        : Task<LoadStudySessionResult> =
        task {
            let sql =
                """
                    SELECT
                        p."Id",
                        p."Name",
                        d."Id" AS "DeckId",
                        d."LastTimeEdited" AS "DeckLastTimeEdited",
                        p."CreationDate",
                        p."PotTypeName",
                        p."PlantSpecieName",
                        p."LastCompletedStudyDay"
                    FROM plant p
                    INNER JOIN deck d ON d."Id" = p."DeckId"
                    WHERE p."Id" = @PlantId AND p."OwnerId" = @OwnerId;

                    SELECT
                        c."Id",
                        c."ContentFront",
                        c."ContentBack",
                        c."StudyStateType",
                        c."StudyDueAt",
                        c."StudyLearningStepIndex",
                        c."StudyStartedAt",
                        c."StudyReviewInterval",
                        c."StudyLastReviewedAt",
                        c."LastTimeEdited",
                        c."CreationTime"
                    FROM card c
                    INNER JOIN plant p ON p."DeckId" = c."DeckId"
                    WHERE p."Id" = @PlantId
                      AND p."OwnerId" = @OwnerId
                      AND c."StudyStateType" IS NOT NULL
                      AND c."StudyDueAt" <= @Now
                    ORDER BY c."StudyDueAt" ASC, c."CreationTime" ASC, c."Id" ASC
                    LIMIT @ReviewCardsPerDay;

                    SELECT
                        c."Id",
                        c."ContentFront",
                        c."ContentBack",
                        c."StudyStateType",
                        c."StudyDueAt",
                        c."StudyLearningStepIndex",
                        c."StudyStartedAt",
                        c."StudyReviewInterval",
                        c."StudyLastReviewedAt",
                        c."LastTimeEdited",
                        c."CreationTime"
                    FROM card c
                    INNER JOIN plant p ON p."DeckId" = c."DeckId"
                    WHERE p."Id" = @PlantId
                      AND p."OwnerId" = @OwnerId
                      AND c."StudyStateType" IS NULL
                    ORDER BY c."CreationTime" ASC, c."Id" ASC
                    LIMIT @NewCardsPerDay;
                """

            let args =
                {| PlantId = PlantId.value plantId
                   OwnerId = AppUserId.value ownerId
                   Now = now
                   ReviewCardsPerDay = StudyConstants.ReviewCardsPerDay
                   NewCardsPerDay = StudyConstants.NewCardsPerDay |}

            let! grid = conn.QueryMultipleAsync(sql, args)
            use grid = grid

            let! plantDto = grid.ReadSingleOrDefaultAsync<StudyPlantDetailsDbDto>()

            match plantDto |> Option.ofObj with
            | None -> return LoadStudySessionResult.PlantNotFoundOrAccessDenied

            | Some plant when
                plant.LastCompletedStudyDay.HasValue
                && plant.LastCompletedStudyDay.Value = today
                ->
                return LoadStudySessionResult.AlreadyCompletedToday

            | Some plant ->
                let! reviewCards = grid.ReadAsync<StudyPlantCardDbDto>()
                let! newCards = grid.ReadAsync<StudyPlantCardDbDto>()

                return
                    LoadStudySessionResult.Loaded
                        { Plant = plant
                          ReviewCards = reviewCards |> Seq.toList
                          NewCards = newCards |> Seq.toList }
        }
