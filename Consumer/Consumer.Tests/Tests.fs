namespace Tests

open Xunit
open Consumer.Service
open Consumer.Domain.Types
open ProducerFake.Client
open Producer.Domain.Types

type ``Overstock functionality`` () = 

    [<Fact>]
    let ``Overstocking persists`` () =
        let client = (new ConsumerService (new ProducerClientEdgeFake ())) :> IConsumerApi
        let overstockResult =
            async {
                return! client.Overstocked "a" 2
            } |> Async.RunSynchronously
        
        Assert.Equal(UpdateQuantityResponse.Updated { ItemState.sku = "a"; quantity = 3; price = 1.0m }, overstockResult)

