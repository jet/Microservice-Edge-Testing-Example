module Helpers

open System.Text
open System.IO
open System.Net
open Newtonsoft.Json

let httpPost (url:string) payload =
    let req = HttpWebRequest.Create(url) :?> HttpWebRequest 
    req.ProtocolVersion <- HttpVersion.Version10
    req.Method <- "POST"

    let data = JsonConvert.SerializeObject payload
    let postBytes = Encoding.ASCII.GetBytes(data)
    req.ContentType <- "application/json";
    req.ContentLength <- int64 postBytes.Length
    // Write data to the request
    let reqStream = req.GetRequestStream() 
    reqStream.Write(postBytes, 0, postBytes.Length);
    reqStream.Close()

    // Obtain response and download the resulting page 
    // (The sample contains the first & last name from POST data)
    let resp = req.GetResponse() 
    let stream = resp.GetResponseStream() 
    let reader = new StreamReader(stream) 
    reader.ReadToEnd()
    
        

