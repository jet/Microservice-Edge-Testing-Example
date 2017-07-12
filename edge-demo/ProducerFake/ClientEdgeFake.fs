module ProducerFake.Client

open FSharp.Data
open Newtonsoft.Json
open Producer.Domain.Constants
open Producer.Domain.Types
open Producer.Logic.Quantity
open DatabaseMock.SkuStorage

type ProducerClientEdgeFake () =

    let database = (new FakeDatabase ()) :> ISkuDatabase

    let stateChangeEvent = new Event<ItemState> ()

    interface IProducerApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return
                request.sku
                |> database.GetSku
                |> function
                   | Some state -> GetItemResponse.Success state
                   | None -> GetItemResponse.NotFound
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            let res =
                request.sku
                |> database.GetSku
                |> UpdateQuantity
                <| request.action

            match res with
            | UpdateQuantityResponse.Updated state -> database.UpdateSku state
            | _ -> ()

            return res
        }

        member x.SetPrice request =
            request.sku
            |> database.GetSku
            |> (function
                | Some itemState ->
                    let newState = 
                        { itemState with price = request.price }
                    if newState <> itemState
                    then
                        newState |> database.UpdateSku
                        newState |> stateChangeEvent.Trigger
                    else
                        ()
                | None -> ()
            )

        member x.StateChange () =
            stateChangeEvent.Publish
