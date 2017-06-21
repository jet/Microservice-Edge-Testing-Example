namespace ProducerTests.Tests

open System
open Producer.Domain.Types
open Producer.Logic.Quantity
open Producer.DirectEdge
open DatabaseMock.SkuStorage
open Xunit


type ``Quantity Updates Correctly`` () =
    
    [<Fact>]
    let ``Decrementing quantity past 0 doesn't change state`` () =
        let initialState =
            {
                ItemState.sku = "testsku"
                quantity = 2
            }
        let shouldFail =
            initialState
            |> Some
            |> UpdateQuantity
            <| Decrement 3
        Assert.Equal(UpdateQuantityResponse.Failed initialState, shouldFail)


    [<Fact>]
    let ``Decrementing quantity to 0 works correctly`` () =
        let initialState =
            {
                ItemState.sku = "testsku"
                quantity = 2
            }
        let shouldFail =
            initialState
            |> Some
            |> UpdateQuantity
            <| Decrement 2
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "testsku"; quantity = 0 }, shouldFail)

    [<Fact>]
    let ``Incrementing quantity persists`` () =
        let edge =
            (new ProducerInternalEdge (new FakeDatabase ())) :> IProducerApi

        let result =
            { UpdateQuantityRequest.sku = "a"; action = UpdateQuantityAction.Increment 2 }
            |> edge.UpdateQuantity
            |> Async.RunSynchronously
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "a"; quantity = 3 }, result)

        let state =
            { GetItemRequest.sku = "a" }
            |> edge.GetItem
            |> Async.RunSynchronously
        Assert.Equal(GetItemResponse.Success { ItemState.sku = "a"; quantity = 3 }, state)

    [<Fact>]
    let ``Decrementing quantity doesn't go past 0`` () =
        let edge =
            (new ProducerInternalEdge (new FakeDatabase ())) :> IProducerApi

        let result =
            { UpdateQuantityRequest.sku = "a"; action = UpdateQuantityAction.Decrement 2 }
            |> edge.UpdateQuantity
            |> Async.RunSynchronously
        Assert.Equal(UpdateQuantityResponse.Failed { ItemState.sku = "a"; quantity = 1 }, result)

