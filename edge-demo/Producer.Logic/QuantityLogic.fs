module Producer.Logic

open Producer.Domain.Types

// Some Complex Business Logic
// This is In its own library so the producer and the producer's fake can both use the same code for the important logic
// Also, this allows for easier unit testing of these critical functions
let UpdateQuanity (currentState: ItemState option) (action: UpdateQuantityAction) : UpdateQuantityResponse =
    match currentState, action with
    | None, _ -> UpdateQuantityResponse.NotFound
    | Some state, Update newTotal ->
        Updated { state with quantity = newTotal }
    | Some state, Decrement difference ->
        let newQuantity = state.quantity - difference
        if newQuantity >= 0
        then
            UpdateQuantityResponse.Updated { state with quantity = newQuantity }
        else
            UpdateQuantityResponse.Failed (state.sku, difference)
    | Some state, Increment difference ->
        Updated { state with quantity = state.quantity + difference }
