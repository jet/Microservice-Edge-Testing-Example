module JsonStorage.SkuStorage

open System
open System.IO
open Producer.Domain.Types
open Newtonsoft.Json

// This file represents an adapter for whatever database you are using
// this would be calling mongo, docdb, whatever, but is using
// the filesystem to decrease dependencies for this demo

type Database = Map<string, ItemState>

let databaseFile = "database.json"

let internal getDatabase () : Database =
    File.ReadAllText(databaseFile)
    |> (fun (resText: string) -> JsonConvert.DeserializeObject<Database> resText)

let internal writeDatabase (database: Database) =
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
    