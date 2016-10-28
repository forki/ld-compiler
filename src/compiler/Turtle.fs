module compiler.Turtle

open FSharp.RDF
open Assertion

let transformToTurtle resource =
  let s = ""
  let sb = new System.Text.StringBuilder(s)
  
  let graph = Graph.empty (!!"https://nice.org.uk/") [("base",!!"https://nice.org.uk/")]
  
  [resource]
  |> Assert.graph graph
  |> Graph.writeTtl (toString sb)
  |> ignore
  let id = Resource.id resource 
  (id.ToString(), sb.ToString())
  
