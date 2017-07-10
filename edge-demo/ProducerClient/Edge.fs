module ProducerClient.Client

open System
open System.Net
open System.Net.Http
open FSharp.Data
open Newtonsoft.Json
open Helpers
open Producer.Domain.Constants
open Producer.Domain.Types
open Kafunk

type ProducerClientEdge (path:Uri) =
    let conn = Kafka.connHost kafkaHost
    
    let producerCfg =
        ProducerConfig.create (
            topic = "priceupdate-topic", 
            partition = Partitioner.roundRobin, 
            requiredAcks = RequiredAcks.Local)

    let producer =
        Producer.createAsync conn producerCfg
        // probably wouldn't have this sync in a real prod environment
        |> Async.RunSynchronously

    interface IProducerApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return
                request
                //TODO: confirm that this absoluteUri works
                |> Helpers.httpPost (sprintf "%s%s" path.OriginalString Producer.Domain.Constants.itemRoute)
                |> (fun (resText: string) -> JsonConvert.DeserializeObject<GetItemResponse> resText)
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            return
                request
                |> Helpers.httpPost (sprintf "%s%s" path.OriginalString Producer.Domain.Constants.updateQuantityRoute)
                |> (fun (resText: string) -> JsonConvert.DeserializeObject<UpdateQuantityResponse> resText)
        }

        member x.SetPrice request =
            let prodRes =
                request
                |> JsonConvert.SerializeObject
                |> ProducerMessage.ofString
                |> Producer.produce producer
                |> Async.RunSynchronously

            printfn "partition=%i offset=%i" prodRes.partition prodRes.offset
                