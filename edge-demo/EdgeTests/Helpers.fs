module Helpers

open Producer.Domain.Types
open Producer.DirectEdge
open ProducerFake.Client
open DatabaseMock.SkuStorage

let testBoth test =
    [
        (new ProducerInternalEdge (new FakeDatabase ())) :> IProducerApi
        (new ProducerClientEdgeFake ()) :> IProducerApi
    ]
    |> List.iter test
