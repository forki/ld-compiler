module publish.Stardog

open System.Diagnostics
open System.IO
open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open FSharp.Data
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.Text.RegexProvider
type PathRegex = Regex< ".*<(?<firstPartOfPropertyPath>.*)>.*">

let write ttl =
  // TODO: figure out how to do use dotNetRDF/FSharp.RDF to do this.
  let filePath = "$ARTIFACTS_DIR/output.ttl"
  File.WriteAllText(filePath, ttl) 
  let cmd = sprintf "addgraph --named-graph http://ld.nice.org.uk/ %s" filePath
  let proc = Process.Start(cmd)
  let timeout = 10000

  proc.WaitForExit(timeout) |> ignore
  File.Delete filePath
  

let queryResources propertyPaths () =

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
  
  let clause =
    List.mapi (fun i v -> sprintf "optional { @entity %s ?o_%d . } " v i)
    >> String.concat "\n"

  let firstPathOPath (s:System.String) =
    PathRegex().Match(s).firstPartOfPropertyPath.Value

  let construct =
    List.mapi (fun i v -> sprintf " @entity <%s> ?o_%d . " (firstPathOPath v) i)
    >> String.concat "\n"

  let queryResources () =
    stardog.queryResultSet [] "select distinct ?s from <http://ld.nice.org.uk/> where { ?s ?o ?p }" [] |> ResultSet.singles |> asUri

  let querySubGraph entity =
    let clause = clause propertyPaths
    let construct = construct propertyPaths
    let query = (sprintf """
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

  let resources = queryResources ()
  resources
  |> Seq.map querySubGraph 
  |> Seq.map
       (Resource.fromType
          (Uri.from "http://www.w3.org/2002/07/owl#NamedIndividual"))
  |> Seq.filter (List.isEmpty >> not)

