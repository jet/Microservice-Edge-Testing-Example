module Provider.Domain.Constants

let [<Literal>] itemRoute = "/api/item"
let [<Literal>] updateQuantityRoute = "/api/update-quantity"

let [<Literal>] kafkaHost = "localhost:9092"

let [<Literal>] stateUpdateTopic = "stateupdate-topic"
let [<Literal>] priceUpdateTopic = "priceupdate-topic"