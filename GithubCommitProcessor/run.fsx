#r "System.Net.Http"
#r "Newtonsoft.Json"

open System.Net
open System.Net.Http
open Newtonsoft.Json
open FSharp.Data

[<Literal>]
let pushSample = __SOURCE_DIRECTORY__ + "/push.json"

type PushWebHook = JsonProvider<pushSample> 

let Run(req: HttpRequestMessage, log: TraceWriter) =
    async {
        log.Info(sprintf 
            "Handling a GitHub Webhook.")

        let! data = req.Content.ReadAsStringAsync() |> Async.AwaitTask

        log.Info(sprintf "Request received: %s" data)

        let webhook = PushWebHook.Parse(data)
        let repository = webhook.Repository.Name
        let username = webhook.Commits.[0].Author.Username
        return req.CreateResponse(HttpStatusCode.OK, sprintf "Processed webhook from %s from %s" repository username);
    } |> Async.RunSynchronously
