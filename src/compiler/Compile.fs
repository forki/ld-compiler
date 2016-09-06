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
open compiler.Domain
open compiler.ValidationUtils
open compiler.ConfigUtils
open compiler.BindDataToHtml
open compiler

let private addGraphs outputDir dbName = 
  let concatToArgs turtles = List.fold (fun acc file -> file + " " + acc) "" turtles

  findFiles outputDir "*.ttl"
  |> concatToArgs 
  |> Stardog.addGraph dbName

let writeHtml outputDir statement = 
  prepareAsFile "notused" outputDir ".html" (statement.Id, statement.Html)
  |> writeFile

  statement

let compile config extractor items outputDir dbName = 
  let rdfArgs = config |> getRdfArgs
  let baseUrl = config |> getBaseUrl
  let propertyBaseUrl = config |> getPropertyBaseUrl
  let annotationConfig = config |> getAnnotationConfig

  let compileItem =
    extractor.readContentForItem
    >> convertMarkdownToHtml 
    >> extractStatement config
    >> validateStatement annotationConfig
    >> bindDataToHtml
    >> writeHtml outputDir
    >> transformToRDF rdfArgs annotationConfig propertyBaseUrl
    >> transformToTurtle
    >> prepareAsFile baseUrl outputDir ".ttl"
    >> writeFile 

  items |> Seq.iter (fun item -> try compileItem item with ex -> printf "[ERROR] problem processing item %s with: %s\n" item.Thing ( ex.ToString() ))

  addGraphs outputDir dbName

