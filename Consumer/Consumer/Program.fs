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
    Console.ReadKey() |> ignore
    0
