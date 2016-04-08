module Program 

open publish
open publish.File
open publish.Markdown
open publish.RDF
open publish.Turtle
open System.IO

[<EntryPoint>]
let main args =
  let inputFile = args.[0]
  let outputFile = args.[1]
  printf "Input file: %s output file %s" inputFile outputFile
  let content = File.ReadAllText inputFile
  let file = {Path = inputFile; Content = content}

  let ttl =
    file
    |> extractStatement 
    |> transformToRDF  
    |> transformToTurtle

  File.WriteAllText( outputFile, ttl)

  0
