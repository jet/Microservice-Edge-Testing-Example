module JsonStorage.SkuStorage

open System
open System.IO
open Newtonsoft.Json
open Producer.Domain.Types

// This file represents an adapter for whatever database you are using
// this would be calling mongo, docdb, whatever, but is using
// the filesystem to decrease dependencies for this demo

//TODO: since this database gets mutated upon use, you might want to ship your demo with a special database.json file that can be used as the initial database (and provide brief instructions about using it again to reset the database). I also considered having a database initializer function, but that's meta code that might get mistaken for real code.

type Database = Map<string, ItemState>

type JsonDatabase () =
    let [<Literal>] databaseFile = "database.json"

    let getDatabase () : Database =
        File.ReadAllText(databaseFile)
        |> (fun (resText: string) -> JsonConvert.DeserializeObject<Database> resText) //TODO: rename "resText" to something more descriptive

    let writeDatabase (database: Database) =
        database
        |> JsonConvert.SerializeObject
        |> (fun text -> File.WriteAllText(databaseFile, text))

    interface ISkuDatabase with
        member x.GetSku sku =
            getDatabase ()
            |> Map.tryFind sku

        member x.UpdateSku (item: ItemState) =
            getDatabase ()
            |> Map.add item.sku item
            |> writeDatabase
