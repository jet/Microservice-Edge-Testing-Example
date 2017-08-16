namespace Consumer.Domain.Types

open Provider.Domain.Types

type IConsumerApi =
    /// Increments the stock due to being overstocked
    // TODO will increment price as well
    abstract member Overstocked: Sku -> int -> Async<UpdateQuantityResponse>

    abstract member NudgePriceDown: Sku -> Async<unit>
    
    abstract member SalesToRun: unit -> int