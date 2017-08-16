module Provider.Logic.Quantity

open Provider.Domain.Types

// Some Complex Business Logic
// This is In its own library so the producer and the producer's fake can both use the same code for the important logic
// Also, this allows for easier unit testing of these critical functions
let UpdateQuantity (currentState: DatabaseReadResult) (action: UpdateQuantityAction) : UpdateQuantityResponse =
    match currentState, action with
    | DatabaseReadResult.Failed, _
    | DatabaseReadResult.NotFound, _ ->
        UpdateQuantityResponse.NotFound
    | DatabaseReadResult.Success state, Update newTotal ->
        { state with quantity = newTotal } |> UpdateQuantityResponse.Updated
    | DatabaseReadResult.Success state, Decrement difference ->
        let newQuantity = state.quantity - difference
        if newQuantity >= 0
        then
            { state with quantity = newQuantity } |> UpdateQuantityResponse.Updated
        else
            UpdateQuantityResponse.Failed
    | DatabaseReadResult.Success state, Increment difference ->
        { state with quantity = state.quantity + difference } |> UpdateQuantityResponse.Updated
