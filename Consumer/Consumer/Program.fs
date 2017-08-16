module Consumer.Main

open System
open Consumer.Service
open Consumer.Domain.Types
open ProviderClient.Client
open ProviderFake.Client
open Provider.Domain.Types
    
    
// The actual function of the consumer isn't relevant to this demo,
// only how it interacts with the producer, which is why there is not much real behavior
[<EntryPoint>]
let main argv =
    printfn "%A" argv
    let client = (new ProviderClientEdge(new Uri("http://localhost:8080"))) :> IProviderApi

    let service = (new ConsumerService (client)) :> IConsumerApi

    (client.StateChange ()).Add (fun newState -> printfn "A sku just got updated to look like this %A" newState)

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

    { SetPriceRequest.sku = "a" ; price = 98m }
    |> client.SetPrice
    |> printfn "%A"

    Console.ReadKey() |> ignore
    0
