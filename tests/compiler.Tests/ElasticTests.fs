module publish.Tests.ElasticTests

open NUnit.Framework
open Swensen.Unquote
open FSharp.Data
open publish.File
open publish.Elastic

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
  let jsonldResources = [{Path = "somefile.jsonld"; Content = """{"field":"value"}"""}]
  let indexName = "index_name"
  let typeName = "type_name"
  let bulkData = buildBulkData indexName typeName jsonldResources
  let createLine = bulkData.Split('\n').[0]
  let createJson = CreateSchema.Parse(createLine)

  test <@ createJson.Create.Id.JsonValue.AsString() = "id1" @>
  test <@ createJson.Create.Type.JsonValue.AsString() = typeName @>
  test <@ createJson.Create.Index.JsonValue.AsString() = indexName @>

[<Test>]
let ``Should build data command on line after create command for jsonld resource`` () =
  let jsonldResourceContent = "jsonld content"
  let jsonldResources = [{Path = "somefile.jsonld"; Content = jsonldResourceContent}]
  let bulkData = buildBulkData "notused" "notused" jsonldResources
  let json = bulkData.Split('\n').[1]

  test <@ json = jsonldResourceContent @>

[<Test>]
let ``Should build a create line and a data line for each jsonld resource`` () =
  let jsonldResources = [{Path = ""; Content = ""}
                         {Path = ""; Content = ""}]
  let bulkData = buildBulkData "notused" "notused" jsonldResources

  test <@ bulkData.Split('\n').Length = 5 @>
