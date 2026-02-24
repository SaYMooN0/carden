open System
open System.Net
open System.Text.Json
open System.Text.Json.Serialization
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open WebApi
open WebApi.BackendResponse
open WebApi.Repositories
open WebApi.UserPassword



let handleNoMatchedEndpoint: HttpHandler =
    fun next ctx ->
        let method = ctx.Request.Method
        let route = ctx.Request.Path.Value

        let err =
            BackendResponseErr.create "Endpoint not found"
            |> BackendResponseErr.SetAdditionalData.noMatchedEndpoint {| Method = method; Route = route |}

        constructFailure HttpStatusCode.NotFound [ err ] next ctx

let webApp =
    choose [ subRoute "/auth" AuthHandlers.handlers; handleNoMatchedEndpoint ]

let errorHandler (ex: Exception) (logger: ILogger) =
    fun next ctx ->
        logger.LogError(ex, "Unhandled exception")

        let err =
            BackendResponseErr.create "Server error"
            |> BackendResponseErr.SetAdditionalData.serverException ex

        clearResponse >=> constructFailure HttpStatusCode.InternalServerError [ err ]
        <| next
        <| ctx

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler).UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services
        .AddTransient<ConnectionFactory>()
        .AddTransient<UsersRepository>()
        .AddTransient<WebApi.UserPassword.PasswordHasher>()
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
