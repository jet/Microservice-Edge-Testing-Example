module Controllers

open Newtonsoft.Json
open Producer.Domain.Types
open Producer.Logic.Quantity

let getItem (db: ISkuDatabase) (req: Producer.Domain.Types.GetItemRequest) =
    match db.GetSku req.sku with
    | Some item -> GetItemResponse.Success item
    | None -> GetItemResponse.NotFound

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

let updatePrice (db: ISkuDatabase) (req: Producer.Domain.Types.SetPriceRequest) =
    req.sku
    |> db.GetSku
    |> (function
        | Some itemState ->
            { itemState with price = req.price }
            |> db.UpdateSku
        | None -> () // probably log here too...
    )