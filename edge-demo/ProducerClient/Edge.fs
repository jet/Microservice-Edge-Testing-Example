module ProviderClient.Client

open System
open System.Net
open System.Net.Http
open FSharp.Data
open Newtonsoft.Json
open Helpers
open Provider.Domain.Constants
open Provider.Domain.Types
open Kafunk

type ProviderClientEdge (path:Uri) =
    let conn = Kafka.connHost kafkaHost
    
    let producerCfg =
        ProducerConfig.create (
            topic = "priceupdate-topic", 
            partition = Partitioner.roundRobin, 
            requiredAcks = RequiredAcks.Local)

    let kafkaProvider =
        Producer.createAsync conn producerCfg
        // probably wouldn't have this sync in a real prod environment
        |> Async.RunSynchronously


    interface IProviderApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return
                request
                |> Helpers.httpPost (sprintf "%s%s" path.AbsoluteUri Provider.Domain.Constants.itemRoute)
                |> (fun (resText: string) -> JsonConvert.DeserializeObject<GetItemResponse> resText)
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            return
                request
                |> Helpers.httpPost (sprintf "%s%s" path.AbsoluteUri Provider.Domain.Constants.updateQuantityRoute)
                |> (fun (resText: string) -> JsonConvert.DeserializeObject<UpdateQuantityResponse> resText)
        }

        member x.SetPrice request =
            let prodRes =
                request
                |> JsonConvert.SerializeObject
                |> ProducerMessage.ofString
                |> Producer.produce kafkaProvider
                |> Async.RunSynchronously

            printfn "partition=%i offset=%i" prodRes.partition prodRes.offset

        member x.StateChange () =
            let event = new Event<ItemState> ()

            let conn = Kafka.connHost kafkaHost

            let consumerConfig = 
                ConsumerConfig.create (
                    groupId = "consumer-group", 
                    topic = stateUpdateTopic)

            let consumer =
                Consumer.create conn consumerConfig

            let last =
                Offsets.offsetRange conn stateUpdateTopic [0]
                |> Async.RunSynchronously
                |> Map.find 0 
                |> snd

            // TODO find a better way to start up and set offsets
            Consumer.commitOffsets consumer [| 0, 0L |]
            |> Async.RunSynchronously 

            Consumer.consume consumer 
                (fun (s:ConsumerState) (ms:ConsumerMessageSet) -> async {
                    ms.messageSet.messages
                    |> Array.map (fun message ->
                        let value = message.message.value
                        let deserialized =
                            value.Array.[value.Offset .. (max 0 value.Offset + value.Count - 1)]
                            |> System.Text.Encoding.ASCII.GetString
                            |> (fun (resText: string) -> JsonConvert.DeserializeObject<ItemState> resText)
                        printfn "on %A recieved %A" stateUpdateTopic deserialized
                        
                        event.Trigger deserialized
                    )
                    |> ignore
                    do! Consumer.commitOffsets consumer (ConsumerMessageSet.commitPartitionOffsets ms)
                })
            |> Async.Start

            event.Publish
