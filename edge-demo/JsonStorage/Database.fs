module JsonStorage.SkuStorage

open System
open System.IO
open Producer.Domain.Types
open Newtonsoft.Json

type Database = Map<string, ItemState>

let databaseFile = "database.json"

let getDatabase () : Database =
    File.ReadAllText(databaseFile)
    |> (fun (resText: string) -> JsonConvert.DeserializeObject<Database> resText)

let writeDatabase (database: Database) =
    database
    |> JsonConvert.SerializeObject
    |> (fun text -> File.WriteAllText(databaseFile, text))

let getSku sku =
    ()
    |> getDatabase
    |> Map.tryFind sku

let updateSku (item: ItemState) =
    ()
    |> getDatabase
    |> Map.add item.sku item
    |> writeDatabase
    