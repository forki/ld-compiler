module compiler.Publish

open compiler.JsonLd
open compiler.Elastic
open compiler.Stardog
open compiler.Utils
open compiler.ContentHandle
open FSharp.Data

let private uploadResource html =
  let url = sprintf "http://resourceapi:8082/resource/%s" html.Path
  printf "uploading static html to %s\n" url
  Http.RequestString(url,
                     httpMethod="POST",
                     body=TextRequest html.Content,
                     headers = [ "Content-Type", "text/plain;charset=utf-8" ]) |> ignore

let convertPathToResourceUri outputDir handle = 
  {handle with Path=handle.Path.Replace(outputDir+"/","").Replace(".html", "") .Replace("_","/")}

let publish propertyPaths contexts outputDir indexName typeName =

  let publishJsonLdResources =
    printf "Publishing jsonld resources\n"
    Stardog.extractResources propertyPaths
    |> transformToJsonLD contexts
    |> bulkUpload indexName typeName

  let publishStaticHtmlResources = 
    printf "Publishing static html resources\n"
    findFiles outputDir "*.html"
    |> Seq.iter ((fun f -> {Path=f;Content=""})
                 >> readHandle
                 >> convertPathToResourceUri outputDir
                 >> uploadResource)

  publishJsonLdResources
  publishStaticHtmlResources
