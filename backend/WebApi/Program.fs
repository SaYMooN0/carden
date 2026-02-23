open System
open System.Data
open System.Text.Json
open System.Text.Json.Serialization
open Dapper
open Domain.Errs
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Npgsql
open WebApi
open WebApi.Repositories
open WebApi.Types



let handleNoMatchedEndpoint: HttpHandler =
    fun next ctx ->
        let method = ctx.Request.Method
        let route = ctx.Request.Path.Value
        let response = ErrResponse.NoMatchingEndpoint method route
        json response next ctx

let webApp =
    choose [ subRoute "/auth" AuthHandlers.handlers; handleNoMatchedEndpoint ]

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "Unhandled exception")

    clearResponse
    >=> setStatusCode 500
    >=> json (
        ErrResponse.create (DomainErr ErrCode.ProgramBug) "Server error"
        |> ErrResponse.setAdditionalData {| ExceptionMsg = ex.Message |}
    )

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler).UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    services
        .AddSingleton<JsonSerializerOptions>(fun _ ->
            let opts = JsonSerializerOptions(JsonSerializerDefaults.Web)
            opts.Converters.Add(JsonFSharpConverter())
            opts)
        .AddTransient<ConnectionFactory>()
        .AddTransient<UsersRepository>()
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
