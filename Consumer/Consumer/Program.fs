open System
open ProducerClient.Client
open Producer.Domain.Types

[<EntryPoint>]
let main argv = 
    printfn "%A" argv
    let client = (new ProducerClientEdge ()) :> IProducerApi
    let response =
        async {
            return! client.GetItem { GetItemRequest.sku = "skufromclient" }
        } |> Async.RunSynchronously
    printfn "%A" response
    async {
        return! client.UpdateQuantity
            {
                UpdateQuantityRequest.sku = "updatethissku"
                action = Decrement 2
            }
    }
    |> Async.RunSynchronously
    |> printfn "%A"
    Console.ReadKey() |> ignore
    0
