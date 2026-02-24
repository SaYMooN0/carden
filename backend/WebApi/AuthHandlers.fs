module WebApi.AuthHandlers

open System
open System.Net
open Domain.Models
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.AspNetCore.Identity
open WebApi.BackendResponse
open WebApi.Repositories
open Email
open WebApi.Validation

let handlePingAuth: HttpHandler = text "Hello World, from Giraffe"




open UserPassword

type RawSignUpRequest =
    { Email: string
      Password: string
      ConfirmPassword: string }

type ParsedSignUpRequest =
    { Email: Email; Password: UserPassword }

module RawSignUpRequest =
    let private validateEmail (rawEmail: string) : Result<Email, BackendResponseErr list> =
        Email.tryCreate rawEmail
        |> Result.mapError (fun e ->
            match e with
            | Email.NoValue -> [ BackendResponseErr.create "No email provided" ]
            | IncorrectFormat _ -> [ BackendResponseErr.create $"{rawEmail} is not a valid email" ])

    let private validatePassword password confirm =
        map2
            (fun pwd _ -> pwd)
            (tryCreate password
             |> Result.mapError (fun e ->
                 match e with
                 | NoValue -> [ BackendResponseErr.create "Password is required" ]
                 | TooShort -> [ BackendResponseErr.create $"Password must be at least {MinLength} characters" ]
                 | TooLong -> [ BackendResponseErr.create $"Password must be at most {MaxLength} characters" ]))
            (if password = confirm then
                 Ok()
             else
                 Error [ BackendResponseErr.create "Passwords do not match" ])

    let parse (req: RawSignUpRequest) : Result<ParsedSignUpRequest, BackendResponseErr list> =
        map2
            (fun email password -> { Email = email; Password = password })
            (validateEmail req.Email)
            (validatePassword req.Password req.ConfirmPassword)


let handleSignUp: HttpHandler =
    withValidatedBody RawSignUpRequest.parse (fun req ->
        fun next ctx ->
            task {
                let repo = ctx.GetService<UsersRepository>()
                let! exists = repo.AnyUserWithEmail req.Email

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

                    let! insertRes = repo.Insert user

                    return!
                        match insertRes with
                        | Ok() -> constructSuccess () HttpStatusCode.OK next ctx
                        | Error msg -> constructFailure HttpStatusCode.InternalServerError (BackendResponseErr.create msg) next ctx
            })

let handleLogin: HttpHandler = text "Hello World, from Giraffe"
let handleLogout: HttpHandler = text "Hello World, from Giraffe"

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
