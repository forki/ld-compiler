module compiler.Main 

open compiler
open compiler.ContentHandle
open compiler.ContentExtractor
open compiler.Compile
open compiler.Utils
open compiler.Markdown
open compiler.RDF
open compiler.Turtle
open compiler.Pandoc
open compiler.Publish
open compiler.Preamble
open compiler.OntologyUtils
open FSharp.RDF
open FSharp.Data

//// These should be passed in as arguments ////////
let private inputDir = "/git"
let private outputDir = "/artifacts"
let private dbName = "nice"
let private dbUser = "admin"
let private dbPass = "admin"

//// These get pulled in from the config file ///////

let mutable private baseUrl = "" 
let mutable private propertyPaths:string list = []
let mutable private jsonldContexts:string list = []
let mutable private schemas:string list = []
let mutable private indexName = ""
let mutable private typeName = ""

/////////////////////////////////////////////////////////////////

let compileAndPublish ( fetchUrl:string ) () =

  let extractor =
    {readAllContentItems = Git.readAll (Uri.from fetchUrl)
     readContentForItem = Git.readOne}

  prepare inputDir outputDir dbName dbUser dbPass schemas

  let items = extractor.readAllContentItems ()
  let config = sprintf "%s/OntologyConfig.json" inputDir
                     |> GetConfigFromFile
                     |> deserializeConfig
  
  let rdfArgs = config |> GetRdfArgs

  baseUrl <- config |> GetBaseUrl

  propertyPaths <- config |> getPropPaths
  jsonldContexts <- config |> getJsonLdContext
  schemas <- config |> getSchemaTtl
  indexName <- config.IndexName
  typeName <- config.TypeName

  downloadSchema schemas outputDir

  compile extractor items rdfArgs baseUrl outputDir dbName

  publish propertyPaths jsonldContexts outputDir indexName typeName 

  printf "Knowledge base creation complete!\n"
