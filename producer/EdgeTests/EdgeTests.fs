namespace EdgeTests.Tests

open System
open Xunit
open Provider.Domain.Types
open Provider.Logic.Quantity
open DatabaseMock.SkuStorage
open Helpers

type ``Quantity Updates And Persists Correctly`` () =
    [<Fact>]
    let ``Incrementing quantity persists`` () =
        let test (edge: IProviderApi) =
            let result =
                { UpdateQuantityRequest.sku = "a"; action = UpdateQuantityAction.Increment 2 }
                |> edge.UpdateQuantity
                |> Async.RunSynchronously
            Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "a"; price = 1.0m; quantity = 3 }, result)

            let state =
                { GetItemRequest.sku = "a" }
                |> edge.GetItem
                |> Async.RunSynchronously
            Assert.Equal(GetItemResponse.Success { ItemState.sku = "a"; price = 1.0m; quantity = 3 }, state)

        testBoth test


    [<Fact>]
    let ``Decrementing quantity doesn't go past 0`` () =
        let test (edge: IProviderApi) =
            let result =
                { UpdateQuantityRequest.sku = "a"; action = UpdateQuantityAction.Decrement 2 }
                |> edge.UpdateQuantity
                |> Async.RunSynchronously
            Assert.Equal(UpdateQuantityResponse.Failed, result)

        testBoth test

    [<Fact>]
    let ``Updating Price Emits Event Response`` () =
        let test (edge: IProviderApi) =
            let priceEvent = edge.StateChange ()
            let mutable hasTriggeredListener = false

            priceEvent.Add (fun _ -> hasTriggeredListener <- true)

            { SetPriceRequest.sku = "a"; price = 70m}
            |> edge.SetPrice 

            Assert.Equal(true, hasTriggeredListener)

        testBoth test
