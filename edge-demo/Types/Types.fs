module Producer.Domain.Types

// General

type Sku = string

type ItemState = {
    sku: Sku
    quantity: int
    price: decimal
}

// Requests

type UpdateQuantityAction =
    | Update of int
    | Increment of int
    | Decrement of int

type UpdateQuantityRequest = {
    sku: Sku
    action: UpdateQuantityAction
}

type GetItemRequest = {
    sku: Sku
}

// Responses

type UpdateQuantityResponse =
    | Updated of ItemState
    | Failed of ItemState
    | NotFound

type GetItemResponse =
    | Success of ItemState
    | Failed
    | NotFound

// Interfaces

type IProducerApi =
    abstract member UpdateQuantity: UpdateQuantityRequest -> Async<UpdateQuantityResponse>

    abstract member GetItem: GetItemRequest -> Async<GetItemResponse>
    // TODO find out AsyncSeq Type
    // abstract member SubscribeToUpdateQuantity: AsyncSeq<UpdateQuantityResponse>

type ISkuDatabase =
    /// Given a sku this will return the current state of the item
    abstract member GetSku: Sku -> ItemState option

    /// Given a state for an item, this will update the current sku
    /// in the db to have the given state
    abstract member UpdateSku: ItemState -> unit
