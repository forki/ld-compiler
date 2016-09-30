module compiler.Main 

open compiler
open compiler.ContentHandle
open compiler.ContentExtractor
open compiler.Compile
open compiler.Utils
open compiler.MarkdownParser
open compiler.RDF
open compiler.Turtle
open compiler.Pandoc
open compiler.Publish
open compiler.Preamble
//open compiler.ConfigUtils
open FSharp.RDF
open FSharp.Data

//// These should be passed in as arguments ////////
let private outputDir = "/artifacts"
let private dbName = "nice"
let private dbUser = "admin"
let private dbPass = "admin"

/////////////////////////////////////////////////////////////////

let compileAndPublish ( fetchUrl:string ) () =

  let extractor =
    {readAllContentItems = Git.readAll (Uri.from fetchUrl)
     readContentForItem = Git.readOne
     readConfig = Git.readConfig
     prepare = Git.prepare}

  extractor.prepare ()

  prepare outputDir dbName dbUser dbPass

  let items = extractor.readAllContentItems ()
  let config = extractor.readConfig ()

  downloadSchema config outputDir

  compile config extractor items outputDir dbName

  publish outputDir config

  printf "Knowledge base creation complete!\n"
