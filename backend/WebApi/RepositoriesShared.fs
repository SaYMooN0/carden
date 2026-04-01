module WebApi.RepositoriesShared

open Microsoft.Extensions.Configuration
open Npgsql

type ConnectionFactory(configuration: IConfiguration) =
    let connectionString = configuration.GetConnectionString("CardenDb")

    member _.CreateConnection() : NpgsqlConnection =
        let conn: NpgsqlConnection = new NpgsqlConnection(connectionString)
        conn

type DecksSortBy =
    | Name
    | CreationDate

type SortDirection =
    | Asc
    | Desc