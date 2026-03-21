open System
open System.Net
open System.Text.Json
open System.Text.Json.Serialization
open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open WebApi
open WebApi.BackendResponse
open WebApi.EmailService
open WebApi.JwtToken
open WebApi.Repositories

let handleNoMatchedEndpoint: HttpHandler =
    fun next ctx ->
        let method = ctx.Request.Method
        let route = ctx.Request.Path.Value

        let err =
            BackendResponseErr.create "Endpoint not found"
            |> BackendResponseErr.SetExtraData.noMatchedEndpoint {| Method = method; Route = route |}

        constructFailure HttpStatusCode.NotFound [ err ] next ctx

let webApp =
    choose
        [ subRoute "/auth" AuthHandlers.handlers
          subRoute "/plants" PlantsHandlers.handlers
          handleNoMatchedEndpoint ]

let errorHandler (ex: Exception) (logger: ILogger) =
    fun next ctx ->
        logger.LogError(ex, "Unhandled exception")

        let err =
            BackendResponseErr.create "Server error"
            |> BackendResponseErr.SetExtraData.serverException ex

        clearResponse >=> constructFailure HttpStatusCode.InternalServerError [ err ]
        <| next
        <| ctx

let configureApp (app: IApplicationBuilder) =
    app.UseGiraffeErrorHandler(errorHandler).UseGiraffe webApp

let getTypedConfig<'a> (context: WebHostBuilderContext) (fieldName: string) =
    context.Configuration.GetSection(fieldName).Get<'a>()

let addServices (context: WebHostBuilderContext) (services: IServiceCollection) =

    services
        .AddSingleton(getTypedConfig<FrontendConfig> context "FrontendConfig")
        
        .AddTransient<ConnectionFactory>()
        .AddTransient<UsersRepository>()
        .AddTransient<PlantsRepository>()
        .AddTransient<UnconfirmedUsersRepository>()

        .AddSingleton(getTypedConfig<EmailServiceConfig> context "EmailServiceConfig")
        .AddTransient<EmailService>()
        
        .AddSingleton(getTypedConfig<JwtTokenConfig> context "JwtSettings")
        .AddTransient<UserPassword.PasswordHasher>()
        .AddSingleton<JwtTokenService>()
        
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
        .ConfigureWebHostDefaults(fun webHostBuilder -> webHostBuilder.Configure(configureApp).ConfigureServices(addServices) |> ignore)
        .Build()
        .Run()

    0
