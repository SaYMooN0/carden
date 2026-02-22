module WebApi.AuthHandlers

open Giraffe
open Microsoft.AspNetCore.Http

let handlePingAuth: HttpHandler = text "Hello World, from Giraffe"
let handleSignUp: HttpHandler = text "Hello World, from Giraffe"
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
