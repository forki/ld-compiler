module compiler.Compile

open FSharp.RDF
open FSharp.Data
open System.IO
open compiler.ContentHandle
open compiler.ContentExtractor
open compiler.Utils
open compiler.Markdown
open compiler.RDF
open compiler.Turtle
open compiler.Pandoc
open compiler.Publish
open compiler

let private addGraphs outputDir dbName = 
  let concatToArgs turtles = List.fold (fun acc file -> file + " " + acc) "" turtles

  findFiles outputDir "*.ttl"
  |> concatToArgs 
  |> Stardog.addGraph dbName

let compile extractor rdfArgs baseUrl outputDir dbName = 
  let items = extractor.readAllContentItems ()

  let compileItem =
    extractor.readContentForItem
    >> extractStatement
    >> convertMarkdownToHtml outputDir
    >> transformToRDF rdfArgs
    >> transformToTurtle
    >> prepareAsFile baseUrl outputDir ".ttl"
    >> writeFile 

  items |> Seq.iter (fun item -> try compileItem item with ex -> printf "[ERROR] problem processing item %s with: %s\n" item.Path ( ex.ToString() ))

  addGraphs outputDir dbName

