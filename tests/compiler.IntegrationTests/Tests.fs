module publish.IntegrationTests.Tests

open NUnit.Framework
open Swensen.Unquote
open FSharp.Data
open System.Diagnostics
open System.IO

let execSynchronousProcess cmd args =
  let procInfo = new ProcessStartInfo(cmd, Arguments=args, RedirectStandardOutput=true, RedirectStandardError=true, UseShellExecute=false)
  let proc = new Process(StartInfo=procInfo)
  proc.Start() |> ignore
  let timeout = 100000

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
          "_type":""
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

  let inputDir = "/samples"
  let outputDir = "/artifacts"
  Directory.CreateDirectory (inputDir) |> ignore
  Directory.CreateDirectory (inputDir + "/qs1") |> ignore
  Directory.CreateDirectory (inputDir + "/qs1/st1") |> ignore
  Directory.CreateDirectory (outputDir) |> ignore
  File.WriteAllText(inputDir + "/qs1/st1/Statement.md", markdown)

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
  

