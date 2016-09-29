module compiler.Preamble

open compiler.Stardog
open compiler.ContentHandle
open compiler.Utils
open compiler.ConfigUtils
open FSharp.Data
open System.IO

let downloadSchema (config:ConfigTypes.NewConfig) outputDir =
  let download (schema:string) =
    {Thing = sprintf "%s/%s" outputDir (schema.Remove(0,schema.LastIndexOf('/')+1))
     Content = Http.RequestString(schema)}
  
//  let schemas = config |> getSchemaTtls
  
  List.iter (download >> writeFile) config.Ttls

let prepare outputDir dbName dbUser dbPass = 
  tryClean outputDir
  Stardog.deleteDb dbName dbUser dbPass
  Stardog.createDb dbName
