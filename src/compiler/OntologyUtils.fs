module compiler.OntologyUtils

open System
open System.IO
open FSharp.Data
open compiler.OntologyConfig
open Newtonsoft.Json
open FSharp.RDF
open compiler.RDF
open compiler.Utils

//let GetConfigFromFile =
//  let file = sprintf "%s\\OntologyConfig.json" __SOURCE_DIRECTORY__
//  //let file = sprintf "%s/OntologyConfig.json" 
//  if File.Exists file then
//    "Hurrah!! I have found the file. And there was much rejoicing"
//  else
//    "Nope"

let DeserializeConfig jsonString =
  let ret = JsonConvert.DeserializeObject<OntologyConfig>(jsonString)
  ret

let GetJsonLdContext oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.JsonLD))

let GetSchemaTtl oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.Schema))

let rec ConcatDelimit (list:string List) delimiter =
  if list = [] then
    ()
  if list.Length = 1 then
    list.Head
  else
    sprintf "%s%s%s" list.Head delimiter (ConcatDelimit list.Tail delimiter)

let GetPathWithSubclass urlBase qsBase p = 
  ConcatDelimit (p.PropertyPath |> List.map (fun pp -> (sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp))) "|"

let GetPropPaths oc =
  oc.SchemaDetails
    |> List.map (fun f -> (f.Publish 
                             |> List.map (fun p -> (if obj.ReferenceEquals(p.PropertyPath, null) then
                                                      sprintf "<%s%s#%s>" oc.UrlBase oc.QSBase p.Uri
                                                    else
                                                       GetPathWithSubclass oc.UrlBase oc.QSBase p))))
    |> List.concat

let GetVocabMap oc =
  oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (p.Uri, Uri.from(sprintf "<%s%s#%s>" oc.UrlBase oc.QSBase p.Uri)))))
    |> List.concat
    |> Map.ofList

let GetTermMap oc =
  oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (p.Uri, vocabLookup(sprintf "<%s#%s>" oc.SchemaBase f.Schema)))))

    |> List.concat
    |> Map.ofList

let GetRdfArgs oc =
  {
    BaseUrl = sprintf "%s%s" oc.UrlBase oc.ThingBase
    VocabMap = GetVocabMap oc
    TermMap = GetTermMap oc
  }

let ReadConfigFile fullpath =
  let ret = readHandle {Thing = fullpath; Content = ""}
  DeserializeConfig ret.Content