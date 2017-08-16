module Producer

open System.Net
open Newtonsoft.Json
open Suave
open Suave.Web
open Suave.Successful

[<EntryPoint>]
let main argv =
  startWebServer defaultConfig (OK "Hello, Suave!")
  0
