module JsonStorage.SkuStorage

open System
open System.IO
open Producer.Domain.Types
open Newtonsoft.Json

// This file represents an adapter for whatever database you are using
// this would be calling mongo, docdb, whatever, but is using
// the filesystem to decrease dependencies for this demo


type Database = Map<string, ItemState>

type JsonDatabase () =
    let databaseFile = "database.json"

    let getDatabase () : Database =
        File.ReadAllText(databaseFile)
        |> (fun (resText: string) -> JsonConvert.DeserializeObject<Database> resText)

    let writeDatabase (database: Database) =
        database
        |> JsonConvert.SerializeObject
        |> (fun text -> File.WriteAllText(databaseFile, text))

    interface ISkuDatabase with
        member x.GetSku sku =
            ()
            |> getDatabase
            |> Map.tryFind sku

        member x.UpdateSku (item: ItemState) =
            ()
            |> getDatabase
            |> Map.add item.sku item
            |> writeDatabase
    