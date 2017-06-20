module DatabaseMock.SkuStorage

open System
open System.IO
open Producer.Domain.Types

// This file is a fake database mock, that implements
// the interface of a real DB adapter but sits in memory


type Database = Map<string, ItemState>

type FakeDatabase () =
    let mutable database = [("a", { ItemState.sku = "a"; quantity = 1 })] |> Map<string, ItemState>

    let getDatabase () : Database =
        database

    let writeDatabase (newDatabase: Database) =
        database <- newDatabase

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
    
