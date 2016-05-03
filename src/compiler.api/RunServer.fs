module compiler.api.RunServer
open Suave
open compiler.api.core.App
open compiler.Compile

[<EntryPoint>]
let main _ =
  let defaultConfig =
    { defaultConfig with
                    bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" 8083 ]}
  startWebServer defaultConfig ( createApp compile )
  0
