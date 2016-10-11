module compiler.Main 

open Serilog
open NICE.Logging
open compiler.ContentExtractor
open compiler.Compile
open compiler.Publish
open compiler.Preamble
open FSharp.RDF

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

  Log.Information "Knowledge base creation complete!"
