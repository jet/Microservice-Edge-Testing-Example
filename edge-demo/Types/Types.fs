module Producer.Domain.Types

// General

type Sku = string

type ItemState = {
    sku: Sku
    quantity: int
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

type UpdateQuanityResponse =
    | Updated of ItemState
    | Failed of Sku * int
    | NotFound of string

type GetItemResponse =
    | Success of ItemState
    | Failed
    | NotFound

// Interfaces

type IProducerApi =
    //abstract member UpdateQuantity: UpdateQuantityRequest -> unit

    abstract member GetItem: GetItemRequest -> Async<GetItemResponse>
    // TODO find out AsyncSeq Type
    // abstract member SubscribeToUpdateQuantity: AsyncSeq<UpdateQuantityResponse>

