open System
open System.Threading.Tasks
open Suave
open Suave.Successful
open Suave.Web
open Suave.Operators
open Suave.Filters
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open Producer.Domain.Types
open JsonStorage.SkuStorage
open Controllers
open FSharp.Control
open Kafunk
open System

[<AutoOpen>]
module Helpers =
    let toJson v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
        JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm =
            System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>

    let inline startAsPlainTask (work : Async<unit>) = Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

let startServer (db: ISkuDatabase) =
    let getItemResponse = getResourceFromReq >> getItem db >> JsonConvert.SerializeObject >> OK
    let updateQuantityResponse = getResourceFromReq >> handleUpdateQuantity db >> JsonConvert.SerializeObject >> OK

    let app =
        choose
            [ POST >=> choose
                [ path Producer.Domain.Constants.itemRoute >=> request getItemResponse
                  path Producer.Domain.Constants.updateQuantityRoute >=> request updateQuantityResponse ]
            ]

    startWebServer defaultConfig app

let createAsyncConsumer (db: ISkuDatabase) =
    let conn = Kafka.connHost "localhost:9092"

    let consumerConfig = 
        ConsumerConfig.create (
            groupId = "consumer-group", 
            topic = "priceupdate-topic")

    let consumer =
        Consumer.create conn consumerConfig

    Consumer.consume consumer 
        (fun (s:ConsumerState) (ms:ConsumerMessageSet) -> async {
            printfn "member_id=%s assignment_strategy=%s topic=%s partition=%i" 
                s.memberId s.protocolName ms.topic ms.partition
            ms.messageSet.messages
            |> Array.map (fun message ->
                let req =
                    message.message.value.Array.[message.message.value.Offset..message.message.value.Offset + message.message.value.Count - 1]
                    |> System.Text.Encoding.ASCII.GetString
                    |> (fun (resText: string) -> JsonConvert.DeserializeObject<SetPriceRequest> resText)
                printfn "recieved %A" req
                
                req
                |> updatePrice db
            )
            |> ignore
            do! Consumer.commitOffsets consumer (ConsumerMessageSet.commitPartitionOffsets ms)
        })

[<EntryPoint>]
let main argv =
    let db = 
        (new JsonDatabase ()) :> ISkuDatabase

    db
    |> createAsyncConsumer
    |> Async.Start
    
    db
    |> startServer
    0
