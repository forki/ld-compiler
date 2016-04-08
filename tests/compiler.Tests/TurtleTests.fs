module publish.Tests.TurtleTests

open NUnit.Framework
open Swensen.Unquote
open FSharp.RDF
open Assertion
open resource
open rdf
open publish.Turtle

[<Test>]
let ``Should serialize resource to ttl`` () =
  let resource = resource !! "http://someuri.com"
                  [dataProperty !!"http://someuri#someproperty" ("Some property"^^xsd.string)]

  let expected = """@base <http://ld.nice.org.uk/ns#>.

@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@prefix base: <http://ld.nice.org.uk/ns#>.
@prefix owl: <http://www.w3.org/2002/07/owl#>.

<http://someuri.com/> <http://someuri/#someproperty> "Some property"^^<http://www.w3.org/2001/XMLSchema#string>.
"""

  let actual = transformToTurtle resource

  test <@ actual = expected @>
