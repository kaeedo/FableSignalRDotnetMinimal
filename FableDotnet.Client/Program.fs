open Fable.SignalR
open FableShared
open Microsoft.Extensions.Logging
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Hosting
open Program

[<EntryPoint>]
let main argv =
    async {
        let! host =
            App.app
                .ConfigureWebHost(fun webBuilder -> webBuilder.UseTestServer() |> ignore)
                .StartAsync()
            |> Async.AwaitTask

        let testServer = host.GetTestServer()

        let hub: HubConnection<SignalRHub.Action, _, _, SignalRHub.Response, _> =
            SignalR.Connect<SignalRHub.Action, _, _, SignalRHub.Response, _>(fun hub ->
                hub
                    .WithUrl(sprintf "http://127.0.0.1:5000%s" Endpoints.Root,
                             (fun o -> o.HttpMessageHandlerFactory <- (fun _ -> testServer.CreateHandler())))
                    .ConfigureLogging(fun l -> l.SetMinimumLevel(LogLevel.Debug))
                    .UseMessagePack())
            
        do! hub.Start()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State

        let! res = hub.Invoke <| SignalRHub.Action.OnConnect 1
        printfn "responsed: {%O}" res
        do! Async.Sleep(1000)
        
        printfn "-------"


        do! hub.Send <| SignalRHub.Action.OnConnect 1

        use a =
            hub.OnMessage(fun b -> async { printfn "onmessage received: %O" b })

        printfn "%O" a

        do! Async.Sleep(1000)

        do! hub.Stop()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State
    }
    |> Async.RunSynchronously

    0
