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

let getConfigFromFile file =
  if File.Exists file then
    File.ReadAllText(file, Encoding.UTF8 )
  else
    ""

let deserializeConfig jsonString =
  JsonConvert.DeserializeObject<OntologyConfig>(jsonString)

let getJsonLdContexts oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.JsonLD))

let getSchemaTtls oc =
  oc.SchemaDetails
    |> List.map (fun f -> (sprintf "%s%s" oc.SchemaBase f.Schema))

let private getPathWithSubclass urlBase qsBase p =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp 
  let concatPropertyPaths acc prop = match acc with
                                     | "" -> prop
                                     | _ -> sprintf "%s%s%s" acc delimiter prop
  p.PropertyPath 
  |> List.map buildPropertyPathUri
  |> List.fold concatPropertyPaths ""  

let getPropPaths oc =
  let buildSchemaDetails p = match obj.ReferenceEquals(p.PropertyPath, null) with
                             | true ->  sprintf "<%s%s#%s>" oc.UrlBase oc.QSBase p.Uri
                             | _ -> getPathWithSubclass oc.UrlBase oc.QSBase p
  oc.SchemaDetails
    |> List.map (fun f -> (f.Publish 
                             |> List.map (fun p -> buildSchemaDetails p)))
    |> List.concat

let private getGetMmKey s (l:string) =
    match obj.ReferenceEquals(l, null) with
    |true -> s
    |_ -> l.ToLower().Replace(" ","")

let rdf_getVocabMap oc =
  let getMmkVocabList p =
    p
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s#%s" oc.UrlBase oc.QSBase p.Uri))

  let getVocabList oc =
    oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish |> getMmkVocabList))
    |> List.concat

  getVocabList oc
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let rdf_getTermMap oc =
  let getTermPublishList pl schema =
    pl
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.filter (fun p -> p.PropertyPath.Length > 0)
    |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s" oc.SchemaBase schema)) 
 
  let getTermList oc =
    oc.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> getTermPublishList f.Publish f.Schema)
    |> List.concat

  getTermList oc
    |> List.map (fun t -> (fst t, vocabLookup(snd t)))
    |> Map.ofList

let getBaseUrl oc =
  sprintf "%s%s" oc.UrlBase oc.ThingBase

let getRdfArgs oc =
  {
    BaseUrl = getBaseUrl oc
    VocabMap = rdf_getVocabMap oc
    TermMap = rdf_getTermMap oc
  }

let getAnnotatationValidations oc =
  oc.SchemaDetails
    |> List.filter (fun x -> x.Map=false)
    |> List.map (fun f -> (f.Publish 
                            |> List.filter (fun p -> p.Required=true)
                        ))
    |> List.concat

