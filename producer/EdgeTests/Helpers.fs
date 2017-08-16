module Helpers

open Provider.Domain.Types
open Provider.DirectEdge
open ProviderFake.Client
open DatabaseMock.SkuStorage

let testBoth test =
    [
        (new ProviderDirectEdge (new FakeDatabase ())) :> IProviderApi
        (new ProviderClientEdgeFake ()) :> IProviderApi
    ]
    |> List.iter test
