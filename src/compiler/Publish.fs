module publish.Publish

open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open System.IO
open FSharp.Data
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.RDF.JsonLD
open FSharp.Text.RegexProvider
open JsonLD.Core
open Newtonsoft.Json.Linq
open Newtonsoft.Json
type iriP = Regex< "(?<prefix>.*):(?<fragment>.*)" >
type PathRegex = Regex< ".*<(?<firstPartOfPropertyPath>.*)>.*">

let stardog =
    Store.Store.stardog "http://localhost:5820" "nice" "admin" "admin" false

let urinode =
    function
    | Node.Uri x -> Some x
    | _ -> None

let asUri =
    Seq.map urinode
    >> Seq.filter Option.isSome
    >> Seq.map Option.get

let resources =
    (stardog.queryResultSet [] """
select distinct ?s
from <http://ld.nice.org.uk/>
where { ?s ?o ?p }
          """ []
     |> ResultSet.singles
     |> asUri)


type PropertyPaths =
  | PropertyPaths of Uri list list

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

let contextArgs = [
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

let contextS (uri) = sprintf """ "%s" """ uri
let contexts = contextArgs |> List.map contextS
let context = (JObject.Parse(sprintf """ {
            "@context": [
                 {"@language" : "en"},
                 %s,
                 {"resource" : "http://ld.nice.org.uk/resource#" }
              ]
           }
    """ (String.concat ",\n" contexts)) :> JToken)

let clause =
  List.mapi (fun i v -> sprintf "optional { @entity %s ?o_%d . } " v i)
  >> String.concat "\n"

let firstPathOPath (s:System.String) =
  PathRegex().Match(s).firstPartOfPropertyPath.Value

let construct =
  List.mapi (fun i v -> sprintf " @entity <%s> ?o_%d . " (firstPathOPath v) i)
  >> String.concat "\n"


let subGraph entity =
  let clause = clause propertyPaths
  let construct = construct propertyPaths
  let query = (sprintf """
                       prefix prov: <http://www.w3.org/ns/prov#>
                       prefix nice: <http://ld.nice.org.uk/>
                       construct {
                         @entity a owl:NamedIndividual .
                         %s
                       }
                       from <http://ld.nice.org.uk/ns>
                       from <http://ld.nice.org.uk/>
                       where {
                         @entity a owl:NamedIndividual .
                         %s
                       }
               """ construct clause) 

  Graph.defaultPrefixes (Uri.from "http://ld.nice.org.uk/") [] (stardog.queryGraph [] query [ ("entity", Param.Uri entity) ])


///Append _id and _type, kill context for now as elastic doesn't like the remotes
///Would whine about type implicit from structure but this would be a bit hypocritical
let elasiticerise (x : JObject) =
  x.["_id"] <- x.["prov:specializationOf"]
  x.Add("_type",JValue("qualitystatement"))
  x.Remove("@context") |> ignore
  x

let opts = JsonLdOptions()
opts.SetCompactArrays(true)
opts.SetUseRdfType(false)
opts.SetUseNativeTypes(true)
opts.SetEmbed(System.Nullable<_>(true))
opts.SetExplicit(System.Nullable<_>(false))

let toPublish = 
  let xr =
    (resources
     |> Seq.map subGraph 
     |> Seq.map
          (Resource.fromType
             (Uri.from "http://www.w3.org/2002/07/owl#NamedIndividual"))
    |> Seq.filter (List.isEmpty >> not))
  Seq.map
    ((Resource.compatctedJsonLD opts (Context(context, opts)))
     >> elasiticerise) xr


toPublish
  |> Seq.iter (fun x ->
                 let id = (x.["_id"]).Value<string>() //ugh
                 printfn "Writing ld for %s" id
                 use fout =
                   System.IO.File.CreateText "output.json"
                 use w = new JsonTextWriter(fout)
                 x.WriteTo w)
