module Program 

open publish
open publish.File
open publish.Markdown
open publish.RDF
open publish.Turtle
open publish.Stardog
open publish.JsonLd
open publish.Elastic
open System.IO

let private writeToDisk outputDir (resources:string seq) =
  let outputFile = sprintf "%s/output.jsonld" outputDir
  printf "writing %s to disk\n" outputFile
  try 
    File.WriteAllText(outputFile, resources |> Seq.head)
    printf "finsihed writing %s to disk\n" outputFile
  with ex -> printf "problem writing %s to disk!\n" outputFile

let private findFiles inputDir filePattern =
  printf "finding %s files in %s\n" filePattern inputDir
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

[<EntryPoint>]
let main args =
  let inputFile = args.[0]
  let outputDir = args.[1]
  printf "Input file: %s\n" inputFile 
  printf "Output file: %s\n" outputDir 

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

  let indexName = "kb"
  let typeName = "qualitystatement"

  let content = File.ReadAllText inputFile
  let file = {Path = inputFile; Content = content}

  Stardog.createDb ()

  file
  |> extractStatement 
  |> transformToRDF  
  |> transformToTurtle
  |> Stardog.addGraph outputDir
  |> ignore

  printf "Extracting resources\n"
  let resources = Stardog.queryResources propertyPaths
  printf "Found %d resources\n" (Seq.length resources)
  resources
  |> transformToJsonLD contexts
  |> writeToDisk outputDir

  let jsonldFiles = findFiles outputDir "*.jsonld"
  printf "Found jsonld files: %A" jsonldFiles
  jsonldFiles
  |> Seq.map (fun f -> {Path = f; Content = File.ReadAllText f})
  |> bulkUpload indexName typeName

  0
