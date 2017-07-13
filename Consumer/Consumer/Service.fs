module Consumer.Service

open Consumer.Domain.Types
open Producer.Domain.Types

type ConsumerService (producerClient: IProducerApi) =

    interface IConsumerApi with
        member x.Overstocked sku amount = async {
            return! producerClient.UpdateQuantity
                {
                    UpdateQuantityRequest.sku = sku
                    action = Increment amount
                }
        }

        member x.NudgePriceDown sku = async {
            let! current = { GetItemRequest.sku = sku } |> producerClient.GetItem

            match current with
            | Success item ->
                if item.price < 0.5m
                then
                    { SetPriceRequest.price = item.price - 0.1m; sku = item.sku }
                    |> producerClient.SetPrice
                else
                    ()
            | _ -> ()
        }