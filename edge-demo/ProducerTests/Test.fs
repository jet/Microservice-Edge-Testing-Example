namespace ProducerTests.Tests

open System
open Producer.Domain.Types
open Producer.Logic.Quantity
open Producer.DirectEdge
open ProducerFake.Client
open DatabaseMock.SkuStorage
open Xunit


type ``Quantity Updates Correctly`` () =
    
    [<Fact>]
    let ``Decrementing quantity past 0 doesn't change state`` () =
        let initialState =
            {
                ItemState.sku = "testsku"
                quantity = 2
                price = 1.0m
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
                price = 1.0m
            }
        let shouldFail =
            initialState
            |> Some
            |> UpdateQuantity
            <| Decrement 2
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "testsku"; price = 1.0m; quantity = 0 }, shouldFail)


    let testBoth test =
        [
            (new ProducerInternalEdge (new FakeDatabase ())) :> IProducerApi
            (new ProducerClientEdgeFake ()) :> IProducerApi
        ]
        |> List.iter test 
        
    [<Fact>]
    let ``Incrementing quantity persists`` () =
        let test (edge: IProducerApi) =
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
        let test (edge: IProducerApi) =
            let result =
                { UpdateQuantityRequest.sku = "a"; action = UpdateQuantityAction.Decrement 2 }
                |> edge.UpdateQuantity
                |> Async.RunSynchronously
            Assert.Equal(UpdateQuantityResponse.Failed { ItemState.sku = "a"; price = 1.0m; quantity = 1 }, result)

        testBoth test

