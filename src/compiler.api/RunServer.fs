module compiler.api.RunServer
open Suave
open compiler.api.App
open compiler.Compile

[<EntryPoint>]
let main argv =
  startWebServer defaultConfig ( createApp compile )
  0
