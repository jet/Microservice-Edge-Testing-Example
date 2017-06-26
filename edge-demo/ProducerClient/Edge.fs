module ProducerClient.Client

open System
open System.Net
open System.Net.Http
open FSharp.Data
open Newtonsoft.Json
open Helpers
open Producer.Domain.Constants
open Producer.Domain.Types

type ProducerClientEdge (path:Uri) =
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
