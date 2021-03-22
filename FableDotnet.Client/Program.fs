open Fable.SignalR
open FableShared
open Microsoft.Extensions.Logging

[<EntryPoint>]
let main argv =
    let hub =
        SignalR.Connect<SignalRHub.Action, _, _, SignalRHub.Response, _>(fun hub ->
            hub
                .WithUrl(sprintf "http://127.0.0.1:5000%s" Endpoints.Root)
                //.UseMessagePack()
                .ConfigureLogging(fun l -> l.SetMinimumLevel(LogLevel.Debug)))

    // This works
    async {
        do! hub.Start()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State

        let! res = hub.Invoke <| SignalRHub.Action.OnConnect 1
        printfn "responsed: {%O}" res
        
        do! hub.Stop()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State
    }
    |> Async.RunSynchronously
    
    printfn "-------"
    
    // This doesn't work
    async {
        do! hub.Start()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State

        do! hub.Send <| SignalRHub.Action.OnConnect 1
        
        do! hub.Stop()
        printfn "ConnectionID: {%O}, State: {%O}" hub.ConnectionId hub.State
    }
    |> Async.RunSynchronously

    0
