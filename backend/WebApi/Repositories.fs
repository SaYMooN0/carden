module WebApi.Repositories

open System
open System.Data
open System.Threading.Tasks
open Dapper
open Domain.Models
open Microsoft.Extensions.Configuration
open Npgsql

type ConnectionFactory(configuration: IConfiguration) =
    let connectionString = configuration.GetConnectionString("CardenDb")

    member _.CreateConnection() : NpgsqlConnection =
        let conn: NpgsqlConnection = new NpgsqlConnection(connectionString)
        conn

[<CLIMutable>]
type AppUserDbDto =
    { Id: Guid
      Email: string
      PasswordHash: string
      RegistrationDate: DateTimeOffset }

module AppUserDbDto =
    let toDomain (dto: AppUserDbDto) : AppUser =
        { Id = AppUserId dto.Id
          Email =    
            match Email.tryCreate dto.Email with
            | Ok email -> email
            | Error _ -> failwith "Incorrect email in the db"
          PasswordHash = PasswordHash dto.PasswordHash
          RegistrationDate = dto.RegistrationDate }


    let fromDomain (u: AppUser) : AppUserDbDto =
        { Id = AppUserId.value u.Id
          Email = Email.value u.Email
          PasswordHash = PasswordHash.value u.PasswordHash
          RegistrationDate = u.RegistrationDate }

type UsersRepository() =
    member _.GetById (conn: NpgsqlConnection) (id: AppUserId) : Task<AppUser option> =
        task {
            let sql =
                """
                    SELECT Id, Email, PasswordHash, RegistrationDate
                    FROM app_user
                    WHERE Id = @Id
                """

            let! dto = conn.QuerySingleOrDefaultAsync<AppUserDbDto>(sql, {| Id = AppUserId.value id |})
            return dto |> Option.ofObj |> Option.map AppUserDbDto.toDomain
        }

    member _.GetByEmail (conn: NpgsqlConnection) (email: Email.Email) : Task<AppUser option> =
        task {
            let sql =
                """
                    SELECT Id, Email, PasswordHash, RegistrationDate
                    FROM app_user
                    WHERE Email = @Email
                """

            let! dto = conn.QuerySingleOrDefaultAsync<AppUserDbDto>(sql, {| Email = Email.value email |})
            return dto |> Option.ofObj |> Option.map AppUserDbDto.toDomain
        }

    member _.AnyUserWithEmail (conn: NpgsqlConnection) (email: Email.Email) : Task<bool> =
        task {
            let sql =
                """
                    SELECT EXISTS (
                        SELECT 1
                        FROM app_user
                        WHERE email = @Email
                    )
                """

            let! exists = conn.QuerySingleOrDefaultAsync<bool>(sql, {| Email = Email.value email |})
            return exists
        }

    member _.AnyUserWithId (conn: NpgsqlConnection) (id: AppUserId) : Task<bool> =
        task {
            let sql =
                """
                        SELECT EXISTS (
                            SELECT 1
                            FROM app_user
                            WHERE id = @Id
                        )
                    """

            let! exists = conn.QuerySingleOrDefaultAsync<bool>(sql, {| Id = AppUserId.value id |})
            return exists
        }

    member _.Insert (conn: NpgsqlConnection) (user: AppUser) : Task<Result<unit, string>> =
        task {
            try
                let sql =
                    """
                        INSERT INTO app_user (Id, Email, PasswordHash, RegistrationDate)
                        VALUES (@Id, @Email, @PasswordHash, @RegistrationDate)
                    """

                let dto = AppUserDbDto.fromDomain user
                let! rows = conn.ExecuteAsync(sql, dto)

                if rows = 1 then
                    return Ok()
                else
                    return Error "User insert into db failed"

            with :? PostgresException as ex when ex.SqlState = "23505" ->
                return Error "User with this email already exists"
        }
    member _.InsertWithinTransaction
        (conn: NpgsqlConnection)
        (tx: NpgsqlTransaction)
        (user: AppUser)
        : Task<Result<unit, string>> =
        task {
            try
                let sql =
                    """
                        INSERT INTO app_user (Id, Email, PasswordHash, RegistrationDate)
                        VALUES (@Id, @Email, @PasswordHash, @RegistrationDate)
                    """

                let dto = AppUserDbDto.fromDomain user
                let! rows = conn.ExecuteAsync(sql, dto, tx)

                if rows = 1 then
                    return Ok()
                else
                    return Error "User insert into db failed"

            with :? PostgresException as ex when ex.SqlState = "23505" ->
                return Error "User with this email already exists"
        }

[<CLIMutable>]
type UnconfirmedUserDbDto =
    { Id: Guid
      Email: string
      PasswordHash: string
      ConfirmationCode: string }

module UnconfirmedUserDbDto =
    let toDomain (dto: UnconfirmedUserDbDto) : UnconfirmedUser =
        let email =
            match Email.tryCreate dto.Email with
            | Ok email -> email
            | Error _ -> failwith "Incorrect email in the db"

        let confirmationCode =
            match ConfirmationCode.tryCreate dto.ConfirmationCode with
            | Ok code -> code
            | Error _ -> failwith "Incorrect confirmation code in the db"

        { Id = UnconfirmedUserId dto.Id
          Email = email
          PasswordHash = PasswordHash dto.PasswordHash
          ConfirmationCode = confirmationCode }

    let fromDomain (u: UnconfirmedUser) : UnconfirmedUserDbDto =
        { Id = UnconfirmedUserId.value u.Id
          Email = Email.value u.Email
          PasswordHash = PasswordHash.value u.PasswordHash
          ConfirmationCode = ConfirmationCode.value u.ConfirmationCode }


type UnconfirmedUsersRepository() =
    member _.GetByIdAndConfirmationCode
        (conn: NpgsqlConnection)
        (id: UnconfirmedUserId)
        (confirmationCode: ConfirmationCode)
        : Task<UnconfirmedUser option> =
        task {
            let sql =
                """
                    SELECT Id, Email, PasswordHash, ConfirmationCode
                    FROM unconfirmed_user
                    WHERE Id = @Id AND ConfirmationCode = @ConfirmationCode
                """

            let! dto =
                conn.QuerySingleOrDefaultAsync<UnconfirmedUserDbDto>(
                    sql,
                    {| Id = UnconfirmedUserId.value id
                       ConfirmationCode = ConfirmationCode.value confirmationCode |}
                )

            return dto |> Option.ofObj |> Option.map UnconfirmedUserDbDto.toDomain
        }

    member _.UpsertByEmail
        (conn: NpgsqlConnection)
        (user: UnconfirmedUser)
        : Task<Result<UnconfirmedUser, string>> =
        task {
            try
                let sql =
                    """
                        INSERT INTO unconfirmed_user (Id, Email, PasswordHash, ConfirmationCode)
                        VALUES (@Id, @Email, @PasswordHash, @ConfirmationCode)
                        ON CONFLICT (Email)
                        DO UPDATE SET
                            PasswordHash = EXCLUDED.PasswordHash,
                            ConfirmationCode = EXCLUDED.ConfirmationCode
                        RETURNING Id, Email, PasswordHash, ConfirmationCode
                    """

                let dto = UnconfirmedUserDbDto.fromDomain user
                let! savedDto = conn.QuerySingleAsync<UnconfirmedUserDbDto>(sql, dto)

                return Ok(UnconfirmedUserDbDto.toDomain savedDto)
            with ex ->
                return Error $"Unconfirmed user save failed: {ex.Message}"
        }

    member _.DeleteByIdWithinTransaction
        (conn: NpgsqlConnection)
        (tx: NpgsqlTransaction)
        (id: UnconfirmedUserId)
        : Task<Result<unit, string>> =
        task {
            try
                let sql =
                    """
                        DELETE FROM unconfirmed_user
                        WHERE Id = @Id
                    """

                let! rows =
                    conn.ExecuteAsync(
                        sql,
                        {| Id = UnconfirmedUserId.value id |},
                        tx
                    )

                if rows = 1 then
                    return Ok()
                else
                    return Error "Unconfirmed user deletion failed"
            with ex ->
                return Error $"Unconfirmed user deletion failed: {ex.Message}"
        }