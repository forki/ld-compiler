module compiler.Publish

open FSharp.Data
open compiler.ConfigTypes
open compiler.ContentHandle
open compiler.Utils
open compiler.Stardog
open compiler.JsonLd
open compiler.Elastic

let private uploadResource html =
  let url = sprintf "http://resourceapi:8082/resource/%s" html.Thing
  printf "uploading static html to %s\n" url
  Http.RequestString(url,
                     httpMethod="POST",
                     body=TextRequest html.Content,
                     headers = [ "Content-Type", "text/plain;charset=utf-8" ]) |> ignore

let publish outputDir (config:ConfigDetails) =

  let publishJsonLdResources =
    printf "Publishing jsonld resources\n"
    config.PropPaths
    |> Stardog.extractResources
    |> transformToJsonLD config.JsonLdContexts
    |> bulkUpload config.IndexName config.TypeName

  let publishStaticHtmlResources = 
    printf "Publishing static html resources\n"
    findFiles outputDir "*.html"
    |> Seq.iter ((fun f -> {Thing=f; Content=""})
                 >> readHandle
                 >> uploadResource)

  publishJsonLdResources
  publishStaticHtmlResources
