open System
open Fable.SignalR
open Saturn
open FSharp.Control.Tasks.V2
open FableShared.SignalRHub

module SignalRHub =
    let update (msg: Action) =
        match msg with
        | Action.OnConnect i -> Response.Connected(sprintf "{%i} connected" i)

    let invoke (msg: Action) (hubContext: FableHub) =
        printfn "invoked msg: {%O}" msg
        task { return update msg }

    let send (msg: Action) (hubContext: FableHub<Action, Response>) =
        printfn "Recevied msg: {%O}" msg
        update msg |> hubContext.Clients.Caller.Send

module App =
    let app =
        application {
            url "http://0.0.0.0:5000"
            no_router
            use_developer_exceptions
            use_signalr (
                configure_signalr{
                    endpoint FableShared.Endpoints.Root
                    send SignalRHub.send
                    invoke SignalRHub.invoke
                    use_messagepack
                    with_hub_options (fun ho -> ho.EnableDetailedErrors <- Nullable<bool>(true))
                }
            )
        }
    [<EntryPoint>]
    let main _ =
        run app
    
        0