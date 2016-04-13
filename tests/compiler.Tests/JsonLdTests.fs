module publish.Test.JsonLdTests

open NUnit.Framework
open Swensen.Unquote
open publish.JsonLd
open FSharp.RDF
open Assertion
open resource
open rdf

[<Test>]
let ``Should convert rdf resource to json-ld``() =

  let r =
    resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here" 
      [a !! "owl:NamedIndividual"
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("title goes here"^^xsd.string)]

  let contexts = [
      "http://ld.nice.org.uk/ns/qualitystandard.jsonld "
    ]

  let json = transformToJsonLD [[r]] contexts 

  test <@ json |> Seq.head = "hahaha" @>
