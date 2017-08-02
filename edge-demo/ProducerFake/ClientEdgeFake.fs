module ProviderFake.Client

open FSharp.Data
open Newtonsoft.Json
open Provider.Domain.Constants
open Provider.Domain.Types
open Provider.Logic.Quantity
open DatabaseMock.SkuStorage

type ProviderClientEdgeFake () =

    let database = (new FakeDatabase ()) :> ISkuDatabase

    let stateChangeEvent = new Event<ItemState> ()

    let mutable databaseDown = false

    // The fake edge exposes ways to simulate failure states or conditions of the producer
    // This could be poor network connectivity, degraded experience, or in this case a downed DB
    member x.DatabaseIsDown status =
        databaseDown <- status

    interface IProviderApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return
                if databaseDown
                then
                    GetItemResponse.Failed
                else 
                    request.sku
                    |> database.GetSku
                    |> function
                       | DatabaseReadResult.Success state -> GetItemResponse.Success state
                       | DatabaseReadResult.NotFound -> GetItemResponse.NotFound
                       | DatabaseReadResult.Failed -> GetItemResponse.Failed
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            if databaseDown
            then
                return UpdateQuantityResponse.Failed
            else
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
            if databaseDown
            then
                ()
            else
                request.sku
                |> database.GetSku
                |> (function
                    | DatabaseReadResult.Success itemState ->
                        let newState = 
                            { itemState with price = request.price }
                        if newState <> itemState
                        then
                            newState |> database.UpdateSku
                            newState |> stateChangeEvent.Trigger
                        else
                            ()
                    | DatabaseReadResult.Failed
                    | DatabaseReadResult.NotFound -> ()
                )

        member x.StateChange () =
            stateChangeEvent.Publish
