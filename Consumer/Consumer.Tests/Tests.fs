namespace Tests

open Xunit
open Consumer.Service
open Consumer.Domain.Types
open ProviderFake.Client
open Provider.Domain.Types

type ``Core Service Functionality`` () =

    [<Fact>]
    let ``Overstocking persists`` () =
        let client = (new ConsumerService (new ProviderClientEdgeFake ())) :> IConsumerApi
        let overstockResult =
            async {
                return! client.Overstocked "a" 2
            } |> Async.RunSynchronously
        
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "a"; quantity = 3; price = 1.0m }, overstockResult)
        Assert.Equal(1, client.SalesToRun ())

    [<Fact>]
    let ``Don't expect to run a sale if overstock fails`` () =
        
        // Simulate a downed database for the producer
        let provider = new ProviderClientEdgeFake ()
        true |> provider.DatabaseStatus

        let client = (new ConsumerService (provider)) :> IConsumerApi

        let overstockResult =
            async {
                return! client.Overstocked "a" 2
            } |> Async.RunSynchronously
        
        Assert.Equal(UpdateQuantityResponse.Failed, overstockResult)
        Assert.Equal(0, client.SalesToRun ())

    [<Fact>]
    let ``NudgePriceDown doesn't nudge by too much when price is already low`` () =
        let producer = (new ProviderClientEdgeFake ()) :> IProviderApi
        let client = (new ConsumerService (producer)) :> IConsumerApi

        let mutable currentPrice = 1.0m

        (fun (state: ItemState) -> currentPrice <- state.price)
        |> (producer.StateChange ()).Add


        "a"
        |> client.NudgePriceDown
        |> Async.RunSynchronously

        Assert.Equal(0.5m, currentPrice)

        "a"
        |> client.NudgePriceDown
        |> Async.RunSynchronously


        Assert.Equal(0.45m, currentPrice)

