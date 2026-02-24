module WebApi.Repositories

open System
open System.Data
open System.Threading.Tasks
open Dapper
open Domain.Models
open Microsoft.Extensions.Configuration
open Npgsql

type ConnectionFactory(configuration: IConfiguration) =
    let connectionString = configuration.GetConnectionString("Default")

    member _.CreateConnection() : IDbConnection =
        let conn = new NpgsqlConnection(connectionString)
        conn


type UserDbDto =
    { Id: Guid
      Email: string
      PasswordHash: string
      RegistrationDate: DateTimeOffset }

module UserDbDto =
    let toDomain (dto: UserDbDto) : User =
        { Id = { Value = dto.Id }
          Email =
            match Email.tryCreate dto.Email with
            | Ok email -> email
            | Error _ -> failwith "Incorrect email in the db"
          PasswordHash = dto.PasswordHash
          RegistrationDate = dto.RegistrationDate }


    let fromDomain (u: User) : UserDbDto =
        { Id = u.Id.Value
          Email = Email.value u.Email
          PasswordHash = u.PasswordHash
          RegistrationDate = u.RegistrationDate }

type UsersRepository(connectionFactory: ConnectionFactory) =
    member _.GetById(id: UserId) : Task<User option> =
        task {
            use conn = connectionFactory.CreateConnection()

            let sql =
                """
                    SELECT Id, Email, PasswordHash, RegistrationDate
                    FROM users
                    WHERE Id = @Id
                """

            let! dto = conn.QuerySingleOrDefaultAsync<UserDbDto>(sql, {| Id = id.Value |})
            return dto |> Option.ofObj |> Option.map UserDbDto.toDomain
        }

    member _.AnyUserWithEmail(email: Email.Email) : Task<bool> =
        task {
            use conn = connectionFactory.CreateConnection()

            let sql =
                """
                    SELECT EXISTS (
                        SELECT 1
                        FROM users
                        WHERE email = @Email
                    )
                """

            let! exists = conn.QuerySingleOrDefaultAsync<bool>(sql, {| Email = Email.value email |})
            return exists
        }
