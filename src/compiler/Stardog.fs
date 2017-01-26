module compiler.Stardog

open compiler.ConfigTypes
open Serilog
open NICE.Logging
open System.Diagnostics
open System.IO
open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open FSharp.Data
open FSharp.Data.HttpRequestHeaders
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.Text.RegexProvider

type PathRegex = Regex< ".*<(?<firstPartOfPropertyPath>.*)>.*">
open System.Threading


let createDb dbName =
  // TODO: rewrite this script as a http request!
  let proc = Process.Start("createdb", dbName)
  let timeout = 10000

  proc.WaitForExit(timeout) |> ignore

let deleteDb dbName dbUser dbPass =
  try
    Http.RequestString ( "http://stardog:5820/admin/databases/"+dbName, httpMethod = "DELETE", headers = [ BasicAuth dbUser dbPass] ) |> ignore
  with _ -> Log.Warning "Database does not exist yet"

let addGraph dbName files =
  // TODO: figure out how to do use dotNetRDF/FSharp.RDF to do this.
  let args = sprintf "%s --named-graph https://nice.org.uk/ %s" dbName files
  let proc = Process.Start("addgraph", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore

let extractResources (config:ConfigDetails) =
  Log.Information "extracting resources from stardog..."
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
    List.mapi (fun i v -> sprintf "UNION { @entity %s ?o_%d . } " v i)
    >> String.concat "\n"

  let rec retry f x =
    try
      f x
    with
      | e ->
        printf "Failure: %s" e.Message
        Thread.Sleep(2)
        retry f x

  let firstPathOPath (s:System.String) =
    PathRegex().TypedMatch(s).firstPartOfPropertyPath.Value

  let construct =
    List.mapi (fun i v -> sprintf " @entity <%s> ?o_%d . " (firstPathOPath v) i)
    >> String.concat "\n"

  let queryResources () =
    stardog.queryResultSet [] """
      select distinct ?s
      from <https://nice.org.uk/>
      where {
       ?s a <https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db>
      }""" [] |> ResultSet.singles |> asUri

  let querySubGraph entity =
    let clause = clause config.PropPaths
    let construct = construct config.PropPaths
    let query = (sprintf """
                       prefix nice: <https://nice.org.uk/>
                       construct {
                         @entity a <https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db> .
                         %s
                       }
                       from <https://nice.org.uk/ontologies>
                       from <https://nice.org.uk/>
                       where {
                         { @entity a <https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db> . }
                         %s
                       }
               """ construct clause)
    
    Graph.defaultPrefixes (Uri.from "https://nice.org.uk/") [] (stardog.queryGraph [] query [ ("entity", Param.Uri entity) ])
  
  let queryExplicitResources entity =
    let clause = clause config.ObjectPropertyPaths
    let construct = construct config.ObjectPropertyPaths
    let query = (sprintf """
                       prefix nice: <https://nice.org.uk/>
                       construct {
                         @entity a <https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db> .
                         %s
                       }
                       from <https://nice.org.uk/ontologies>
                       from <https://nice.org.uk/>
                       where {
                         { @entity a <https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db> . }
                         %s
                       }
               """ construct clause)

    Graph.defaultPrefixes (Uri.from "https://nice.org.uk/") [] (stardog.queryGraph [] query [ ("entity", Param.Uri entity) ])

  let resources = queryResources ()
  
  Log.Information (sprintf "extracted %d resources from stardog" ( Seq.length resources ))
  
  let xr =
    resources
    |> Seq.map ( querySubGraph |> retry) 
    |> Seq.map
         (Resource.fromType
            (Uri.from "https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db"))
    |> Seq.filter (List.isEmpty >> not)
  
  Log.Information (sprintf "extracted %d subgraphs" (Seq.length xr))
  
  let explicitResources =
    resources
    |> Seq.map ( queryExplicitResources |> retry)
    |> Seq.map
         (Resource.fromType
            (Uri.from "https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db"))
    |> Seq.filter (List.isEmpty >> not)

  Log.Information (sprintf "extracted explicit %d subgraphs" (Seq.length explicitResources))
  
  let allResources =
    Map.empty.
        Add("allResources", xr).
        Add("explicitResources", explicitResources)

  allResources
