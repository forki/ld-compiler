module compiler.ConfigUtils

open FSharp.Data
open compiler.ConfigTypes
open Newtonsoft.Json
open FSharp.RDF
open compiler.Utils

type RDFArgs = {
  VocabMap : Map<string, Uri>     
  TermMap : Map<string, Map<string, Uri>>
  BaseUrl : string
}

let mkKey (x : string) = x.Replace(" ", "").ToLowerInvariant()

let private getPathWithSubclass urlBase qsBase p =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp 
  let concatPropertyPaths acc prop = match acc with
                                     | "" -> prop
                                     | _ -> sprintf "%s%s%s" acc delimiter prop
  p.PropertyPath 
  |> List.map buildPropertyPathUri
  |> List.fold concatPropertyPaths ""  


let private getPropertyForLabel s (label:string) =
    match obj.ReferenceEquals(label, null) with
    |true -> s
    |_ -> getProperty label

let private vocabLookup uri =
  let rdfslbl = Uri.from "http://www.w3.org/2000/01/rdf-schema#label"
  let gcd = Graph.loadFrom uri
  let onlySome = List.choose id
  Resource.fromPredicate rdfslbl gcd
  |> List.map (fun r ->
       match r with
       | FunctionalDataProperty rdfslbl xsd.string x ->
         Some(mkKey x, Resource.id r)
       | _ -> None)
  |> onlySome
  |> Map.ofList

let private rdf_getVocabMap config =
  let getMmkVocabList p =
    p
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.map (fun p -> (getPropertyForLabel p.Uri p.Label, sprintf "%s%s#%s" config.UrlBase config.QSBase p.Uri))

  let getVocabList config =
    config.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish |> getMmkVocabList))
    |> List.concat

  getVocabList config
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let private rdf_getTermMap config =
  let getTermPublishList pl schema = 
    pl
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.filter (fun p -> p.PropertyPath.Length > 0)
    |> List.map (fun p -> (getPropertyForLabel p.Uri p.Label, sprintf "%s%s" config.SchemaBase schema)) 

  let getTermList config =
    config.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> getTermPublishList f.Publish f.Schema)
    |> List.concat

  getTermList config
  |> List.map (fun t -> (fst t, vocabLookup(snd t)))
  |> Map.ofList

let getBaseUrl config =
  sprintf "%s%s" config.UrlBase config.ThingBase

let getPropertyBaseUrl config =
  sprintf "%s%s" config.UrlBase config.QSBase

let getRdfArgs config =
  {
    BaseUrl = getBaseUrl config
    VocabMap = rdf_getVocabMap config
    TermMap = rdf_getTermMap config
  }

let getPropertyValidations config =
  config.SchemaDetails
  |> List.filter (fun x -> x.Map=false)
  |> List.map (fun f -> (f.Publish |> List.filter (fun p -> p.Validate)))
  |> List.concat

let deserializeConfig jsonString =
  JsonConvert.DeserializeObject<ConfigTypes.Config>(jsonString)

let getJsonLdContexts config =
  config.SchemaDetails
  |> List.map (fun f -> (sprintf "%s%s" config.SchemaBase f.JsonLD))

let getSchemaTtls config =
  config.SchemaDetails
  |> List.map (fun f -> (sprintf "%s%s" config.SchemaBase f.Schema))

let getPropPaths config =
  let isEmptyPropertyPathSet p =
    match obj.ReferenceEquals(p.PropertyPath, null) with
    | true -> true
    | _ -> p.PropertyPath.IsEmpty 
    
  let buildSchemaDetails p =
    match isEmptyPropertyPathSet p with
    | true ->  sprintf "<%s%s#%s>" config.UrlBase config.QSBase p.Uri
    | _ -> getPathWithSubclass config.UrlBase config.QSBase p

  config.SchemaDetails
  |> List.map (fun f -> f.Publish 
                        |> List.map (fun p -> buildSchemaDetails p))
  |> List.concat
  |> List.filter (fun f -> f <> "")
