module compiler.Compile 

open publish
open publish.File
open publish.Markdown
open publish.RDF
open publish.Turtle
open publish.Stardog
open publish.JsonLd
open publish.Elastic
open compiler.Pandoc
open FSharp.RDF
open FSharp.Data
open System.IO

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

let private contexts = [
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

let private compileToRDF files rdfArgs baseUrl outputDir = 
  printf "Compiling files: %A\n" files
  let compile =
    readFile
    >> extractStatement
    >> convertMarkdownToHtml ( outputDir + "/published/qualitystandards/" )
    >> transformToRDF rdfArgs
    >> transformToTurtle
    >> prepareAsFile baseUrl outputDir ".ttl"
    >> writeFile 
  files |> Seq.iter (fun file -> try compile file with ex -> printf "[ERROR] problem processing file %s with: %s\n" file ( ex.ToString() ))

let private downloadSchema schemas outputDir =
  let download (schema:string) =
    {Path = sprintf "%s/%s" outputDir (schema.Remove(0,schema.LastIndexOf('/')+1))
     Content = Http.RequestString(schema)}
  
  List.iter (download >> writeFile) schemas

let private addGraphs outputDir dbName = 
  let concatToArgs turtles = List.fold (fun acc file -> file + " " + acc) "" turtles

  let turtles = findFiles outputDir "*.ttl"
  turtles 
  |> concatToArgs 
  |> Stardog.addGraph dbName

let private publishResources propertyPaths indexName typeName =
  printf "Publishing resources\n"
  let resources = Stardog.extractResources propertyPaths
  resources
  |> transformToJsonLD contexts
  |> bulkUpload indexName typeName

let compile gitRepoUrl () =
  printf "Input directory : %s\n" inputDir 
  printf "Output directory : %s\n" outputDir 
  try 
    Directory.Delete(inputDir, true)
    Directory.Delete(outputDir, true)
  with ex -> ()
  Directory.CreateDirectory inputDir |> ignore
  Directory.CreateDirectory outputDir |> ignore

  Git.clone gitRepoUrl inputDir
  Stardog.deleteDb dbName dbUser dbPass
  Stardog.createDb dbName
  downloadSchema schemas outputDir

  let files = findFiles inputDir "Statement.md"
  compileToRDF files rdfArgs baseUrl outputDir
  addGraphs outputDir dbName
  publishResources propertyPaths indexName typeName

  printf "Knowledge base creation complete!\n"
