module publish.Turtle

open FSharp.RDF
open Assertion

let transformToTurtle resource =
  let s = ""
  let sb = new System.Text.StringBuilder(s)
  
  let graph = Graph.empty (!!"http://ld.nice.org.uk/ns#") [("base",!!"http://ld.nice.org.uk/ns#")]
  
  [resource]
  |> Assert.graph graph
  |> Graph.writeTtl (toString sb)
  |> ignore
  
  sb.ToString()
  
