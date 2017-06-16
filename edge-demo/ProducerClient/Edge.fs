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
            let res =
                request
                // |> JsonConvert.SerializeObject
                |> Helpers.httpPost ("http://localhost:8080" + Producer.Domain.Constants.itemRoute)
            let data =
                {
                    ItemState.quantity = 1
                    sku = res
                }
            return GetItemResponse.Success data
        }