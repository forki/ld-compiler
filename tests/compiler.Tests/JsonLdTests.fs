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

let private baseUrl = "http://ld.nice.org.uk/qualitystatement" 

let private qsContexts = ["http://ld.nice.org.uk/ns/qualitystandard.jsonld"]
let private createResource uri =
  resource !! uri 
    [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("Not used"^^xsd.string)]

[<Test>]
let ``Should return a tuple with id as first and jsonld as second``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let resource = createResource "http://ld.nice.org.uk/qualitystatement/id_goes_here"

  let id,json = transformToJsonLD baseUrl qsContexts [[resource]] |> Seq.head
  printf "%s" json
  test <@ id = "id_goes_here" @>

[<Test>]
let ``Should add a _id field with resource uri``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let uri = "http://ld.nice.org.uk/qualitystatement/id_goes_here"
  let resource = createResource uri

  let _,json = transformToJsonLD baseUrl qsContexts [[resource]] |> Seq.head

  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.Id.JsonValue.AsString() = uri @>

[<Test>]
let ``Should add a _type field``() =
  let simpleResource = resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here"  []

  let contexts = []
  let _,json = transformToJsonLD baseUrl contexts [[simpleResource]] |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.Type.JsonValue.AsString() = "qualitystatement" @>

[<Test>]
let ``Should use context to compress related context fields``() =
  // This test requires internet access to load the remote json-ld context (possibly a way to stub this part out using JsonLD.Core / FSharp.RDF)
  let simpleResource =
    resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
      [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]

  let _,json = transformToJsonLD baseUrl qsContexts [[simpleResource]] |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.HttpLdNiceOrgUkNsQualitystandardTitle.JsonValue.AsString() = "title goes here" @>
