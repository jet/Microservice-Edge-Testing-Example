module Consumer.Service

open Consumer.Domain.Types
open Producer.Domain.Types

type ConsumerService (producerClient: IProducerApi) =

    let mutable salesToRun = 0

    interface IConsumerApi with
        member x.Overstocked sku amount = async {
            let! res =
                {
                    UpdateQuantityRequest.sku = sku
                    action = Increment amount
                }
                |> producerClient.UpdateQuantity
            salesToRun <- salesToRun + (match res with | Updated _ -> 1 | _ -> 0)
            return res
        }

        member x.NudgePriceDown sku = async {
            let! current = { GetItemRequest.sku = sku } |> producerClient.GetItem

            match current with
            | Success item ->
                if item.price < 0.5m
                then
                    salesToRun <- salesToRun + 1
                    { SetPriceRequest.price = item.price - 0.1m; sku = item.sku }
                    |> producerClient.SetPrice
                else
                    ()
            | _ -> ()
        }

        member x.SalesToRun () = salesToRun