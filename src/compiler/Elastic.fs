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


let private postDynamicMappings esUrl =
  Http.Request(esUrl, httpMethod="PUT", body = TextRequest """
{
  "mappings": {
    "qualitystatement": {
      "dynamic_templates": [
        { "notanalyzed": {
            "match": "*3ff270e4_655a_4884_b186_e033f58759de",
            "match_mapping_type": "string",
            "mapping": {
               "type": "integer"
            }
          }
        },
        { "notanalyzed": {
            "match": "*9fcb3758_a4d3_49d7_ab10_6591243caa67",
            "match_mapping_type": "string",
            "mapping": {
              "type": "integer"
            }
          }
        },
        { "notanalyzed": {
            "match": "*0886da59_2c5f_4124_9f46_6be4537a4099",
            "match_mapping_type": "string",
            "mapping": {
              "type": "date",
              "format": "dateOptionalTime"
            }
          }
        },
        { "notanalyzed": {
            "match": "*",
            "match_mapping_type": "string",
            "mapping": {
              "type": "string",
              "index": "not_analyzed"
            }
          }
        }
      ]
    }
  }
}
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
  postDynamicMappings esUrl
  uploadBulkData esUrl typeName bulkData
  refreshIndex esUrl

  
