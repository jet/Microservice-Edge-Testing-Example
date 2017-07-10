module Consumer.Main

open System
open Consumer.Service
open Consumer.Domain.Types
open ProducerClient.Client
open ProducerFake.Client
open Producer.Domain.Types
    
    
[<EntryPoint>]
let main argv =
    printfn "%A" argv
    let client = (new ProducerClientEdge(new Uri("http://localhost:8080"))) :> IProducerApi
    //let client = (new ProducerClientEdgeFake ()) :> IProducerApi

    let service = (new ConsumerService (client)) :> IConsumerApi

    async {
        return! client.GetItem { GetItemRequest.sku = "a" }
    } |> Async.RunSynchronously
    |> printfn "%A"

    service.Overstocked "a" 2
    |> Async.RunSynchronously
    |> printfn "%A"


    async {
        return! client.GetItem { GetItemRequest.sku = "a" }
    } |> Async.RunSynchronously
    |> printfn "%A"

    { SetPriceRequest.sku = "a" ; price = 99.0m }
    |> client.SetPrice
    |> printfn "%A"

    Console.ReadKey() |> ignore
    0
