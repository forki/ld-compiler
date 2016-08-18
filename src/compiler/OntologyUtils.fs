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
  let ret = JsonConvert.DeserializeObject<OntologyConfig>(jsonString)
  ret

let ReadConfigFile fullpath =
  let ret = readHandle {Thing = fullpath; Content = ""}
  deserializeConfig ret.Content

let getJsonLdContext oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.JsonLD))

let getSchemaTtl oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.Schema))

let rec concatDelimit (list:string List) delimiter =
  if list = [] then
    ()
  if list.Length = 1 then
    list.Head
  else
    sprintf "%s%s%s" list.Head delimiter (concatDelimit list.Tail delimiter)

let getPathWithSubclass urlBase qsBase p = 
  concatDelimit (p.PropertyPath |> List.map (fun pp -> (sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp))) "|"

let getPropPaths oc =
  oc.SchemaDetails
    |> List.map (fun f -> (f.Publish 
                             |> List.map (fun p -> (if obj.ReferenceEquals(p.PropertyPath, null) then
                                                      sprintf "<%s%s#%s>" oc.UrlBase oc.QSBase p.Uri
                                                    else
                                                       getPathWithSubclass oc.UrlBase oc.QSBase p))))
    |> List.concat

let getGetMmKey s (l:string) =
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