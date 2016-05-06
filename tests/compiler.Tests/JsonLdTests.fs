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
let ``Should return a tuple with id as first and jsonld as second``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let resource = createResource "http://ld.nice.org.uk/qualitystatement/id_goes_here"

  let id,_ = transformToJsonLD qsContexts [[resource]] |> Seq.head
  id |> should equal "http://ld.nice.org.uk/qualitystatement/id_goes_here"

[<Test>]
let ``Should add a _id field with resource uri``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let uri = "http://ld.nice.org.uk/qualitystatement/id_goes_here"
  let resource = createResource uri

  let _,json = transformToJsonLD qsContexts [[resource]] |> Seq.head

  let jsonld = JsonLdSchema.Parse(json)

  jsonld.Id.JsonValue.AsString() |> should equal uri

[<Test>]
let ``Should add a _type field``() =
  let simpleResource = resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here"  []

  let contexts = []
  let _,json = transformToJsonLD contexts [[simpleResource]] |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  jsonld.Type.JsonValue.AsString() |> should equal "qualitystatement"

[<Test>]
let ``Should use context to compress related context fields``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let simpleResource =
    resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
      [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]

  let _,json = transformToJsonLD qsContexts [[simpleResource]] |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  jsonld.HttpLdNiceOrgUkNsQualitystandardTitle.JsonValue.AsString() |> should equal "title goes here"

[<Test>]
let ``Should process multiple resources``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let resource1 = createResource "http://ld.nice.org.uk/qualitystatement/1"
  let resource2 = createResource "http://ld.nice.org.uk/qualitystatement/2"

  let jsonlds = transformToJsonLD qsContexts [[resource1];[resource2]]

  jsonlds |> Seq.item 0 |> fst |> should equal "http://ld.nice.org.uk/qualitystatement/1"
  jsonlds |> Seq.item 1 |> fst |> should equal "http://ld.nice.org.uk/qualitystatement/2"
