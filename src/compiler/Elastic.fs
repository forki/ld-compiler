module publish.Elastic

open FSharp.Data
open System.IO
open publish.File

let buildBulkData indexName typeName jsonldResources =
  let buildCreateCommand acc jsonldResource = 
    let cmd = sprintf "{ \"create\" : { \"_id\" : \"id1\", \"_type\" : \"%s\",\"_index\" : \"%s\" }}\n%s\n " typeName indexName jsonldResource.Content
    acc + cmd

  jsonldResources
  |> Seq.fold buildCreateCommand ""

let bulkUpload indexName typeName jsonldResources =
  let esUrl = sprintf "http://elastic:9200/%s" indexName

  Http.Request(esUrl, httpMethod="DELETE") |> ignore
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
          "prov:specializationOf" : {
            "type" : "string",
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

  let bulkData = buildBulkData indexName typeName jsonldResources
  Http.Request(esUrl + "/qualitystatement/_bulk", httpMethod="POST", body=TextRequest bulkData ) |> ignore
