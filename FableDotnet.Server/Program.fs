open System
open Fable.SignalR
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.Extensions.Logging

module SignalRHub =
    open Fable.SignalR
    open FSharp.Control.Tasks.V2
    open FableShared.SignalRHub

    let update (msg: Action) =
        match msg with
        | Action.OnConnect i -> Response.Connected(sprintf "{%i} connected" i)

    let invoke (msg: Action) (hubContext: FableHub) =
        // This works
        printfn "invoked msg: {%O}" msg
        task { return update msg }

    let send (msg: Action) (hubContext: FableHub<Action, Response>) =
        // This never gets called
        printfn "Recevied msg: {%O}" msg
        update msg |> hubContext.Clients.Caller.Send

let webApp = choose [ route "/ping" >=> text "pong" ]

let settings =
    { SignalR.Settings.EndpointPattern = FableShared.Endpoints.Root
      SignalR.Settings.Send = SignalRHub.send
      SignalR.Settings.Invoke = SignalRHub.invoke
      SignalR.Settings.Config =
          Some
              { SignalR.Config.Default<FableShared.SignalRHub.Action, FableShared.SignalRHub.Response>() with
                    UseMessagePack = false } }

type Startup() =
    member __.ConfigureServices(services: IServiceCollection) =
        services.AddGiraffe() |> ignore
        services.AddSignalR(settings) |> ignore

    member __.Configure (app: IApplicationBuilder) (env: IHostEnvironment) (loggerFactory: ILoggerFactory) =
        app.UseSignalR(settings) |> ignore
        app.UseGiraffe webApp

[<EntryPoint>]
let main _ =
    Host
        .CreateDefaultBuilder()
        .ConfigureWebHostDefaults(fun webHostBuilder -> webHostBuilder.UseStartup<Startup>() |> ignore)
        .Build()
        .Run()

    0
