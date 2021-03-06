module compiler.IntegrationTests.Tests

open NUnit.Framework
open FsUnit
open FSharp.Data
open FSharp.Data.JsonExtensions
open System.IO
open System.Web
open System.Net
open compiler.IntegrationTests.Utils

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
          "https://nice.org.uk/ontologies/qualitystandard/bc8e0db0_5d8a_4100_98f6_774ac0eb1758":"",
          "https://nice.org.uk/ontologies/qualitystandard/1efaaa6a_c81a_4bd6_b598_c626b21c71fd":"",
          "https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de":"",
          "https://nice.org.uk/ontologies/qualitystandard/9fcb3758_a4d3_49d7_ab10_6591243caa67":"",
          "https://nice.org.uk/ontologies/qualitystandard/0886da59_2c5f_4124_9f46_6be4537a4099":"",
          "https://nice.org.uk/ontologies/qualitystandard/84efb231_0424_461e_9598_1ef5272a597a":"",
          "@id":"",
          "@type":"",
          "qualitystandard:4e7a368e_eae6_411a_8167_97127b490f99":[],
          "qualitystandard:62496684_7027_4f37_bd0e_264c9ff727fd":[],
          "qualitystandard:7ae8413a_2811_4a09_a655_eff8d276ec87":[],
          "qualitystandard:18aa6468_de94_4f9f_bd7a_0075fba942a5":[],
          "qualitystandard:28745bc0_6538_46ee_8b71_f0cf107563d9":[]
        }
      }
    ],
      "total": 3
  }
} """>


let private queryElastic indexName typeName filters =
  let url = sprintf "http://elastic:9200/%s/%s/_search" indexName typeName
  printf "url => %A" url

  let thisQuery = getQueryString filters

  let json = Http.RequestString(url,
                       body = TextRequest thisQuery,
                       headers = [ "Content-Type", "application/json;charset=utf-8" ])

  ElasticResponse.Parse(json)

let private queryElasticViaJsonParser indexName typeName filters =
  let url = sprintf "http://elastic:9200/%s/%s/_search" indexName typeName

  let thisQuery = getQueryString filters

  let json = Http.RequestString(url,
                       body = TextRequest thisQuery,
                       headers = [ "Content-Type", "application/json;charset=utf-8" ])
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

  let indexName = "kb"
  let typeName = "qualitystatement"
  let filters = [{Vocab="https://nice.org.uk/ontologies/qualitystandard/84efb231_0424_461e_9598_1ef5272a597a"; Terms = [ "qs1-st1" ]}]
  let response = queryElastic indexName typeName filters

  response.Hits.Total |> should equal 1

  let docId = (Seq.head response.Hits.Hits).Id
  docId.JsonValue.AsString() |> should equal "http://ld.nice.org.uk/things/8422158b-302e-4be2-9a19-9085fc09dfe7" 

[<Test>]
let ``When publishing a discoverable statement it should apply structured data annotations that exist in metadata`` () =

  let indexName = "kb"
  let typeName = "qualitystatement"
  let filters = [{Vocab="https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de"; Terms = [ "1" ]}]
  let response = queryElastic indexName typeName filters

  response.Hits.Total |> should equal 2

  let doc = (Seq.head response.Hits.Hits).Source

  let firstIssued = doc.HttpsNiceOrgUkOntologiesQualitystandard0886da592c5f41249f466be4537a4099
  firstIssued.JsonValue.AsString() |> should equal "2010-06-01"


[<Test>]
let ``When publishing a discoverable statement it should apply annotations that exist in metadata`` () =

  let indexName = "kb"
  let typeName = "qualitystatement"
  let filters = [{Vocab="https://nice.org.uk/ontologies/qualitystandard/84efb231_0424_461e_9598_1ef5272a597a"; Terms = [ "qs1-st1" ]}]
  let response = queryElasticViaJsonParser indexName typeName filters
  
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

  checkPropertyExistsAndIsValid "qualitystandard:4e7a368e_eae6_411a_8167_97127b490f99"
  checkPropertyExistsAndIsValid "qualitystandard:28745bc0_6538_46ee_8b71_f0cf107563d9"
  checkPropertyExistsAndIsValid "qualitystandard:7ae8413a_2811_4a09_a655_eff8d276ec87"
  checkPropertyExistsAndIsValid "qualitystandard:62496684_7027_4f37_bd0e_264c9ff727fd"
  checkPropertyExistsAndIsValid "qualitystandard:18aa6468_de94_4f9f_bd7a_0075fba942a5"


[<Test>]
let ``When publishing a discoverable statement it should apply supertype and subtype inferred annotations`` () =

  let indexName = "kb"
  let typeName = "qualitystatement"
  let filters = [{Vocab="https://nice.org.uk/ontologies/qualitystandard/84efb231_0424_461e_9598_1ef5272a597a"; Terms = [ "qs1-st1" ]}]
  let response = queryElastic indexName typeName filters

  response.Hits.Total |> should equal 1

  let doc = (Seq.head response.Hits.Hits).Source
  let agegroups = 
    doc.Qualitystandard4e7a368eEae6411a816797127b490f99 
    |> Array.map (fun s -> s.JsonValue.ToString() ) 
    |> Set.ofArray

  agegroups |> should equal 
            ( ["\"https://nice.org.uk/ontologies/agegroup/d3326f46_c734_4ab7_9e41_923256bd7d0b\""
               "\"https://nice.org.uk/ontologies/agegroup/c4347520_adf4_4ddb_9926_8f6c3132525e\""
               "\"https://nice.org.uk/ontologies/agegroup/c7935d78_d1ad_47f3_98a6_f0af04956b97\""
               "\"https://nice.org.uk/ontologies/agegroup/7cd6067c_4af1_411e_ba3c_39abac7633c8\""
               "\"https://nice.org.uk/ontologies/agegroup/aa4da4d7_b934_4d03_b556_f7b97381953f\""
               "\"https://nice.org.uk/ontologies/agegroup/011cdd3d_2911_4676_93b4_5af484c359c0\""
               ] |> Set.ofList )
       

[<Test>]
let ``When publishing a discoverable statement it should generate static html and post to resource api`` () =

  let response = Http.Request("http://resourceapi:8082/resource/8422158b-302e-4be2-9a19-9085fc09dfe7",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  response.StatusCode |> should equal 200

[<Test>]
let ``When publishing a discoverable statement it should generate static html with the correct title in the metadata table`` () =

  let response = Http.Request("http://resourceapi:8082/resource/8422158b-302e-4be2-9a19-9085fc09dfe7",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  match response.Body with
  | Text text ->
      text
      |> parseHtml
      |> CQ.select "#metadata tr:first-child td:first-child"
      |> CQ.text
      |> cleanString 
      |> should equal "First issued"
  | Binary bytes -> bytes |> should equal 0

[<Test>]
let ``When I post a markdown file to the convert end point it should generate html via pandoc`` () =

  let markdown = """### Abstract"""

  let html = Http.RequestString("http://compiler:8081/convert",
                                  headers = [ "Content-Type", "text/plain;charset=utf-8" ],
                                  body = FormValues ["markdown", markdown])

  let expectedHtml = """<h3 id="abstract">Abstract</h3>""" + System.Environment.NewLine

  html |> should equal expectedHtml

[<Test>]
let ``When publishing an undiscoverable statement it should generate static html with the content`` () =

  let response = Http.Request("http://resourceapi:8082/resource/54c3178f-f004-4caf-b1a8-582133bea26d",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  match response.Body with
  | Text text ->
      text
      |> parseHtml
      |> CQ.select "#this-is-the-undiscoverable-title"
      |> CQ.text
      |> cleanString 
      |> should equal "This is the undiscoverable title"
  | Binary bytes -> bytes |> should equal 0

[<Test>]
let ``When publishing a statement flagged with suppress content it should generate html without the content`` () =

  let response = Http.Request("http://resourceapi:8082/resource/2a1937dc-9249-4888-929a-4abcbb76a1ec",
                          headers = [ "Content-Type", "text/plain;charset=utf-8" ])

  match response.Body with
  | Text text ->
      text
      |> parseHtml
      |> CQ.select "h1"
      |> CQ.text
      |> cleanString 
      |> should equal "This quality statement is no longer available"
  | Binary bytes -> bytes |> should equal 0

  match response.Body with
  | Text text ->
      text
      |> parseHtml
      |> CQ.select "#withdrawn-heading"
      |> CQ.text
      |> cleanString 
      |> should equal ""
  | Binary bytes -> bytes |> should equal 0

[<Test>]
let ``When querying elastic for 'Skin conditions' (where several statements are returned) those statements are returned in the correct order`` () =
  let indexName = "kb"
  let typeName = "qualitystatement"
  // Quality Statement applies to condition or disease = Skin conditions
  let filters = [{Vocab="qualitystandard:28745bc0_6538_46ee_8b71_f0cf107563d9"; Terms = [ "https://nice.org.uk/ontologies/conditionordisease/acb63872_2066_431b_b70a_6c718006f572" ]}]
  let response = queryElastic indexName typeName filters

  response.Hits.Total |> should equal 3
  // Positional Id from elastic response
  let statementPositionalIds = response.Hits.Hits 
                               |> Seq.map (fun x -> x.Source.HttpsNiceOrgUkOntologiesQualitystandard84efb2310424461e95981ef5272a597a.JsonValue.AsString())
                               |> Seq.toList

  statementPositionalIds |> should equal ["qs21-st4";"qs21-st21";"qs4-st4"] 