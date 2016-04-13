module publish.Test.JsonLdTests

open NUnit.Framework
open Swensen.Unquote
open publish.JsonLd
open FSharp.Data
open FSharp.RDF
open Assertion
open resource
open rdf

type JsonLdSchema = JsonProvider<""" { "http://ld.nice.org.uk/ns/qualitystandard#title":"" } """>

[<Test>]
let ``Should convert simple rdf resource to jsonld with context``() =

  let r =
    resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
      [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]

  let contexts = [
      "http://ld.nice.org.uk/ns/qualitystandard.jsonld "
    ]

  let json = transformToJsonLD [[r]] contexts |> Seq.head
  let jsonld = JsonLdSchema.Parse(json)

  test <@ jsonld.HttpLdNiceOrgUkNsQualitystandardTitle.JsonValue.AsString() = "title goes here" @>
