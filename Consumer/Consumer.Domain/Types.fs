namespace Consumer.Domain.Types

open Provider.Domain.Types

type IConsumerApi =
    /// Increments the stock due to being overstocked
    // TODO will increment price as well
    // TO DECIDE - do we let the producer type filter through this system?
    // Probably not as I wouldn't want upstream types in my prod system as the adapers should be upstream agnostic
    abstract member Overstocked: Sku -> int -> Async<UpdateQuantityResponse>

    abstract member NudgePriceDown: Sku -> Async<unit>
    
    abstract member SalesToRun: unit -> int