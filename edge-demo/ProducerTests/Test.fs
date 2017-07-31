namespace ProviderTests.Tests

open System
open Xunit
open Provider.Domain.Types
open Provider.Logic.Quantity
open ProviderFake.Client
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
            |> DatabaseReadResult.Success
            |> UpdateQuantity
            <| Decrement 3

        Assert.Equal(UpdateQuantityResponse.Failed, shouldFail)


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
            |> DatabaseReadResult.Success
            |> UpdateQuantity
            <| Decrement 2
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "testsku"; price = 1.0m; quantity = 0 }, shouldFail)


