module compiler.Stardog

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
  with _ -> printf "Database does not exist yet"

let addGraph dbName files =
  // TODO: figure out how to do use dotNetRDF/FSharp.RDF to do this.
  let args = sprintf "%s --named-graph http://ld.nice.org.uk/ %s" dbName files
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
                         { @entity a <http://ld.nice.org.uk/ns/qualitystandard#QualityStatement> . }
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

