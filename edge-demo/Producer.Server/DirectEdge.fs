module Producer.DirectEdge

// This is exposing an "edge" contract api that should be the
// same as calling this service using the distributed edge,
// only without going over a protocol like http or kafka

//TODO-Rand: consider whether or not this belongs to edge testing

open Controllers
open Producer.Logic.Quantity
open Producer.Domain.Types

type ProducerInternalEdge (database: ISkuDatabase) =
    interface IProducerApi with
        member x.GetItem (request: GetItemRequest) : Async<GetItemResponse> = async {
            return getItem database request
        }

        member x.UpdateQuantity (request: UpdateQuantityRequest) = async {
            return handleUpdateQuantity database request
        }
