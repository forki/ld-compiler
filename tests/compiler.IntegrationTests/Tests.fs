module publish.IntegrationTests.Tests

open NUnit.Framework
open Swensen.Unquote
open FSharp.Data
open FSharp.Data.JsonExtensions
open FSharp.Data.HttpRequestHeaders
open System.Diagnostics
open System.IO

let execSynchronousProcess cmd args =
  let procInfo = new ProcessStartInfo(cmd, Arguments=args, RedirectStandardOutput=true, RedirectStandardError=true, UseShellExecute=false)
  let proc = new Process(StartInfo=procInfo)
  proc.Start() |> ignore
  let timeout = 10000

  proc.WaitForExit(timeout) |> ignore
  let stdErr = proc.StandardError.ReadToEnd()
  let stdOut = proc.StandardOutput.ReadToEnd()
  printf "stdErr %s\n" stdErr
  printf "stdOut %s\n" stdOut

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
          "qualitystandard:setting":"",
          "qualitystandard:age":[""]
        }
      }
    ],
      "total": 3
  }
} """>

let private runProgramWith inputFile outputDir =
  let args = sprintf "/compiler/publish.exe %s %s" inputFile outputDir
  execSynchronousProcess "mono" args

let private queryElastic indexName typeName =
  let url = sprintf "http://elastic:9200/%s/%s/_search" indexName typeName
  let json = Http.RequestString(url, httpMethod="GET")
  ElasticResponse.Parse(json)

let private inputDir = "/samples"
let private outputDir = "/artifacts"

let private createStatement qsId stId content =
  Directory.CreateDirectory (inputDir) |> ignore
  Directory.CreateDirectory (inputDir + "/" + qsId) |> ignore
  Directory.CreateDirectory (inputDir + "/" + qsId + "/" + stId) |> ignore
  Directory.CreateDirectory (outputDir) |> ignore
  File.WriteAllText(inputDir + "/" + qsId + "/" + stId + "/Statement.md", content)

[<TearDown>]
let Teardown () =
  Directory.Delete(inputDir, true)
  Directory.Delete(outputDir, true)
  let res = Http.RequestString("http://elastic:9200/kb", httpMethod="DELETE")
  try
    Http.RequestString ( "http://stardog:5820/admin/databases/nice", httpMethod = "DELETE", headers = [ BasicAuth "admin" "admin"] ) |> ignore
  with _ -> ()

[<Test>]
let ``When publishing a statement it should have added a statement to elastic search index`` () =

  let markdown = """
```
Vocab:
    - "Term"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract.

This is some content.
  """
  createStatement "qs1" "st1" markdown

  runProgramWith inputDir outputDir

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

  let markdown = """
```
Setting:
    - "Hospital"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract.

This is some content.
  """
  createStatement "qs1" "st1" markdown

  runProgramWith inputDir outputDir

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  test <@ response.Hits.Total = 1 @>

  let doc = (Seq.head response.Hits.Hits).Source

  test <@ doc.QualitystandardSetting.JsonValue.ToString() = "\"http://ld.nice.org.uk/ns/qualitystandard/setting#Hospital\"" @>


[<Test>]
let ``When publishing a statement it should apply supertype and subtype inferred annotations`` () =
  let markdown = """
```
Age Group:
    - "Adults"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract.

This is some content.
  """
  createStatement "qs1" "st1" markdown

  runProgramWith inputDir outputDir

  let indexName = "kb"
  let typeName = "qualitystatement"
  let response = queryElastic indexName typeName

  test <@ response.Hits.Total = 1 @>

  let doc = (Seq.head response.Hits.Hits).Source
  let settings = doc.QualitystandardAge |> Array.map (fun s -> s.JsonValue.ToString() ) |> Set.ofArray
  test <@ settings = 
            ( ["\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 18-24 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 25-64 years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#Adults 65+ years\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#All age groups\""
               "\"http://ld.nice.org.uk/ns/qualitystandard/agegroup#AgeGroup\""
               "\"http://ld.nice.org.uk/ns/qualitystandard#PopulationSpecifier\""] |> Set.ofList )
       @>
