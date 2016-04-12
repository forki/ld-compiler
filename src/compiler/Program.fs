module Program 

open publish
open publish.File
open publish.Markdown
open publish.RDF
open publish.Turtle
open publish.Stardog
open System.IO

//let findFiles inputDir =
//  let dir = System.IO.DirectoryInfo(inputDir)
//  let files = dir.GetFiles("Statement.md", System.IO.SearchOption.AllDirectories)
//  files |> Array.map(fun fs -> {FilePath = fs.FullName}) |> Array.toList

[<EntryPoint>]
let main args =
  let inputFile = args.[0]
  let outputFile = args.[1]
  printf "Input file: %s output file %s" inputFile outputFile

  let content = File.ReadAllText inputFile
  let file = {Path = inputFile; Content = content}

  file
  |> extractStatement 
  |> transformToRDF  
  |> transformToTurtle
  |> Stardog.write
  |> ignore

  

  0
