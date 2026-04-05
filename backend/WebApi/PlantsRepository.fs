module WebApi.PlantsRepositories

open System
open System.Threading.Tasks
open Dapper
open Domain
open Domain.PlantName
open Domain.Study
open Domain.Study.StudySettings
open Microsoft.FSharp.Linq
open Npgsql
open Domain.Plants
open WebApi.RepositoriesShared

type PlantPreviewDto =
    { Id: PlantId
      Name: PlantName
      PlantSpecie: PlantSpecieName
      PotType: PotTypeName
      CardsCount: int
      CreationDate: DateTimeOffset
      StudySessionsCompleted: uint }

[<CLIMutable>]
type PlantPreviewDbDto =
    { Id: Guid
      Name: string
      PlantSpecieName: string
      PotTypeName: string
      CardsCount: int
      CreationDate: DateTimeOffset
      CompletedStudySessionsCount: int }

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
          CreationDate = dto.CreationDate
          StudySessionsCompleted = uint dto.CompletedStudySessionsCount }

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
      CompletedStudySessionsCount: int
      CardsCount: int }

[<CLIMutable>]
type StudyPlantCardDbDto =
    { Id: Guid
      ContentFront: string array
      ContentBack: string array
      StudyStateType: string
      StudyDueAt: Nullable<DateTimeOffset>
      StudyLearningStepIndex: Nullable<int>
      StudyStartedAt: Nullable<DateTimeOffset>
      StudyReviewIntervalSeconds: Nullable<int>
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

type DeletePlantCardResult =
    | PlantNotFoundOrAccessDenied
    | CardNotFoundOrAccessDenied
    | Deleted

type LoadStudySessionResult =
    | PlantNotFoundOrAccessDenied
    | NotEnoughCardsToStudy of cardsCount: int * plantId: PlantId
    | Loaded of LoadStudySessionDbDto

type PersistedCardStudyState =
    | New
    | Learning of dueAt: DateTimeOffset * learningStepIndex: int * startedAt: DateTimeOffset
    | Review of dueAt: DateTimeOffset * reviewIntervalSeconds: int * lastReviewedAt: DateTimeOffset

[<CLIMutable>]
type StudyCardStateDbDto =
    { Id: Guid
      StudyStateType: string
      StudyDueAt: Nullable<DateTimeOffset>
      StudyLearningStepIndex: Nullable<int>
      StudyStartedAt: Nullable<DateTimeOffset>
      StudyReviewIntervalSeconds: Nullable<int>
      StudyLastReviewedAt: Nullable<DateTimeOffset> }

[<CLIMutable>]
type StudyCardStateUpdateDbDto =
    { Id: Guid
      StudyStateType: string
      StudyDueAt: Nullable<DateTimeOffset>
      StudyLearningStepIndex: Nullable<int>
      StudyStartedAt: Nullable<DateTimeOffset>
      StudyReviewIntervalSeconds: Nullable<int>
      StudyLastReviewedAt: Nullable<DateTimeOffset> }

module PersistedCardStudyState =
    let fromDb (sessionStartedAt: DateTimeOffset) (dto: StudyCardStateDbDto) =
        match dto.StudyStateType with
        | "Learning" ->
            Learning(
                dto.StudyDueAt
                |> Shared.Nullable.toOption
                |> Option.defaultValue sessionStartedAt,
                dto.StudyLearningStepIndex |> Shared.Nullable.toOption |> Option.defaultValue 0,
                dto.StudyStartedAt
                |> Shared.Nullable.toOption
                |> Option.defaultValue sessionStartedAt
            )
        | "Review" ->
            Review(
                dto.StudyDueAt
                |> Shared.Nullable.toOption
                |> Option.defaultValue sessionStartedAt,
                dto.StudyReviewIntervalSeconds
                |> Shared.Nullable.toOption
                |> Option.defaultValue StudySettings.LearningEasyIntervalSeconds,
                dto.StudyLastReviewedAt
                |> Shared.Nullable.toOption
                |> Option.defaultValue sessionStartedAt
            )
        | _ -> New

    let toDbDto (cardId: Guid) (state: PersistedCardStudyState) =
        match state with
        | New ->
            { Id = cardId
              StudyStateType = "New"
              StudyDueAt = Nullable()
              StudyLearningStepIndex = Nullable()
              StudyStartedAt = Nullable()
              StudyReviewIntervalSeconds = Nullable()
              StudyLastReviewedAt = Nullable() }

        | Learning(dueAt, learningStepIndex, startedAt) ->
            { Id = cardId
              StudyStateType = "Learning"
              StudyDueAt = Nullable(dueAt)
              StudyLearningStepIndex = Nullable(learningStepIndex)
              StudyStartedAt = Nullable(startedAt)
              StudyReviewIntervalSeconds = Nullable()
              StudyLastReviewedAt = Nullable() }

        | Review(dueAt, reviewIntervalSeconds, lastReviewedAt) ->
            { Id = cardId
              StudyStateType = "Review"
              StudyDueAt = Nullable(dueAt)
              StudyLearningStepIndex = Nullable()
              StudyStartedAt = Nullable()
              StudyReviewIntervalSeconds = Nullable(reviewIntervalSeconds)
              StudyLastReviewedAt = Nullable(lastReviewedAt) }



module StudyStateTransitions =
    let private multiplyIntervalSeconds (currentIntervalSeconds: int) (multiplier: float) =
        max 1 (int (Math.Round(float currentIntervalSeconds * multiplier)))

    let calculateNextState
        (previousState: PersistedCardStudyState)
        (difficulty: StudyAnswerDifficulty)
        (answeredAt: DateTimeOffset)
        : PersistedCardStudyState =
        match previousState with
        | New ->
            match difficulty with
            | Again -> Learning(answeredAt.AddSeconds(float LearningAgainDelaySeconds), 0, answeredAt)
            | Hard -> Learning(answeredAt.AddSeconds(float LearningHardDelaySeconds), 1, answeredAt)
            | Good -> Learning(answeredAt.AddSeconds(float LearningGoodDelaySeconds), 2, answeredAt)
            | Easy ->
                Review(
                    answeredAt.AddSeconds(float LearningEasyIntervalSeconds),
                    LearningEasyIntervalSeconds,
                    answeredAt
                )

        | Learning(_, previousLearningStepIndex, startedAt) ->
            match difficulty with
            | Again -> Learning(answeredAt.AddSeconds(float LearningAgainDelaySeconds), 0, startedAt)
            | Hard -> Learning(answeredAt.AddSeconds(float LearningHardDelaySeconds), 1, startedAt)
            | Good ->
                Learning(
                    answeredAt.AddSeconds(float LearningGoodDelaySeconds),
                    max previousLearningStepIndex 2,
                    startedAt
                )
            | Easy ->
                Review(
                    answeredAt.AddSeconds(float LearningEasyIntervalSeconds),
                    LearningEasyIntervalSeconds,
                    answeredAt
                )

        | Review(_, previousIntervalSeconds, _) ->
            match difficulty with
            | Again -> Learning(answeredAt.AddSeconds(float ReviewAgainDelaySeconds), 0, answeredAt)
            | Hard ->
                let nextInterval =
                    multiplyIntervalSeconds previousIntervalSeconds ReviewHardIntervalMultiplier

                Review(answeredAt.AddSeconds(float nextInterval), nextInterval, answeredAt)

            | Good ->
                let nextInterval =
                    multiplyIntervalSeconds previousIntervalSeconds ReviewGoodIntervalMultiplier

                Review(answeredAt.AddSeconds(float nextInterval), nextInterval, answeredAt)

            | Easy ->
                let nextInterval =
                    multiplyIntervalSeconds previousIntervalSeconds StudySettings.ReviewEasyIntervalMultiplier

                Review(answeredAt.AddSeconds(float nextInterval), nextInterval, answeredAt)

type GetStudyCardsForCompletionResult =
    | PlantNotFoundOrAccessDenied
    | Success of StudyCardStateDbDto list

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
                        p."CreationDate",
                        p."CompletedStudySessionsCount"
                    FROM plant p
                    INNER JOIN deck d ON d."Id" = p."DeckId"
                    LEFT JOIN card c ON c."DeckId" = d."Id"
                    WHERE p."OwnerId" = @OwnerId
                    GROUP BY
                        p."Id",
                        p."Name",
                        p."PlantSpecieName",
                        p."PotTypeName",
                        p."CreationDate",
                        p."CompletedStudySessionsCount"
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
                            return Ok SavePlantCardResult.CardNotFoundOrAccessDenied
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
                        p."CompletedStudySessionsCount",
                        (
                            SELECT COUNT(*)
                            FROM card c
                            WHERE c."DeckId" = p."DeckId"
                        )::int AS "CardsCount"
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
                        c."StudyReviewIntervalSeconds",
                        c."StudyLastReviewedAt",
                        c."LastTimeEdited",
                        c."CreationTime"
                    FROM card c
                    INNER JOIN plant p ON p."DeckId" = c."DeckId"
                    WHERE p."Id" = @PlantId
                      AND p."OwnerId" = @OwnerId
                      AND c."StudyStateType" IS NOT NULL
                      AND c."StudyStateType" <> 'New'
                      AND (
                            c."StudyDueAt" <= @Now
                            OR NOT EXISTS (
                                SELECT 1
                                FROM card c2
                                INNER JOIN plant p2 ON p2."DeckId" = c2."DeckId"
                                WHERE p2."Id" = @PlantId
                                  AND p2."OwnerId" = @OwnerId
                                  AND c2."StudyStateType" IS NOT NULL
                                  AND c2."StudyStateType" <> 'New'
                                  AND c2."StudyDueAt" <= @Now
                            )
                          )
                    ORDER BY
                        CASE WHEN c."StudyDueAt" <= @Now THEN 0 ELSE 1 END,
                        c."StudyDueAt" ASC NULLS LAST,
                        c."StudyLastReviewedAt" ASC NULLS FIRST,
                        c."CreationTime" ASC,
                        c."Id" ASC
                    LIMIT @ReviewCardsPerSession;

                    SELECT
                        c."Id",
                        c."ContentFront",
                        c."ContentBack",
                        c."StudyStateType",
                        c."StudyDueAt",
                        c."StudyLearningStepIndex",
                        c."StudyStartedAt",
                        c."StudyReviewIntervalSeconds",
                        c."StudyLastReviewedAt",
                        c."LastTimeEdited",
                        c."CreationTime"
                    FROM card c
                    INNER JOIN plant p ON p."DeckId" = c."DeckId"
                    WHERE p."Id" = @PlantId
                      AND p."OwnerId" = @OwnerId
                      AND (c."StudyStateType" IS NULL OR c."StudyStateType" = 'New')
                    ORDER BY c."CreationTime" ASC, c."Id" ASC
                    LIMIT @NewCardsPerSession;
                """

            let args =
                {| PlantId = PlantId.value plantId
                   OwnerId = AppUserId.value ownerId
                   Now = now
                   ReviewCardsPerSession = StudyConstants.ReviewCardsPerSession
                   NewCardsPerSession = StudyConstants.NewCardsPerSession |}

            let! grid = conn.QueryMultipleAsync(sql, args)
            use grid = grid

            let! plantDto = grid.ReadSingleOrDefaultAsync<StudyPlantDetailsDbDto>()

            match plantDto |> Option.ofObj with
            | None -> return LoadStudySessionResult.PlantNotFoundOrAccessDenied

            | Some plant when plant.CardsCount < 10 ->
                return LoadStudySessionResult.NotEnoughCardsToStudy(plant.CardsCount, plantId)

            | Some plant ->
                let! reviewCards = grid.ReadAsync<StudyPlantCardDbDto>()
                let! newCards = grid.ReadAsync<StudyPlantCardDbDto>()

                return
                    LoadStudySessionResult.Loaded
                        { Plant = plant
                          ReviewCards = reviewCards |> Seq.toList
                          NewCards = newCards |> Seq.toList }
        }

    member this.DeleteCardForPlantOwner
        (conn: NpgsqlConnection)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (cardId: Guid)
        (now: DateTimeOffset)
        : Task<Result<DeletePlantCardResult, string>> =
        task {
            try
                do! conn.OpenAsync()
                let! tx = conn.BeginTransactionAsync()
                use tx = tx

                let! ownedDeckIdOpt = this.TryGetOwnedDeckId conn ownerId plantId tx

                match ownedDeckIdOpt with
                | None ->
                    do! tx.RollbackAsync()
                    return Ok DeletePlantCardResult.PlantNotFoundOrAccessDenied

                | Some deckId ->
                    let deleteSql =
                        """
                        DELETE FROM card
                        WHERE "Id" = @CardId
                          AND "DeckId" = @DeckId
                        """

                    let! deletedRows = conn.ExecuteAsync(deleteSql, {| CardId = cardId; DeckId = deckId |}, tx)

                    if deletedRows <> 1 then
                        do! tx.RollbackAsync()
                        return Ok DeletePlantCardResult.CardNotFoundOrAccessDenied
                    else
                        let! deckRows =
                            conn.ExecuteAsync(
                                """UPDATE deck SET "LastTimeEdited" = @Now WHERE "Id" = @DeckId""",
                                {| Now = now; DeckId = deckId |},
                                tx
                            )

                        if deckRows <> 1 then
                            do! tx.RollbackAsync()
                            return Error "Deck timestamp update failed."
                        else
                            do! tx.CommitAsync()
                            return Ok DeletePlantCardResult.Deleted

            with ex ->
                return Error $"Deleting plant card failed: {ex.Message}"
        }

    member _.GetStudyCardsForCompletionForUpdate
        (conn: NpgsqlConnection)
        (tx: NpgsqlTransaction)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (cardIds: Guid list)
        : Task<Result<GetStudyCardsForCompletionResult, string>> =
        task {
            try
                let plantExistsSql =
                    """
                    SELECT COUNT(1)
                    FROM plant p
                    WHERE p."Id" = @PlantId
                      AND p."OwnerId" = @OwnerId
                    """

                let! plantExists =
                    conn.ExecuteScalarAsync<int>(
                        plantExistsSql,
                        {| PlantId = PlantId.value plantId
                           OwnerId = AppUserId.value ownerId |},
                        tx
                    )

                if plantExists = 0 then
                    return Ok PlantNotFoundOrAccessDenied
                else
                    let loadCardsSql =
                        """
                        SELECT
                            c."Id",
                            c."StudyStateType",
                            c."StudyDueAt",
                            c."StudyLearningStepIndex",
                            c."StudyStartedAt",
                            c."StudyReviewIntervalSeconds",
                            c."StudyLastReviewedAt"
                        FROM plant p
                        INNER JOIN deck d ON d."Id" = p."DeckId"
                        INNER JOIN card c ON c."DeckId" = d."Id"
                        WHERE p."Id" = @PlantId
                          AND p."OwnerId" = @OwnerId
                          AND c."Id" = ANY(@CardIds)
                        FOR UPDATE
                        """

                    let! cards =
                        conn.QueryAsync<StudyCardStateDbDto>(
                            loadCardsSql,
                            {| PlantId = PlantId.value plantId
                               OwnerId = AppUserId.value ownerId
                               CardIds = cardIds |> List.toArray |},
                            tx
                        )

                    let loadedCards = cards |> Seq.toList
                    let requestedUniqueCardsCount = cardIds |> List.distinct |> List.length

                    if loadedCards.Length <> requestedUniqueCardsCount then
                        return Error "Some study cards were not found or do not belong to the plant."
                    else
                        return Ok(Success loadedCards)
            with ex ->
                return Error ex.Message
        }

    member _.SaveCompletedStudySession
        (conn: NpgsqlConnection)
        (tx: NpgsqlTransaction)
        (ownerId: AppUserId)
        (plantId: PlantId)
        (updatedCards: StudyCardStateUpdateDbDto list)
        : Task<Result<int, string>> =
        task {
            try
                let updatedCardsCount = List.length updatedCards

                let incrementPlantSql =
                    """
                    UPDATE plant
                    SET "CompletedStudySessionsCount" = "CompletedStudySessionsCount" + 1
                    WHERE "Id" = @PlantId
                      AND "OwnerId" = @OwnerId
                    RETURNING "CompletedStudySessionsCount"
                    """

                if updatedCardsCount > 0 then
                    let updateCardSql =
                        """
                        UPDATE card
                        SET
                            "StudyStateType" = @StudyStateType,
                            "StudyDueAt" = @StudyDueAt,
                            "StudyLearningStepIndex" = @StudyLearningStepIndex,
                            "StudyStartedAt" = @StudyStartedAt,
                            "StudyReviewIntervalSeconds" = @StudyReviewIntervalSeconds,
                            "StudyLastReviewedAt" = @StudyLastReviewedAt
                        WHERE "Id" = @Id
                        """

                    let! updatedRows = conn.ExecuteAsync(updateCardSql, updatedCards, tx)

                    if updatedRows <> updatedCardsCount then
                        return Error "Not all study cards were updated."
                    else
                        let! completedStudySessionsCount =
                            conn.ExecuteScalarAsync<int>(
                                incrementPlantSql,
                                {| PlantId = PlantId.value plantId
                                   OwnerId = AppUserId.value ownerId |},
                                tx
                            )

                        return Ok completedStudySessionsCount
                else
                    let! completedStudySessionsCount =
                        conn.ExecuteScalarAsync<int>(
                            incrementPlantSql,
                            {| PlantId = PlantId.value plantId
                               OwnerId = AppUserId.value ownerId |},
                            tx
                        )

                    return Ok completedStudySessionsCount
            with ex ->
                return Error ex.Message
        }
