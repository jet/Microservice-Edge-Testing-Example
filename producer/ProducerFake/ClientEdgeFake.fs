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
    let mutable underDuress = false
    let mutable badData = false

    let handleDuress () =
        if underDuress
        then
            System.Threading.Thread.Sleep(2000)
        else
            ()

    let throwForBadData () =
        if badData
        then
            // Force the same exception the client libraries would throw on bad data
            // better practice would be to return a 'a option instead of throwing everywhere
            "{BADDATA}"
            |> JsonConvert.DeserializeObject
            |> ignore
        else
            ()

    /// Handle failure states that can be handled independently
    let handleFailures () =
        handleDuress ()
        throwForBadData ()


    // The fake edge exposes ways to simulate failure states or conditions of the producer
    // This could be poor network connectivity, degraded experience, a downed DB, or any other prod issue

    /// Simulates the inability to access the database
    member x.DatabaseIsDown status =
        databaseDown <- status

    /// Simulates a high load or latency issues for the client
    member x.UnderDuress status =
        underDuress <- status

    /// Simulates either mismatched client / server versions or the server sending garbage data
    member x.BadData status =
        badData <- status

    interface IProviderApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            handleFailures ()
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
            handleFailures ()
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
            handleFailures ()
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
