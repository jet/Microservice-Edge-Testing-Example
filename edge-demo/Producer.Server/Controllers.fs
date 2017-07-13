module Controllers

open Newtonsoft.Json
open Producer.Domain.Types
open Producer.Logic.Quantity

let getItem (db: ISkuDatabase) (req: Producer.Domain.Types.GetItemRequest) =
    match db.GetSku req.sku with
    | DatabaseReadResult.Success item -> GetItemResponse.Success item
    | DatabaseReadResult.NotFound -> GetItemResponse.NotFound
    | DatabaseReadResult.Failed -> GetItemResponse.Failed

let handleUpdateQuantity (db: ISkuDatabase) (req: Producer.Domain.Types.UpdateQuantityRequest) =
    let res =
        req.sku
        |> db.GetSku
        |> UpdateQuantity
        <| req.action

    match res with
    | UpdateQuantityResponse.Updated state -> db.UpdateSku state
    | _ -> ()

    res

let updatePrice (db: ISkuDatabase) (transmitCallback: ItemState -> unit) (req: Producer.Domain.Types.SetPriceRequest) =
    req.sku
    |> db.GetSku
    |> (function
        | DatabaseReadResult.Success itemState ->
            let newState = 
                { itemState with price = req.price }

            if newState <> itemState
            then
                newState |> db.UpdateSku
                newState |> transmitCallback
            else
                ()
        | _ -> () // maybe log here or write another event
    )