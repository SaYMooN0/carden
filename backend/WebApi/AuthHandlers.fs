module WebApi.AuthHandlers

open System
open System.Net
open Domain.Models
open Giraffe
open Microsoft.AspNetCore.Http
open WebApi.BackendResponse
open WebApi.Contracts
open WebApi.Repositories
open WebApi.UserPassword
open WebApi.Validation

let private tokenCookieName = "user_tkn"
let handlePingAuth: HttpHandler = text "Hello World, from Giraffe"


let handleSignUp: HttpHandler =
    withValidatedBody RawSignUpRequest.parse (fun req ->
        fun next ctx ->
            task {
                let dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<UsersRepository>()
                let! exists = repo.AnyUserWithEmail dbConn req.Email

                if exists then
                    return!
                        constructFailure
                            HttpStatusCode.Conflict
                            [ BackendResponseErr.create "User with such email already exists" ]
                            next
                            ctx
                else
                    let passwordHasher = ctx.GetService<PasswordHasher>()

                    let user: User =
                        { Id = UserId(Guid.CreateVersion7())
                          Email = req.Email
                          PasswordHash = passwordHasher.HashPassword req.Password
                          RegistrationDate = DateTimeOffset.UtcNow }

                    let! insertRes = repo.Insert dbConn user

                    return!
                        match insertRes with
                        | Ok() -> constructSuccess () HttpStatusCode.OK next ctx
                        | Error msg -> constructFailure HttpStatusCode.InternalServerError (BackendResponseErr.create msg) next ctx
            })


let authCookieOptions () =
    CookieOptions(HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = Nullable(DateTimeOffset.UtcNow.AddDays(7.0)))

let handleLogin: HttpHandler =
    withValidatedBody RawLoginRequest.parse (fun req ->
        fun next ctx ->
            task {
                let dbConn = ctx.GetService<ConnectionFactory>().CreateConnection()
                let repo = ctx.GetService<UsersRepository>()
                let! user = repo.GetByEmail dbConn req.Email

                match user with
                | None  -> return! constructFailure HttpStatusCode.Unauthorized [] next ctx
                | Ok token -> // validate password

                let! result = handler.Handle(query)

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
        [ POST >=> route "/ping" >=> handlePingAuth
          POST >=> route "/sign-up" >=> handleSignUp
          POST >=> route "/login" >=> handleLogin
          POST >=> route "/logout" >=> handleLogout ]

// let someHttpHandler : HttpHandler =
//     fun (next : HttpFunc) (ctx : HttpContext) ->
//         match ctx.GetRequestHeader "X-MyOwnHeader" with
//         | Error msg ->
//             // Mandatory header is missing.
//             // Log error message
//             // Return error response to the client.
//         | Ok headerValue ->
//             // Do something with `headerValue`...
//             // Return a Task<HttpContext option>
