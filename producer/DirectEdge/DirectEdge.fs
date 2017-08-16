module Provider.DirectEdge

// This is exposing an "edge" contract api that should be the
// same as calling this service using the distributed edge,
// only without going over a protocol like http or kafka

open Controllers
open Provider.Domain.Types

type ProviderDirectEdge (database: ISkuDatabase) =
    let stateChangeEvent = new Event<ItemState> ()

    interface IProviderApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return getItem database request
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            return handleUpdateQuantity database request
        }

        member x.SetPrice request =
            request
            |> updatePrice database stateChangeEvent.Trigger

        member x.StateChange () =
            stateChangeEvent.Publish