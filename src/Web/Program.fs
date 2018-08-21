module WebServer

open System
open System.Threading
open Suave
open Clinics.Dto

[<EntryPoint>]
let main argv =
  let conf = { 
    defaultConfig with 
      bindings =
        [ HttpBinding.createSimple HTTP "0.0.0.0" 5000 ]
  }
  printfn "Make requests now"
  let name = { FirstName = "Joe"; Initial = "G"; LastName = "Schmoe" }
  startWebServer conf (Successful.OK (sprintf "Hello World! %A" name))
  printfn "Bye!"
  0
