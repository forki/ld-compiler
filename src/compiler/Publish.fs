module compiler.Publish

open Serilog
open NICE.Logging
open FSharp.Data
open compiler.ConfigTypes
open compiler.ContentHandle
open compiler.Utils
open compiler.Stardog
open compiler.JsonLd
open compiler.Elastic

let private uploadResource html =
  let url = sprintf "http://resourceapi:8082/resource/%s" html.Thing
  Log.Information (sprintf "uploading static html to %s" url)
  Http.RequestString(url,
                     httpMethod="POST",
                     body=TextRequest html.Content,
                     headers = [ "Content-Type", "text/plain;charset=utf-8" ]) |> ignore

let publish outputDir (config:ConfigDetails) =

  let publishJsonLdResources =
    Log.Information "Publishing jsonld resources"

    config
    |> Stardog.extractResources
    |> transformToJsonLD config.JsonLdContexts
    |> bulkUpload config.IndexName config.TypeName

  let publishStaticHtmlResources = 
    Log.Information "Publishing static html resources"
    findFiles outputDir "*.html"
    |> Seq.iter ((fun f -> {Thing=f; Content=""})
                 >> readHandle
                 >> uploadResource)

  publishJsonLdResources
  publishStaticHtmlResources
