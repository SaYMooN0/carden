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
        { Id = UserId dto.Id
          Email =
            match Email.tryCreate dto.Email with
            | Ok email -> email
            | Error _ -> failwith "Incorrect email in the db"
          PasswordHash = PasswordHash dto.PasswordHash
          RegistrationDate = dto.RegistrationDate }


    let fromDomain (u: User) : UserDbDto =
        { Id = UserId.value u.Id
          Email = Email.value u.Email
          PasswordHash = PasswordHash.value u.PasswordHash
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

            let! dto = conn.QuerySingleOrDefaultAsync<UserDbDto>(sql, {| Id = UserId.value id |})
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

    member _.Insert(user: User) : Task<Result<unit, string>> =
        task {
            try
                use conn = connectionFactory.CreateConnection()

                let sql =
                    """
                        INSERT INTO users (Id, Email, PasswordHash, RegistrationDate)
                        VALUES (@Id, @Email, @PasswordHash, @RegistrationDate)
                    """

                let dto = UserDbDto.fromDomain user
                let! rows = conn.ExecuteAsync(sql, dto)

                if rows = 1 then
                    return Ok()
                else
                    return Error "User insert into db failed"

            with :? PostgresException as ex when ex.SqlState = "23505" ->
                return Error "User with this email already exists"
        }
