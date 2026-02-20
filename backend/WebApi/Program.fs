open System
open System.Text.Json
open System.Text.Json.Serialization
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging


let sayHelloWorld: HttpHandler = text "Hello World, from Giraffe"
let sayHi: HttpHandler = text "Hiiiiiiiiiii"

let handleNoMatchedEndpoint: HttpHandler =
    RequestErrors.NOT_FOUND "Endpoint not found"

let webApp =
    choose
        [ GET >=> choose [ route "/foo" >=> text "Foo" ]
          POST >=> choose [ route "/bar" >=> text "Bar" ]
          handleNoMatchedEndpoint ]
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
let errorHandler (ex: Exception) (_: ILogger) =
    clearResponse >=> ServerErrors.INTERNAL_ERROR ex.Message

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler).UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services
        .AddSingleton<JsonSerializerOptions>(fun _ ->
            let opts = JsonSerializerOptions(JsonSerializerDefaults.Web)
            opts.Converters.Add(JsonFSharpConverter())
            opts)
        .AddGiraffe()
    |> ignore

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder.Configure(configureApp).ConfigureServices(configureServices)
            |> ignore)
        .Build()
        .Run()

    0
