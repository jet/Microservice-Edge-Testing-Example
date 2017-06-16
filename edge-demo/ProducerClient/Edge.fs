module ProducerClient.Client

open FSharp.Data
open Producer.Domain.Constants
open Producer.Domain.Types
open Newtonsoft.Json
open System
open System.Net
open System.Net.Http
open Helpers

type ProducerClientEdge () =
    interface IProducerApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return
                request
                |> Helpers.httpPost ("http://localhost:8080" + Producer.Domain.Constants.itemRoute)
                |> (fun (resText: string) -> JsonConvert.DeserializeObject<GetItemResponse> resText)
        }