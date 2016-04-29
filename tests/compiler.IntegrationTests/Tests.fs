module publish.IntegrationTests.Tests

open NUnit.Framework
open Swensen.Unquote
open FSharp.Data
open FSharp.Data.JsonExtensions

let runCompileAndWaitTillFinished () =
  let res = Http.RequestString("http://localhost:8083/compile", httpMethod="POST")
  test <@ res = "Started" @>
  let mutable finished = false
  while finished = false do
    if Http.RequestString("http://localhost:8083/status") = "Not running" then finished <- true

type ElasticResponse = JsonProvider<"""
{
  "hits":{
    "hits":[
      {
        "_id":"",
        "_source":{
          "http://ld.nice.org.uk/ns/qualitystandard#title":"",
          "http://ld.nice.org.uk/ns/qualitystandard#abstract":"",
          "http://ld.nice.org.uk/ns/qualitystandard#qsidentifier":"",
          "http://ld.nice.org.uk/ns/qualitystandard#stidentifier":"",
          "_id":"",
          "_type":"",
          "qualitystandard:age":[""]
        }
      }
    ],
      "total": 3
  }
} """>

let private queryElastic indexName typeName =
  let url = sprintf "http://elastic:9200/%s/%s/_search" indexName typeName
  let json = Http.RequestString(url, httpMethod="GET")
  ElasticResponse.Parse(json)

[<TearDown>]
let Teardown () =
  try Http.RequestString("http://elastic:9200/kb", httpMethod="DELETE") |> ignore with _ -> ()

[<Test>]
let ``When publishing a statement it should have added a statement to elastic search index`` () =

  runCompileAndWaitTillFinished ()

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  test <@ response.Hits.Total = 1 @>

  let doc = (Seq.head response.Hits.Hits).Source

  test <@ doc.Id.JsonValue.AsString() = "http://ld.nice.org.uk/qualitystatement/qs1/st1" @>
  test <@ doc.HttpLdNiceOrgUkNsQualitystandardTitle.JsonValue.AsString() = "Quality Statement 1 from Quality Standard 1" @>
  test <@ doc.HttpLdNiceOrgUkNsQualitystandardAbstract.JsonValue.AsString() = "This is the abstract." @>
  test <@ doc.HttpLdNiceOrgUkNsQualitystandardQsidentifier.JsonValue.AsInteger() = 1 @>
  test <@ doc.HttpLdNiceOrgUkNsQualitystandardStidentifier.JsonValue.AsInteger() = 1 @>
  

[<Test>]
let ``When publishing a statement it should apply annotations`` () =

  runCompileAndWaitTillFinished ()

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  test <@ response.Hits.Total = 1 @>

  let doc = (Seq.head response.Hits.Hits).Source

  let agegroups = doc.QualitystandardAge |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray
  test <@ agegroups.Contains("\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults\"") @>


[<Test>]
let ``When publishing a statement it should apply supertype and subtype inferred annotations`` () =
  runCompileAndWaitTillFinished ()

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  test <@ response.Hits.Total = 1 @>

  let doc = (Seq.head response.Hits.Hits).Source
  let agegroups = doc.QualitystandardAge |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray
  test <@ agegroups = 
            ( ["\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 18-24 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 25-64 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 65+ years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#All age groups\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#AgeGroup\""
               "\"http://ld.nice.org.uk/ns/qualitystandard#PopulationSpecifier\""] |> Set.ofList )
       @>
