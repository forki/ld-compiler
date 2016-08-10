module compiler.Tests.TurtleTests

open NUnit.Framework
open FsUnit
open FSharp.RDF
open Assertion
open resource
open rdf
open compiler.Turtle

let nl:string = System.Environment.NewLine

[<Test>]
let ``Should serialize resource to ttl`` () =
  let resource = resource !! "http://someuri.com/"
                  [dataProperty !!"http://someuri.com/#someproperty" ("Some property"^^xsd.string)]

  let expectedTtl = """@base <http://ld.nice.org.uk/>.

@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#>.
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#>.
@prefix xsd: <http://www.w3.org/2001/XMLSchema#>.
@prefix base: <http://ld.nice.org.uk/>.
@prefix owl: <http://www.w3.org/2002/07/owl#>.

<http://someuri.com/> <http://someuri.com/#someproperty> "Some property"^^<http://www.w3.org/2001/XMLSchema#string>.
"""

  let (id,ttl) = transformToTurtle resource

  ttl.Replace(nl,"\n") |> should equal expectedTtl
  id |> should equal "http://someuri.com/"
