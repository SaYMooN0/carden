module WebApi.AuthHandlers

open System
open System.Net
open Domain.Models
open Giraffe
open Microsoft.AspNetCore.Http
open WebApi.BackendResponse
open WebApi.Contracts
open WebApi.EmailService
open WebApi.JwtToken
open WebApi.Repositories
open WebApi.UserPassword
open WebApi.Validation

let private tokenCookieName = "user_tkn"

let getAuthCookie (ctx: HttpContext) : Result<JwtToken, unit> =
    let exist, cookie = ctx.Request.Cookies.TryGetValue tokenCookieName
    if not exist then Error() else Ok(JwtToken cookie)

let ensureNoAuthToken: HttpHandler =
    fun next ctx ->
        let tokenCorrect =
            match getAuthCookie ctx with
            | Error _ -> false
            | Ok token -> ctx.GetService<JwtTokenService>().UserIdFromJwtToken token |> Result.isOk


        if not tokenCorrect then
            next ctx
        else
            constructFailure
                HttpStatusCode.BadRequest
                [ BackendResponseErr.create
                      "It seems like you are already authenticated. Please log out to proceed. If error persists please wait for 5 minutes" ]
                next
                ctx

let authCookieOptions () =
    CookieOptions(HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = Nullable(DateTimeOffset.UtcNow.AddMinutes(4.0)))

let handlePingAuth: HttpHandler =
    fun next ctx ->
        match getAuthCookie ctx with
        | Error _ ->
            constructFailure
                HttpStatusCode.BadRequest
                [ BackendResponseErr.create "Authentication token missing. Please log in again." ]
                next
                ctx
        | Ok token ->
            let dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let repo = ctx.GetService<UsersRepository>()
            let jwtService = ctx.GetService<JwtTokenService>()

            match jwtService.UserIdFromJwtToken token with
            | Error tokenErr ->
                let tokenErrMsg =
                    match tokenErr with
                    | InvalidToken -> "Invalid authentication token"
                    | Unexpected -> "Unexpected token parsing error"

                constructFailure HttpStatusCode.BadRequest [ BackendResponseErr.create tokenErrMsg ] next ctx

            | Ok userId ->
                task {
                    let! exists = repo.AnyUserWithId dbConn userId

                    if exists then
                        return! constructSuccess {| UserId = AppUserId.value userId |} HttpStatusCode.OK next ctx
                    else
                        return!
                            constructFailure
                                HttpStatusCode.NotFound
                                [ BackendResponseErr.create "Account not found. Please log out and log in again" ]
                                next
                                ctx
                }


let handleResetPassword: HttpHandler = text "Hello World, from Giraffe"


let handleSignUp: HttpHandler =
    withValidatedBody RawSignUpRequest.parse (fun req next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let usersRepo = ctx.GetService<UsersRepository>()
            let unconfirmedUsersRepo = ctx.GetService<UnconfirmedUsersRepository>()
            let emailService = ctx.GetService<EmailService>()

            let! exists = usersRepo.AnyUserWithEmail dbConn req.Email

            if exists then
                return!
                    constructFailure HttpStatusCode.Conflict [ BackendResponseErr.create "User with such email already exists" ] next ctx
            else
                let passwordHasher = ctx.GetService<PasswordHasher>()

                let unconfirmedUser: UnconfirmedUser =
                    { Id = UnconfirmedUserId(Guid.CreateVersion7())
                      Email = req.Email
                      PasswordHash = passwordHasher.HashPassword req.Password
                      ConfirmationCode = ConfirmationCode.generate () }

                let! saveRes = unconfirmedUsersRepo.UpsertByEmail dbConn unconfirmedUser

                match saveRes with
                | Error msg -> return! constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx

                | Ok savedUser ->
                    let! emailRes = emailService.SendRegistrationConfirmationLink savedUser.Email savedUser.Id savedUser.ConfirmationCode

                    match emailRes with
                    | Ok() -> return! constructSuccess () HttpStatusCode.OK next ctx

                    | Error msg -> return! constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx
        })

let handleConfirmRegistration: HttpHandler =
    withValidatedBody RawConfirmRegistrationRequest.parse (fun req next ctx ->
        task {
            use dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
            let usersRepo = ctx.GetService<UsersRepository>()
            let unconfirmedUsersRepo = ctx.GetService<UnconfirmedUsersRepository>()

            let! unconfirmedUserOpt = unconfirmedUsersRepo.GetByIdAndConfirmationCode dbConn req.UserId req.ConfirmationCode

            match unconfirmedUserOpt with
            | None ->
                return!
                    constructFailure
                        HttpStatusCode.BadRequest
                        [ BackendResponseErr.create "Invalid or expired confirmation link" ]
                        next
                        ctx

            | Some unconfirmedUser ->
                do! dbConn.OpenAsync()
                let! tx = dbConn.BeginTransactionAsync()
                use tx = tx

                let confirmedUser =
                    UnconfirmedUser.toConfirmedUser DateTimeOffset.UtcNow unconfirmedUser

                let! insertRes = usersRepo.InsertWithinTransaction dbConn tx confirmedUser

                match insertRes with
                | Error msg ->
                    do! tx.RollbackAsync()

                    let status =
                        if msg = "User with this email already exists" then
                            HttpStatusCode.Conflict
                        else
                            HttpStatusCode.InternalServerError

                    return! constructFailure status [ BackendResponseErr.create msg ] next ctx

                | Ok() ->
                    let! deleteRes = unconfirmedUsersRepo.DeleteByIdWithinTransaction dbConn tx unconfirmedUser.Id

                    match deleteRes with
                    | Error msg ->
                        do! tx.RollbackAsync()
                        return! constructFailure HttpStatusCode.InternalServerError [ BackendResponseErr.create msg ] next ctx

                    | Ok() ->
                        do! tx.CommitAsync()
                        return! constructSuccess () HttpStatusCode.OK next ctx
        })

let handleLogin: HttpHandler =
    withValidatedBody RawLoginRequest.parse (fun req ->
        fun next ctx ->
            task {
                let dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<UsersRepository>()
                let! user = repo.GetByEmail dbConn req.Email

                match user with
                | None ->
                    return!
                        constructFailure
                            HttpStatusCode.Unauthorized
                            [ BackendResponseErr.create "There is no account for this email" ]
                            next
                            ctx

                | Some user ->
                    let passwordHasher = ctx.GetService<PasswordHasher>()

                    if not (passwordHasher.VerifyPassword user.PasswordHash req.Password) then
                        return! constructFailure HttpStatusCode.Unauthorized [ BackendResponseErr.create "Incorrect password" ] next ctx
                    else
                        let jwtService = ctx.GetService<JwtTokenService>()
                        let token = jwtService.CreateToken(user)

                        ctx.Response.Cookies.Append(tokenCookieName, token, authCookieOptions ())
                        return! constructSuccess () HttpStatusCode.OK next ctx
            })

let handleLogout: HttpHandler =
    fun next ctx ->
        task {
            let expired =
                CookieOptions(
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = Nullable(DateTimeOffset.UtcNow.AddDays(-1.0))
                )

            ctx.Response.Cookies.Append(tokenCookieName, "", expired)
            return! constructSuccess () HttpStatusCode.OK next ctx
        }

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [
          POST >=> route "/sign-up" >=> ensureNoAuthToken >=> handleSignUp
          POST >=> route "/login" >=> ensureNoAuthToken >=> handleLogin
          
          POST >=> route "/ping" >=> handlePingAuth
          POST >=> route "/logout" >=> handleLogout
          POST >=> route "/confirm-registration" >=> handleConfirmRegistration
          POST >=> route "/reset-password" >=> ensureNoAuthToken >=> handleResetPassword ]

