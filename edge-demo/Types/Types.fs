module Producer.Domain.Types

// General

type Sku = string

type ItemState = {
    sku: Sku
    quantity: int
    price: decimal
}

type DatabaseReadResult =
    | Success of ItemState
    | NotFound
    | Failed

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

type SetPriceRequest = {
    sku: Sku
    price: decimal
}

// Responses

type UpdateQuantityResponse =
    | Updated of ItemState
    | Failed
    | NotFound

type GetItemResponse =
    | Success of ItemState
    | Failed
    | NotFound

// Interfaces

type IProducerApi =
    /// Increases, decreases, or sets the value of quantity for a sku
    abstract member UpdateQuantity: UpdateQuantityRequest -> Async<UpdateQuantityResponse>

    /// Get the current state of an item by sku
    abstract member GetItem: GetItemRequest -> Async<GetItemResponse>

    // TODO find out AsyncSeq Type
    // abstract member SubscribeToUpdateQuantity: AsyncSeq<UpdateQuantityResponse>

    /// Set the exact price of a sku
    abstract member SetPrice: SetPriceRequest -> unit

    /// Alert on price changes
    abstract member StateChange: unit -> IEvent<ItemState>

type ISkuDatabase =
    /// Given a sku this will return the current state of the item
    abstract member GetSku: Sku -> DatabaseReadResult

    /// Given a state for an item, this will update the current sku
    /// in the db to have the given state
    abstract member UpdateSku: ItemState -> unit
