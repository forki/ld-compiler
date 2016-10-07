module compiler.api.RunServer
open Suave
open compiler.api.core.App
open compiler.Main
open System.Net
open System.Configuration
open Serilog
open NICE.Logging

let printAppSetting (key:string) =
  let value = ConfigurationManager.AppSettings.Get(key)
  printf "%s is %s\n" key value

[<EntryPoint>]
let main _ =
  ["NiceLogging/AmqpUri"
   "NiceLogging/Environment"
   "NiceLogging/Application"] |> List.iter printAppSetting

  let logConfig = LoggerConfiguration().WriteTo.Nice().WriteTo.Console()
  Log.Logger <- logConfig.MinimumLevel.Debug().CreateLogger()

//  Log.Debug ("Debug message")
//  Log.Information ("Information message")
//  Log.Warning ("Warning message")
//  Log.Error ("Error message")
//  Log.Fatal ("Fatal message")

  let defaultConfig =
    { defaultConfig with
                    bindings = [ HttpBinding.mkSimple HTTP "0.0.0.0" 8081 ]}
  startWebServer defaultConfig ( createApp compileAndPublish )
  0
