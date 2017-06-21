module Controllers

open Producer.Domain.Types
open Newtonsoft.Json
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

