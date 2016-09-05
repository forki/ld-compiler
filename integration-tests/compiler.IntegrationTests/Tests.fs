module compiler.IntegrationTests.Tests

open NUnit.Framework
open FsUnit
open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open System.Web
open System.Net

let runCompileAndWaitTillFinished gitRepoUrl =
  let res = Http.RequestString("http://compiler:8081/compile",
                               query=["repoUrl", gitRepoUrl],
                               httpMethod="POST")
  res |> should equal "Started"
  let mutable finished = false
  while finished = false do
    if Http.RequestString("http://compiler:8081/status") = "Not running" then finished <- true

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
          "http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn":"",
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
  printf "Deleting elastic index\n"
  try Http.RequestString("http://elastic:9200/kb", httpMethod="DELETE") |> ignore with _ -> ()
  printf "Deleting all static html resources\n"
  try Http.RequestString("http://resourceapi:8082/resource/8422158b-302e-4be2-9a19-9085fc09dfe7", httpMethod="DELETE") |> ignore with _ -> ()

[<Test>]
let ``When publishing a statement it should have added a statement to elastic search index`` () =

  runCompileAndWaitTillFinished "https://github.com/nhsevidence/ld-dummy-content"

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 1

  let doc = (Seq.head response.Hits.Hits).Source

  doc.Id.JsonValue.AsString() |> should equal "http://ld.nice.org.uk/resource/8422158b-302e-4be2-9a19-9085fc09dfe7"   

[<Test>]
let ``When publishing a statement it should apply structured data annotations that exist in metadata`` () =

  runCompileAndWaitTillFinished "https://github.com/nhsevidence/ld-dummy-content"

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 1 

  let doc = (Seq.head response.Hits.Hits).Source

  let firstIssued = doc.HttpLdNiceOrgUkNsQualitystandardWasFirstIssuedOn
  firstIssued.JsonValue.AsString() |> should equal "2010-06-01"

[<Test>]
let ``When publishing a statement it should apply annotations that exist in metadata`` () =

  runCompileAndWaitTillFinished "https://github.com/nhsevidence/ld-dummy-content"

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 1 

  let doc = (Seq.head response.Hits.Hits).Source

  let agegroups = doc.QualitystandardAge |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray
  agegroups |> should contain "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults\""


[<Test>]
let ``When publishing a statement it should apply supertype and subtype inferred annotations`` () =
  runCompileAndWaitTillFinished "https://github.com/nhsevidence/ld-dummy-content"

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 1 

  let doc = (Seq.head response.Hits.Hits).Source
  let agegroups = doc.QualitystandardAge |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray
  agegroups |> should equal 
            ( ["\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 18-24 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 25-64 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 65+ years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#All age groups\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#AgeGroup\""
               "\"http://ld.nice.org.uk/ns/qualitystandard#PopulationSpecifier\""] |> Set.ofList )
       

[<Test>]
let ``When publishing a statement it should generate static html and post to resource api`` () =

  runCompileAndWaitTillFinished "https://github.com/nhsevidence/ld-dummy-content"

  let response = Http.Request("http://resourceapi:8082/resource/8422158b-302e-4be2-9a19-9085fc09dfe7",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  response.StatusCode |> should equal 200

[<Test>]
let ``When I post a markdown file to the convert end point it should generate html via pandoc`` () =

  let markdown = """### Abstract"""

  let html = Http.RequestString("http://compiler:8081/convert",
    headers = [ "Content-Type", "text/plain;charset=utf-8" ], 
    body = FormValues ["markdown", markdown])

  let expectedHtml = """<h3 id="abstract">Abstract</h3>""" + System.Environment.NewLine

  html |> should equal expectedHtml