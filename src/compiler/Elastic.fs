module compiler.Elastic

open Serilog
open NICE.Logging
open FSharp.Data
open System.IO
open compiler.ContentHandle

type IdSchema = JsonProvider<""" {"_id":"" }""">

let buildBulkData indexName typeName jsonldResources =
  let buildCreateCommand acc (id,json) = 
    Log.Information (sprintf "building bulk data for: %A" id)
    let cmd = sprintf "{ \"create\" : { \"_id\" : \"%s\", \"_type\" : \"%s\",\"_index\" : \"%s\" }}\n%s\n " id typeName indexName json
    acc + cmd

  jsonldResources
  |> Seq.fold buildCreateCommand ""

let private deleteIndex esUrl = 
  try
    Http.Request(esUrl, httpMethod="DELETE") |> ignore
  with ex -> Log.Information "Index not created yet, skipping delete" 

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
          "https://nice.org.uk/ontologies/qualitystandard/bc8e0db0_5d8a_4100_98f6_774ac0eb1758" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "https://nice.org.uk/ontologies/qualitystandard/1efaaa6a_c81a_4bd6_b598_c626b21c71fd" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de" : {
            "type" : "integer",
            "index" : "not_analyzed"
          },
          "https://nice.org.uk/ontologies/qualitystandard/9fcb3758_a4d3_49d7_ab10_6591243caa67" : {
            "type" : "integer",
            "index" : "not_analyzed"
          },
          "qualitystandard:7ae8413a_2811_4a09_a655_eff8d276ec87" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:4e7a368e_eae6_411a_8167_97127b490f99" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:62496684_7027_4f37_bd0e_264c9ff727fd" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:28745bc0_6538_46ee_8b71_f0cf107563d9" : {
            "type" : "string",
            "index" : "not_analyzed"
          },
          "qualitystandard:18aa6468_de94_4f9f_bd7a_0075fba942a5" : {
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

  Log.Information "building bulk data for upload to elastic"
  let bulkData = buildBulkData indexName typeName jsonldResources
  printf "bulk data: %A" bulkData

  deleteIndex esUrl
  postMappings esUrl
  uploadBulkData esUrl typeName bulkData
  refreshIndex esUrl

  
