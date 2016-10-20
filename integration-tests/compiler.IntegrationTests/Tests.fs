module compiler.IntegrationTests.Tests

open NUnit.Framework
open FsUnit
open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open System.Web
open System.Net

let runCompileAndWaitTillFinished () =
  let myGitRepoUrl = "https://github.com/nhsevidence/ld-dummy-content"
  let res = Http.RequestString("http://compiler:8081/compile",
                               query=["repoUrl", myGitRepoUrl],
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
          "@id":"",
          "@type":"",
          "qualitystandard:appliesToAgeGroup":[],
          "qualitystandard:appliesToSetting":[],
          "qualitystandard:appliesToServiceArea":[],
          "qualitystandard:appliesToFactorsAffectingHealthOrWellbeing":[],
          "qualitystandard:appliesToConditionOrDisease":[]
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

let private queryElasticViaJsonParser indexName typeName =
  let url = sprintf "http://elastic:9200/%s/%s/_search" indexName typeName
  let json = Http.RequestString(url, httpMethod="GET")

  JsonValue.Parse(json)

type Result<'TSuccess,'TFailure> = 
| Success of 'TSuccess
| Failed of string

let bind switchFunction twoTrackInput = 
    match twoTrackInput with
    | Success s -> switchFunction s
    | Failed f -> Failed f

let (>>=) twoTrackInput switchFunction = 
    bind switchFunction twoTrackInput 

// convert a normal function into a switch
let switch f x = 
    f x |> Success


[<OneTimeSetUp>]
let ``run before all tests``() =
  printf "Running pre-test setup"
  runCompileAndWaitTillFinished ()

[<OneTimeTearDown>]
let ``run after all tests``() =
  printf "Running post-test teardown"


/// Tests constructed on the presumption that there are 2 statements in the content being deployed, which are similar in annotations, but only one is discoverable
[<Test>]
let ``When publishing a discoverable statement it should have added a statement to elastic search index`` () =

  (* runCompileAndWaitTillFinished ()*)

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 2

//  let doc = (Seq.head response.Hits.Hits).Source
//  doc.Id.JsonValue.AsString() |> should equal "http://ld.nice.org.uk/resource/8422158b-302e-4be2-9a19-9085fc09dfe7" 
  let docId = (Seq.head response.Hits.Hits).Id
  docId.JsonValue.AsString() |> should equal "http://ld.nice.org.uk/resource/8422158b-302e-4be2-9a19-9085fc09dfe7" 

[<Test>]
let ``When publishing a discoverable statement it should apply structured data annotations that exist in metadata`` () =

  (* runCompileAndWaitTillFinished ()*)

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 2

  let doc = (Seq.head response.Hits.Hits).Source

  let firstIssued = doc.HttpLdNiceOrgUkNsQualitystandardWasFirstIssuedOn
  firstIssued.JsonValue.AsString() |> should equal "2010-06-01"


[<Test>]
let ``When publishing a discoverable statement it should apply annotations that exist in metadata`` () =

  (* runCompileAndWaitTillFinished ()*)

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElasticViaJsonParser indexName typeName
  
  let getProperty (propertyName:string) (root:JsonValue) =
    let a = root.TryGetProperty(propertyName)

    match a with
    | Some (a) -> Success a
    | None -> sprintf "Couldn't find the property: %s" propertyName |> Failed

  let checkValueIsAnArrayOrString response =
    match response with
    | JsonValue.Array a -> a |> Array.head |> Success 
    | JsonValue.String a -> response |> Success
    | _ -> Failed "The type you are accesing is not an array or a string"
    
  let checkValueIsAString (response:JsonValue) =
    match response with
    | JsonValue.String _ -> Success true
    | _ -> Failed "Array doesn't contain strings"

  let getRootProperty response =
    Success response?hits?hits.[0]?_source
     
  let checkPropertyExistsAndIsValid propertyName =
    let result =
      response
      |> (fun r -> Success r)
      >>= getRootProperty
      >>= getProperty propertyName
      >>= checkValueIsAnArrayOrString
      >>= checkValueIsAString

    match result with
    | Success a -> a |> should equal true
    | Failed x -> sprintf "Test failed with error message: %A" x |> failwith

  checkPropertyExistsAndIsValid "qualitystandard:appliesToAgeGroup"
  checkPropertyExistsAndIsValid "qualitystandard:appliesToConditionOrDisease"
  checkPropertyExistsAndIsValid "qualitystandard:appliesToServiceArea"
  checkPropertyExistsAndIsValid "qualitystandard:appliesToSetting"
  checkPropertyExistsAndIsValid "qualitystandard:appliesToFactorsAffectingHealthOrWellbeing"


[<Test>]
let ``When publishing a discoverable statement it should apply supertype and subtype inferred annotations`` () =
  (* runCompileAndWaitTillFinished ()*)

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  response.Hits.Total |> should equal 2

  let doc = (Seq.head response.Hits.Hits).Source
  let agegroups = doc.QualitystandardAppliesToAgeGroup |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray

  agegroups |> should equal 
            ( ["\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/d3326f46_c734_4ab7_9e41_923256bd7d0b\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/c4347520_adf4_4ddb_9926_8f6c3132525e\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/c7935d78_d1ad_47f3_98a6_f0af04956b97\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/7cd6067c_4af1_411e_ba3c_39abac7633c8\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/aa4da4d7_b934_4d03_b556_f7b97381953f\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup/011cdd3d_2911_4676_93b4_5af484c359c0\""
               ] |> Set.ofList )
       

[<Test>]
let ``When publishing a discoverable statement it should generate static html and post to resource api`` () =

  (* runCompileAndWaitTillFinished ()*)

  let response = Http.Request("http://resourceapi:8082/resource/8422158b-302e-4be2-9a19-9085fc09dfe7",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  response.StatusCode |> should equal 200

[<Test>]
let ``When publishing an undiscoverable statement it should generate static html and post to resource api`` () =

  (* runCompileAndWaitTillFinished ()*)

  let response = Http.Request("http://resourceapi:8082/resource/54c3178f-f004-4caf-b1a8-582133bea26d",
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

