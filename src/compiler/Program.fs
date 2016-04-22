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

let private findFiles inputDir filePattern =
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

let private readFile file =
  {Path = file; Content = File.ReadAllText file}

let private writeFile file =
  try 
    File.WriteAllText(file.Path, file.Content)
    printf "Written %s\n" file.Path
  with ex -> printf "Couldnt write %s to disk!\n" file.Path

let private prepareAsFile baseUrl outputDir ext (id:string, jsonld) =
  let id = id.Replace(baseUrl+"/", "").Replace("/","_")
  {Path = sprintf "%s/%s%s" outputDir id ext; Content = jsonld}

let private compileToRDF files baseUrl outputDir = 
  printf "Compiling files: %A\n" files
  let compile =
    readFile
    >> extractStatement
    >> transformToRDF baseUrl
    >> transformToTurtle
    >> prepareAsFile baseUrl outputDir ".ttl"
    >> writeFile 
  files |> Seq.iter (fun file -> try compile file with ex -> printf "[ERROR] problem processing file %s with: %s\n" file ( ex.ToString() ))

let private addGraphs outputDir = 
  let concatToArgs turtles = List.fold (fun acc file -> file + " " + acc) "" turtles

  let turtles = findFiles outputDir "*.ttl"
  turtles 
  |> concatToArgs 
  |> Stardog.addGraph

let private extractResources propertyPaths baseUrl outputDir =
  printf "Extracting resources\n"
  let resources = Stardog.queryResources propertyPaths
  resources
  |> transformToJsonLD contexts
  |> Seq.map (fun f -> prepareAsFile baseUrl outputDir ".jsonld" f)
  |> Seq.iter writeFile

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
  addGraphs outputDir
  extractResources propertyPaths baseUrl outputDir

  publishResources outputDir indexName typeName

  0
