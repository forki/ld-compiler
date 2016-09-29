module compiler.Publish

open compiler.JsonLd
open compiler.Elastic
open compiler.Stardog
open compiler.Utils
open compiler.ContentHandle
open compiler.ConfigUtils
open FSharp.Data

let private uploadResource html =
  let url = sprintf "http://resourceapi:8082/resource/%s" html.Thing
  printf "uploading static html to %s\n" url
  Http.RequestString(url,
                     httpMethod="POST",
                     body=TextRequest html.Content,
                     headers = [ "Content-Type", "text/plain;charset=utf-8" ]) |> ignore

let publish outputDir (config:compiler.ConfigTypes.NewConfig) =

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
