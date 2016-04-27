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

let createDb () =
  // TODO: rewrite this script as a http request!
  let proc = Process.Start("createdb")
  let timeout = 10000

  proc.WaitForExit(timeout) |> ignore

let addGraph files =
  // TODO: figure out how to do use dotNetRDF/FSharp.RDF to do this.
  let args = sprintf "--named-graph http://ld.nice.org.uk/ %s" files
  let proc = Process.Start("addgraph", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore

let extractResources propertyPaths =
  printf "extractin resources from stardog...\n"
  let stardog =
    Store.Store.stardog "http://stardog:5820" "nice" "admin" "admin" false

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

  let rec retry f x =
    try
      f x
    with
      | e ->
        printf "Failure: %s" e.Message
        retry f x

  let firstPathOPath (s:System.String) =
    PathRegex().Match(s).firstPartOfPropertyPath.Value

  let construct =
    List.mapi (fun i v -> sprintf " @entity <%s> ?o_%d . " (firstPathOPath v) i)
    >> String.concat "\n"

  let queryResources () =
    stardog.queryResultSet [] """
      select distinct ?s
      from <http://ld.nice.org.uk/>
      where {
       ?s a <http://ld.nice.org.uk/ns/qualitystandard#QualityStatement>
      }""" [] |> ResultSet.singles |> asUri

  let querySubGraph entity =
    let clause = clause propertyPaths
    let construct = construct propertyPaths
    let query = (sprintf """
                       prefix nice: <http://ld.nice.org.uk/>
                       construct {
                         @entity a <http://ld.nice.org.uk/ns/qualitystandard#QualityStatement> .
                         %s
                       }
                       from <http://ld.nice.org.uk/ns>
                       from <http://ld.nice.org.uk/>
                       where {
                         @entity a <http://ld.nice.org.uk/ns/qualitystandard#QualityStatement> .
                         %s
                       }
               """ construct clause) 

    Graph.defaultPrefixes (Uri.from "http://ld.nice.org.uk/") [] (stardog.queryGraph [] query [ ("entity", Param.Uri entity) ])

  let resources = queryResources ()
  printf "extracted %d resources from stardog\n" ( Seq.length resources )
  let xr =
    resources
    |> Seq.map ( querySubGraph |> retry) 
    |> Seq.map
         (Resource.fromType
            (Uri.from "http://ld.nice.org.uk/ns/qualitystandard#QualityStatement"))
    |> Seq.filter (List.isEmpty >> not)
  printf "extracted %d subgraphs\n" (Seq.length xr)
  xr

