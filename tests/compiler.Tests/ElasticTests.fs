module compiler.Tests.ElasticTests

open NUnit.Framework
open FsUnit
open FSharp.Data
open compiler.ContentHandle
open compiler.Elastic

type CreateSchema = JsonProvider<"""
{
  "create": {
    "_id":"",
    "_type":"",
    "_index":""
  }
}""">

[<Test>]
let ``Should build correct create command for single jsonld resource`` () =
  let jsonldResources = [("id_goes_here", "some json goes here")]
  let indexName = "index_name"
  let typeName = "type_name"
  let bulkData = buildBulkData indexName typeName jsonldResources
  let createLine = bulkData.Split('\n').[0]
  let createJson = CreateSchema.Parse(createLine)

  createJson.Create.Id.JsonValue.AsString() |> should equal "id_goes_here"
  createJson.Create.Type.JsonValue.AsString() |> should equal typeName
  createJson.Create.Index.JsonValue.AsString() |> should equal indexName

[<Test>]
let ``Should build data command on line after create command for jsonld resource`` () =
  let jsonldResourceContent = "some json goes here"
  let jsonldResources = [("", jsonldResourceContent)]
  let bulkData = buildBulkData "notused" "notused" jsonldResources
  let json = bulkData.Split('\n').[1]

  json |> should equal jsonldResourceContent


[<Test>]
let ``Should build a create line and a data line for each jsonld resource`` () =
  let jsonldResources = [("","some json goes here")
                         ("","some json goes here")]
  let bulkData = buildBulkData "notused" "notused" jsonldResources

  bulkData.Split('\n').Length |> should equal 5
