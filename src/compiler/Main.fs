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
open FSharp.RDF
open FSharp.Data

//// These should be passed in as arguments ////////
let private inputDir = "/git"
let private outputDir = "/artifacts"
let private dbName = "nice"
let private dbUser = "admin"
let private dbPass = "admin"

let private baseUrl = "http://ld.nice.org.uk/qualitystatement" 

let private rdfArgs = {
  BaseUrl = baseUrl    
  VocabMap = 
    ([ "setting", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#setting"
       "agegroup", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#age"
       "conditionordisease", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#condition"
       "servicearea", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#serviceArea"
       "lifestylecondition", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#lifestyleCondition" ] |> Map.ofList)
  TermMap = 
    ([ "setting", vocabLookup "http://ld.nice.org.uk/ns/qualitystandard/setting.ttl"
       "agegroup", vocabLookup "http://ld.nice.org.uk/ns/qualitystandard/agegroup.ttl"
       "lifestylecondition", vocabLookup "http://ld.nice.org.uk/ns/qualitystandard/lifestylecondition.ttl"
       "conditionordisease", vocabLookup "http://ld.nice.org.uk/ns/qualitystandard/conditionordisease.ttl"
       "servicearea", vocabLookup "http://ld.nice.org.uk/ns/qualitystandard/servicearea.ttl" ] |> Map.ofList)
}
let private propertyPaths = [ 
  "<http://ld.nice.org.uk/ns/qualitystandard#age>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#age>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#condition>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#condition>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#setting>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#serviceArea>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#lifestyleCondition>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#title>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#abstract>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#qsidentifier>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#stidentifier>"
]

let private jsonldContexts = [
  "http://ld.nice.org.uk/ns/qualitystandard.jsonld "
  "http://ld.nice.org.uk/ns/qualitystandard/conditionordisease.jsonld "
  "http://ld.nice.org.uk/ns/qualitystandard/agegroup.jsonld "
  "http://ld.nice.org.uk/ns/qualitystandard/lifestylecondition.jsonld "
  "http://ld.nice.org.uk/ns/qualitystandard/setting.jsonld "
  "http://ld.nice.org.uk/ns/qualitystandard/servicearea.jsonld "
]

let private schemas = [
  "http://schema/ns/qualitystandard.ttl"
  "http://schema/ns/qualitystandard/agegroup.ttl"
  "http://schema/ns/qualitystandard/conditionordisease.ttl"
  "http://schema/ns/qualitystandard/lifestylecondition.ttl"
  "http://schema/ns/qualitystandard/setting.ttl"
  "http://schema/ns/qualitystandard/servicearea.ttl"
]

let private indexName = "kb"
let private typeName = "qualitystatement"
/////////////////////////////////////////////////////////////////

let compileAndPublish ( fetchUrl:string ) () =

  let extractor =
    {readAllContentItems = Git.readAll (Uri.from fetchUrl)
     readContentForItem = Git.readOne}

  prepare inputDir outputDir dbName dbUser dbPass schemas

  compile extractor rdfArgs baseUrl outputDir dbName
  publish propertyPaths jsonldContexts indexName typeName 

  printf "Knowledge base creation complete!\n"
