module compiler.OntologyUtils

open System
open System.IO
open FSharp.Data
open compiler.OntologyConfig
open Newtonsoft.Json
open FSharp.RDF
open compiler.RDF
open compiler.Utils
open System.Text

let GetConfigFromFile file =
  if File.Exists file then
    File.ReadAllText(file, Encoding.UTF8 )
  else
    ""

let deserializeConfig jsonString =
  JsonConvert.DeserializeObject<OntologyConfig>(jsonString)

let ReadConfigFile fullpath =
  let ret = readHandle {Thing = fullpath; Content = ""}
  deserializeConfig ret.Content

let getJsonLdContext oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.JsonLD))

let getSchemaTtl oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.Schema))

let getPathWithSubclass urlBase qsBase p =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp 
  let concatPropertyPaths acc prop = match acc with
                                     | "" -> prop
                                     | _ -> sprintf "%s%s%s" acc delimiter prop
  p.PropertyPath 
  |> List.map buildPropertyPathUri
  |> List.fold concatPropertyPaths ""  

let getPropPaths oc =
  oc.SchemaDetails
    |> List.map (fun f -> (f.Publish 
                             |> List.map (fun p -> (if obj.ReferenceEquals(p.PropertyPath, null) then
                                                      sprintf "<%s%s#%s>" oc.UrlBase oc.QSBase p.Uri
                                                    else
                                                       getPathWithSubclass oc.UrlBase oc.QSBase p))))
    |> List.concat

let private getGetMmKey s (l:string) =
    match obj.ReferenceEquals(l, null) with
    |true -> s
    |_ -> l.ToLower().Replace(" ","")

let getVocabList oc =
  oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s#%s" oc.UrlBase oc.QSBase p.Uri))))
    |> List.concat

let getVocabMap oc =
  getVocabList oc
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let getTermList oc =
    oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s" oc.SchemaBase f.Schema))))

    |> List.concat

let getTermMap oc =
  getTermList oc
    |> List.map (fun t -> (fst t, vocabLookup(snd t)))
    |> Map.ofList

let getBaseUrl oc =
  sprintf "%s%s" oc.UrlBase oc.ThingBase

let getRdfArgs oc =
  {
    BaseUrl = getBaseUrl oc
    VocabMap = getVocabMap oc
    TermMap = getTermMap oc
  }