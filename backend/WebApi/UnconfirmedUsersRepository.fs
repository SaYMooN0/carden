module WebApi.UnconfirmedUsersRepository

open System
open System.Threading.Tasks
open Dapper
open Domain.Email
open Domain.Users
open Npgsql


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
                    SELECT "Id", "Email", "PasswordHash", "ConfirmationCode"
                    FROM unconfirmed_user
                    WHERE "Id" = @Id AND "ConfirmationCode" = @ConfirmationCode
                """

            let! dto =
                conn.QuerySingleOrDefaultAsync<UnconfirmedUserDbDto>(
                    sql,
                    {| Id = UnconfirmedUserId.value id
                       ConfirmationCode = ConfirmationCode.value confirmationCode |}
                )

            return dto |> Option.ofObj |> Option.map UnconfirmedUserDbDto.toDomain
        }

    member _.UpsertByEmail (conn: NpgsqlConnection) (user: UnconfirmedUser) : Task<Result<UnconfirmedUser, string>> =
        task {
            let sql =
                """
                        INSERT INTO unconfirmed_user ("Id", "Email", "PasswordHash", "ConfirmationCode")
                        VALUES (@Id, @Email, @PasswordHash, @ConfirmationCode)
                        ON CONFLICT ("Email")
                        DO UPDATE SET
                            "PasswordHash" = EXCLUDED."PasswordHash",
                            "ConfirmationCode" = EXCLUDED."ConfirmationCode"
                        RETURNING "Id", "Email", "PasswordHash", "ConfirmationCode"
                    """

            let dto = UnconfirmedUserDbDto.fromDomain user
            let! savedDto = conn.QuerySingleAsync<UnconfirmedUserDbDto>(sql, dto)

            return Ok(UnconfirmedUserDbDto.toDomain savedDto)
        }

    member _.DeleteByIdWithinTransaction
        (conn: NpgsqlConnection)
        (tx: NpgsqlTransaction)
        (id: UnconfirmedUserId)
        : Task<Result<unit, string>> =
        task {
            let sql =
                """
                    DELETE FROM unconfirmed_user
                    WHERE "Id" = @Id
                """

            let! rows = conn.ExecuteAsync(sql, {| Id = UnconfirmedUserId.value id |}, tx)

            if rows = 1 then
                return Ok()
            else
                return Error "Unconfirmed user deletion failed"
        }
