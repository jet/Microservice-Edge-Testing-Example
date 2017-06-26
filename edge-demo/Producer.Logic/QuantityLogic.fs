module Producer.Logic.Quantity

open Producer.Domain.Types

// Some Complex Business Logic
// This is In its own library so the producer and the producer's fake can both use the same code for the important logic
// Also, this allows for easier unit testing of these critical functions
let UpdateQuantity (currentState: ItemState option) (action: UpdateQuantityAction) : UpdateQuantityResponse =
    match currentState, action with
    | None, _ -> UpdateQuantityResponse.NotFound
    | Some state, Update newTotal ->
        { state with quantity = newTotal } |> UpdateQuantityResponse.Updated
    | Some state, Decrement difference ->
        let newQuantity = state.quantity - difference
        if newQuantity >= 0
        then
            { state with quantity = newQuantity } |> UpdateQuantityResponse.Updated
        else
            state |> UpdateQuantityResponse.Failed
    | Some state, Increment difference ->
        { state with quantity = state.quantity + difference } |> UpdateQuantityResponse.Updated
