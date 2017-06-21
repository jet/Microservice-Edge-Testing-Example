open System
open ProducerClient.Client
open ProducerFake.Client
open Producer.Domain.Types

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let client = (new ProducerClientEdge ()) :> IProducerApi
    //let client = (new ProducerClientEdgeFake ()) :> IProducerApi

    async {
        return! client.GetItem { GetItemRequest.sku = "a" }
    } |> Async.RunSynchronously
    |> printfn "%A"
    async {
        return! client.UpdateQuantity
            {
                UpdateQuantityRequest.sku = "a"
                action = Increment 2
            }
    }
    |> Async.RunSynchronously
    |> printfn "%A"


    async {
        return! client.GetItem { GetItemRequest.sku = "a" }
    } |> Async.RunSynchronously
    |> printfn "%A"
    Console.ReadKey() |> ignore
    0
