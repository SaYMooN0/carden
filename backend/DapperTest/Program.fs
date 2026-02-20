module Domain

open Dapper
open Npgsql
open System.Data
open System

type ConnectionFactory(connectionString: string) =

    member _.CreateConnection() : IDbConnection =
        let conn = new NpgsqlConnection(connectionString)
        conn.Open()
        conn


type PlantId = Guid

type Plant =
    { Id: PlantId
      Name: string
      CreatedAt: DateTime }

type PlantRepository(connectionFactory: ConnectionFactory) =

    member _.Insert(plant: Plant) =
        use conn = connectionFactory.CreateConnection()

        let sql =
            """
            INSERT INTO plants (id, name, created_at)
            VALUES (@Id, @Name, @CreatedAt)
        """

        conn.Execute(sql, plant) |> ignore


    member _.GetById(id: PlantId) : Plant option =
        use conn = connectionFactory.CreateConnection()

        let sql =
            """
            SELECT id, name, created_at
            FROM plants
            WHERE id = @Id
        """

        conn.QuerySingleOrDefault<Plant>(sql, {| Id = id |}) |> Option.ofObj


    member _.GetAll() : Plant list =
        use conn = connectionFactory.CreateConnection()

        let sql =
            """
        SELECT 
            id          AS "Id",
            name        AS "Name",
            created_at  AS "CreatedAt"
        FROM plants
        """

        conn.Query<Plant>(sql) |> Seq.toList

    member _.InsertTwo (p1: Plant) (p2: Plant) =
        use conn = connectionFactory.CreateConnection()
        use tx = conn.BeginTransaction()

        try
            let sql =
                """
                INSERT INTO plants (id, name, created_at)
                VALUES (@Id, @Name, @CreatedAt)
            """

            conn.Execute(sql, p1, tx) |> ignore
            conn.Execute(sql, p2, tx) |> ignore

            tx.Commit()
        with ex ->
            tx.Rollback()
            raise ex

[<EntryPoint>]
let main _ =

    let connectionString =
        "Host=localhost;Port=5432;Username=postgres;Password=saymoon08;Database=books_db"
    Dapper.DefaultTypeMap.MatchNamesWithUnderscores <- true
    let factory = ConnectionFactory(connectionString)
    let repo = PlantRepository(factory)

    let plant =
        { Id = Guid.NewGuid()
          Name = "Venus Flytrap"
          CreatedAt = DateTime.UtcNow }

    repo.Insert plant

    let plants = repo.GetAll()

    printfn "%A" plants

    0
