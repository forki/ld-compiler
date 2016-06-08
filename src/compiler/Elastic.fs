module compiler.Elastic

open FSharp.Data
open System.IO
open compiler.ContentHandle

type IdSchema = JsonProvider<""" {"_id":"" }""">

let buildBulkData indexName typeName jsonldResources =
  let buildCreateCommand acc (id,json) = 
    printf "building bulk data for: %A \n" id
    let cmd = sprintf "{ \"create\" : { \"_id\" : \"%s\", \"_type\" : \"%s\",\"_index\" : \"%s\" }}\n%s\n " id typeName indexName json
    acc + cmd

  jsonldResources
  |> Seq.fold buildCreateCommand ""

let private deleteIndex esUrl = 
  try
    Http.Request(esUrl, httpMethod="DELETE") |> ignore
  with ex -> printf "Index not created yet, skipping delete\n" 

let private postMappings esUrl = 
  Http.Request(esUrl, httpMethod="POST", body = TextRequest """
{
  "settings" : {
    "index" : {
      "number_of_replicas" : "1",
      "number_of_shards" : "5"
    }
  },
  "mappings" : {
      "qualitystatement" : {
        "properties" : {
          "@id" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "@context" : {
            "type" : "object",
            "index" : "no",
            "store" : "no"
          },
          "@type" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "http://ld.nice.org.uk/ns/qualitystandard#title" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "http://ld.nice.org.uk/ns/qualitystandard#abstract" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "http://ld.nice.org.uk/ns/qualitystandard#qsidentifier" : {
            "type" : "integer",
            "index" : "not_analyzed"
          },
          "http://ld.nice.org.uk/ns/qualitystandard#stidentifier" : {
            "type" : "integer",
            "index" : "not_analyzed"
          },
          "qualitystandard:serviceArea" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:age" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:setting" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:condition" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:lifestyleCondition" : {
            "type" : "string",
            "index" : "not_analyzed"
          }
        }
      }
    }
  }
}'
""") |> ignore

let private refreshIndex esUrl = Http.Request(esUrl + "/_refresh", httpMethod="POST") |> ignore
let private uploadBulkData esUrl typeName bulkData = 
  let url = sprintf "%s/%s/_bulk" esUrl typeName
  Http.Request(url,
               httpMethod="POST",
               body=TextRequest bulkData,
               headers = [ "Content-Type", "application/json;charset=utf-8" ]) |> ignore

let bulkUpload indexName typeName jsonldResources =
  let esUrl = sprintf "http://elastic:9200/%s" indexName

  printf "building bulk data for upload to elastic"
  let bulkData = buildBulkData indexName typeName jsonldResources

  deleteIndex esUrl
  postMappings esUrl
  uploadBulkData esUrl typeName bulkData
  refreshIndex esUrl
