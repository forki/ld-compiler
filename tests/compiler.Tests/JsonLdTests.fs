module publish.Test.JsonLdTests

open NUnit.Framework
open Swensen.Unquote
open publish.JsonLd
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


[<Test>]
let ``Should add a _id field with resource uri``() =
  let uri = "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
  let simpleResource =
      resource !! uri 
          [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]
  let contexts = ["http://ld.nice.org.uk/ns/qualitystandard.jsonld"]

  let json = transformToJsonLD [[simpleResource]] contexts |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.Id.JsonValue.AsString() = uri @>

[<Test>]
let ``Should add a _type field``() =
  let simpleResource = resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here"  []

  let contexts = []
  let json = transformToJsonLD [[simpleResource]] contexts |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.Type.JsonValue.AsString() = "qualitystatement" @>

[<Test>]
let ``Should use context to compress related context fields``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let simpleResource =
    resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
      [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]
  let contexts = ["http://ld.nice.org.uk/ns/qualitystandard.jsonld"]

  let json = transformToJsonLD [[simpleResource]] contexts |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.HttpLdNiceOrgUkNsQualitystandardTitle.JsonValue.AsString() = "title goes here" @>
