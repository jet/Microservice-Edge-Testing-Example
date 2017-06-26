namespace ProducerTests.Tests

open System
open Xunit
open Producer.Domain.Types
open Producer.Logic.Quantity
open ProducerFake.Client
open DatabaseMock.SkuStorage

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


