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

//// These should be passed in as arguments ////////
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

let private contexts = [
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

let private baseUrl = "http://ld.nice.org.uk/qualitystatement" 

let private indexName = "kb"
let private typeName = "qualitystatement"
/////////////////////////////////////////////////////////////////

let private writeToDisk outputDir (id,content) =
  let outputFile = sprintf "%s/%s.jsonld" outputDir id
  try 
    File.WriteAllText(outputFile, content)
    printf "Written %s\n" outputFile
  with ex -> printf "Couldnt write %s to disk!\n" outputFile

let private findFiles inputDir filePattern =
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

let private readFile file =
  {Path = file; Content = File.ReadAllText file}

let private compileToRDF files baseUrl outputDir = 
  printf "Compiling files: %A\n" files
  let compile =
    readFile
    >> extractStatement
    >> transformToRDF baseUrl
    >> transformToTurtle
    >> (Stardog.addGraph outputDir)

  files |> Seq.iter compile

let private extractResources propertyPaths baseUrl outputDir =
  printf "Extracting resources\n"
  let resources = Stardog.queryResources propertyPaths
  resources
  |> transformToJsonLD baseUrl contexts
  |> Seq.iter (writeToDisk outputDir)

let private publishResources outputDir indexName typeName = 
  printf "Publishing resources:\n"
  let jsonldFiles = findFiles outputDir "*.jsonld"
  printf "Found jsonld files: %A\n" jsonldFiles
  jsonldFiles
  |> Seq.map readFile
  |> bulkUpload indexName typeName

[<EntryPoint>]
let main args =
  let inputDir = args.[0]
  let outputDir = args.[1]
  printf "Input directory : %s\n" inputDir 
  printf "Output directory : %s\n" outputDir 
  let files = findFiles inputDir "Statement.md"

  Stardog.createDb ()

  compileToRDF files baseUrl outputDir

  extractResources propertyPaths baseUrl outputDir

  publishResources outputDir indexName typeName

  0
