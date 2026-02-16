open Giraffe
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting

let webApp =
    choose
        [ route "/ping" >=> text "pong"
          route "/pong" >=> htmlFile "/pages/index.html"
          route "/ok"
          >=> setHttpHeader "X-Foo" "Bar"
          >=> setStatusCode 200
          >=> setBodyFromString "Hello World"
          route "/not-ok" >=> setStatusCode 400 ]

let configureApp (app: IApplicationBuilder) =
    // Add Giraffe to the ASP.NET Core pipeline
    app.UseGiraffe webApp

let configureServices (services: IServiceCollection) =
    // Add Giraffe dependencies
    services.AddGiraffe() |> ignore

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
