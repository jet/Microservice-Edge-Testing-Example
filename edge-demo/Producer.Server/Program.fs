open Suave
open Suave.Successful
open Suave.Web
open Suave.Operators
open Suave.Filters
open Newtonsoft.Json
open Newtonsoft.Json.Serialization
open System
open System.Threading.Tasks
open Producer.Domain.Types
open Producer.Logic.Quantity
open JsonStorage.SkuStorage

[<AutoOpen>]
module Helpers = 
    let toJson v =
        let jsonSerializerSettings = new JsonSerializerSettings()
        jsonSerializerSettings.ContractResolver <- new CamelCasePropertyNamesContractResolver()

        JsonConvert.SerializeObject(v, jsonSerializerSettings) |> OK
        >=> Writers.setMimeType "application/json; charset=utf-8"

    let fromJson<'a> json =
        JsonConvert.DeserializeObject(json, typeof<'a>) :?> 'a

    let getResourceFromReq<'a> (req : HttpRequest) =
        let getString rawForm =
            System.Text.Encoding.UTF8.GetString(rawForm)
        req.rawForm |> getString |> fromJson<'a>
        
    let inline startAsPlainTask (work : Async<unit>) = Task.Factory.StartNew(fun () -> work |> Async.RunSynchronously)

let startServer (db: ISkuDatabase) =
    let getItem (req: Producer.Domain.Types.GetItemRequest) =
        (match db.GetSku req.sku with
        | Some item -> GetItemResponse.Success item
        | None -> GetItemResponse.NotFound)
        |> JsonConvert.SerializeObject
        |> OK

    let handleUpdateQuantity (req: Producer.Domain.Types.UpdateQuantityRequest) =
        let res =
            req.sku
            |> db.GetSku 
            |> UpdateQuantity
            <| req.action


        match res with
        | UpdateQuantityResponse.Updated state -> db.UpdateSku state
        | _ -> ()

        res
        |> JsonConvert.SerializeObject
        |> OK

    let getItemResponse = getResourceFromReq >> getItem
    let updateQuantityResponse = getResourceFromReq >> handleUpdateQuantity

    let app =
        choose
            [ POST >=> choose
                [ path Producer.Domain.Constants.itemRoute >=> request getItemResponse
                  path Producer.Domain.Constants.updateQuantityRoute >=> request updateQuantityResponse ]
            ]

    startWebServer defaultConfig app


[<EntryPoint>]
let main argv =
    (new JsonDatabase ()) :> ISkuDatabase
    |> startServer
    0