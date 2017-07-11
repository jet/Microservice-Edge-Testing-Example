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
open Producer.Domain.Constants
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
    let conn = Kafka.connHost kafkaHost

    let consumerConfig =
        ConsumerConfig.create (
            groupId = "consumer-group",
            topic = priceUpdateTopic)

    let consumer =
        Consumer.create conn consumerConfig

    // TODO find a better way to start up and set offsets
    Consumer.commitOffsets consumer [| 0, 0L |]
    |> Async.RunSynchronously 

    let producerCfg =
        ProducerConfig.create (
            topic = stateUpdateTopic,
            partition = Partitioner.roundRobin,
            requiredAcks = RequiredAcks.Local)

    let producer =
        Producer.createAsync conn producerCfg
        // probably wouldn't have this sync in a real prod environment
        |> Async.RunSynchronously

    let broadcastStateChange =
        JsonConvert.SerializeObject
        >> ProducerMessage.ofString
        >> Producer.produce producer
        >> Async.Ignore
        >> Async.RunSynchronously

    Consumer.consume consumer 
        (fun (s:ConsumerState) (ms:ConsumerMessageSet) -> async {
            printfn "member_id=%s assignment_strategy=%s topic=%s partition=%i" 
                s.memberId s.protocolName ms.topic ms.partition
            ms.messageSet.messages
            |> Array.map (fun message ->
                let value = message.message.value
                let req =
                    value.Array.[value.Offset .. (max 0 value.Offset + value.Count - 1)]
                    |> System.Text.Encoding.ASCII.GetString
                    |> (fun (resText: string) -> JsonConvert.DeserializeObject<SetPriceRequest> resText)
                printfn "recieved %A" req
                
                req
                |> updatePrice db broadcastStateChange
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
