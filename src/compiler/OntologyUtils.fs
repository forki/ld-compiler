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

let getGetMmKey s (l:string) =
    match obj.ReferenceEquals(l, null) with
    |true -> s
    |_ -> l.ToLower().Replace(" ","")

let GetVocabList oc =
  oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s#%s" oc.UrlBase oc.QSBase p.Uri))))
    |> List.concat

let GetVocabMap oc =
  GetVocabList oc
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let GetTermList oc =
    oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish 
                             |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
                             |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s" oc.SchemaBase f.Schema))))

    |> List.concat

let GetTermMap oc =
  GetTermList oc
    |> List.map (fun t -> (fst t, vocabLookup(snd t)))
    |> Map.ofList

let GetBaseUrl oc =
  sprintf "%s%s" oc.UrlBase oc.ThingBase

let GetRdfArgs oc =
  {
    BaseUrl = GetBaseUrl oc
    VocabMap = GetVocabMap oc
    TermMap = GetTermMap oc
  }

let ReadConfigFile fullpath =
  let ret = readHandle {Thing = fullpath; Content = ""}
  DeserializeConfig ret.Content