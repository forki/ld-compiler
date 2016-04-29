module compiler.api.RunServer
open Suave
open compiler.api.core.App
open compiler.Compile

[<EntryPoint>]
let main _ =
  startWebServer defaultConfig ( createApp compile )
  0
