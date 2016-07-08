module compiler.Test.JsonLdTests

open NUnit.Framework
open FsUnit
open compiler.JsonLd
open FSharp.Data
open FSharp.RDF
open Assertion
open resource
open rdf

type JsonLdSchema = JsonProvider<"""
{
  "http://ld.nice.org.uk/ns/qualitystandard#title":"",
  "_id":"",
  "_type":""
}""">

let private qsContexts = ["http://ld.nice.org.uk/ns/qualitystandard.jsonld"]
let private createResource uri =
  resource !! uri 
    [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("Not used"^^xsd.string)]


[<Test>]
let ``Should add a _type field``() =
  let simpleResource = resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here"  []

  let contexts = []
  let _,json = transformToJsonLD contexts [[simpleResource]] |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  jsonld.Type.JsonValue.AsString() |> should equal "qualitystatement"

