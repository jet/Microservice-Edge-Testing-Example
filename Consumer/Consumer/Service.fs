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
