module compiler.api.RunServer
open Suave
open compiler.api.core.App
open compiler.Compile

let compileAsync () =
  async {
    return compile ()
  }

[<EntryPoint>]
let main _ =
  startWebServer defaultConfig ( createApp compileAsync )
  0
