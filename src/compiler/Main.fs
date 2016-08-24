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

/////////////////////////////////////////////////////////////////

let compileAndPublish ( fetchUrl:string ) () =

  let extractor =
    {readAllContentItems = Git.readAll (Uri.from fetchUrl)
     readContentForItem = Git.readOne}

  prepare inputDir outputDir dbName dbUser dbPass

  let items = extractor.readAllContentItems ()
  let config = sprintf "%s/OntologyConfig.json" inputDir
                     |> getConfigFromFile
                     |> deserializeConfig

  downloadSchema (config |> getSchemaTtls) outputDir

  compile extractor items (config |> getRdfArgs) (config |> getBaseUrl)  (config |> getAnnotationValidations) outputDir dbName

  publish (config |> getPropPaths) (config |> getJsonLdContexts) outputDir config.IndexName config.TypeName 

  printf "Knowledge base creation complete!\n"
