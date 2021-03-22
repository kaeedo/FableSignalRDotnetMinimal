namespace FableShared

module SignalRHub =
    [<RequireQualifiedAccess>]
    type Action =
        | OnConnect of int

    [<RequireQualifiedAccess>]
    type Response =
        | Connected of string

[<RequireQualifiedAccess>]
module Endpoints =
    [<Literal>]
    let Root = "/gamehub"
