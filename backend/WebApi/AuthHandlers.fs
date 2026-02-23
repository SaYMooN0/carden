module WebApi.AuthHandlers

open Domain.Models
open Giraffe
open Microsoft.AspNetCore.Http
open WebApi.Repositories

let handlePingAuth: HttpHandler = text "Hello World, from Giraffe"

type SignUpRequest =
    { Email: string
      Password: string
      ConfirmPassword: string }

type ParsedSignUpRequest = { Email: Email; Password: string }
module SignUpRequest =
    let parse req : Result<ParsedSignUpRequest, > =
        match Email.tryCreate req.Email with
        | None -> Error InvalidEmail
        | Some email ->
            if req.Password <> req.ConfirmPassword then
                Error PasswordMismatch
            else
                Ok
                    { Email = email
                      Password = req.Password }
let handleSignUp: HttpHandler =
    fun next ctx ->
        task {
            let! model = ctx.BindJsonAsync<SignUpRequest>()
            let repo = ctx.GetService<UsersRepository>()

            let! anyUserWithEmail = repo.AnyUserWithEmail "123@gmail.com"

            return! json anyUserWithEmail next ctx
        }

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
