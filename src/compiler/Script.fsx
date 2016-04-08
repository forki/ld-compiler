#I "../../packages/FSharp.RDF/lib"
#r "FSharp.RDF.dll"
#r "dotNetRDF.dll"

open FSharp.RDF
open Assertion
open resource
open rdf

let title = "This is the title"
let abs = "This is the abstract"
let content = "This si the content"
let s = ""
let sb = new System.Text.StringBuilder(s)

let og = Graph.empty (!!"http://sometest/ns#") [("base",!!"http://sometest/ns#")]

let r = resource !! "http://someuri.com"
          [dataProperty !!"http://someuri#someproperty" ("Some property"^^xsd.string)]

[r]
|> Assert.graph og
|> Graph.writeTtl (toString sb)
|> ignore

s

open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Writing.Formatting
open VDS.RDF.Update
open VDS.RDF.Storage
open VDS.RDF.Parsing
open VDS.RDF.Query
open System.IO

//let sGraph = File.ReadAllText "sample.ttl" 
//let g2 = new Graph()
//g2.LoaIGraph g = new Graph();

let g = new Graph()
let h = new Graph()
let ttlparser = new TurtleParser()

//Load using a Filename
ttlparser.Load(g, "sample.ttl")

let loadGraph (g:string) =

  let graph = new Graph()
  graph.LoadFromString(g)
  printf "graph: %A" ( graph.ToString() )
  use dog = new StardogConnector("http://localhost:5820", "nice", "admin", "admin")
  dog.LoadGraph(graph, "http://ld.nice.org.uk")
  1

let ttl = sb.ToString()
loadGraph ttl

