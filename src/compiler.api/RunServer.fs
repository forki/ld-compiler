module compiler.api.RunServer
open Suave
open compiler.api.core.App
open compiler.Main

[<EntryPoint>]
let main _ =
  let defaultConfig =
    { defaultConfig with
                    bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" 8081 ]}
  startWebServer defaultConfig ( createApp compileAndPublish )
  0
