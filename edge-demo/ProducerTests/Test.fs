namespace ProducerTests.Tests

open System
open Producer.Domain.Types
open Producer.Logic.Quantity
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
