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
  printf "Input file: %s arse" inputFile 
  printf "Input file: %s arse" inputFile 

  let propertyPaths = [ 
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

  let contexts = [
    "http://ld.nice.org.uk/ns/prov.jsonld"
    "http://ld.nice.org.uk/ns/owl.jsonld "
    "http://ld.nice.org.uk/ns/dcterms.jsonld"
    "http://ld.nice.org.uk/ns/content.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard/conditionordisease.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard/agegroup.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard/lifestylecondition.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard/setting.jsonld "
    "http://ld.nice.org.uk/ns/qualitystandard/servicearea.jsonld "
  ]

  let content = File.ReadAllText inputFile
  let file = {Path = inputFile; Content = content}

  file
  |> extractStatement 
  |> transformToRDF  
  |> transformToTurtle
  |> Stardog.write
  |> ignore

  Stardog.queryResources propertyPaths |> ignore
  // |> transformToJsonLD contexts
  // |> writeToDisk
  // |> uploadToElastic

  0
