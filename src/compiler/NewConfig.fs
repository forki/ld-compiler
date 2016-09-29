module compiler.NewConfig

open Newtonsoft.Json
open compiler.ConfigTypes
open compiler.Utils
open compiler.Domain
open FSharp.RDF


let private buildUrl baseUrl path =
  sprintf "%s%s" baseUrl path

// move these to ConfigTypes?????
let private getJsonLd item = item.JsonLD
let private getTtl item = item.Schema

let private getPropertySet (config:compiler.ConfigTypes.Config) getPropFn  =
  config.SchemaDetails
  |> List.map (getPropFn >> buildUrl config.SchemaBase)

let private getPathWithSubclass urlBase qsBase (p:PublishItem) =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp 
  let concatPropertyPaths acc prop = match acc with
                                     | "" -> prop
                                     | _ -> sprintf "%s%s%s" acc delimiter prop
  p.PropertyPath 
  |> List.map buildPropertyPathUri
  |> List.fold concatPropertyPaths ""  

let private getPropPaths config =
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

let private getAnnotationConfig (config:compiler.ConfigTypes.Config) =
  config.SchemaDetails
  |> List.map (fun f -> f.Publish |> List.filter(fun p -> p.Validate))
  |> List.concat

let private getPropertyForLabel s label =
    match label |> isNullOrWhitespace with
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
         Some(getProperty x, Resource.id r)
       | _ -> None)
  |> onlySome
  |> Map.ofList

let private getRdfTerms (config:compiler.ConfigTypes.Config) =
  config.SchemaDetails
  |> List.filter (fun sd -> sd.Map )
  |> List.map (fun sd -> sd.Publish
                         |> List.map (fun p -> getPropertyForLabel p.Uri p.Label, sprintf "%s%s" config.SchemaBase sd.Schema))
  |> List.concat

let private getRdfTermMap termList =
  termList
  |> List.map (fun t -> (fst t, vocabLookup(snd t)))
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

let private getRdfArgs (config:compiler.ConfigTypes.Config) () =
  let getTermListMap = getRdfTerms >> getRdfTermMap
  { VocabMap = rdf_getVocabMap config
    TermMap = getTermListMap config
    BaseUrl = sprintf "%s%s" config.UrlBase config.ThingBase
  }

let createConfig jsonString = 
  let deserialisedConfig = JsonConvert.DeserializeObject<compiler.ConfigTypes.Config>(jsonString)

  let getPropertySetFromConfig = getPropertySet deserialisedConfig

  { BaseUrl = sprintf "%s%s" deserialisedConfig.UrlBase deserialisedConfig.ThingBase
    PropertyBaseUrl = sprintf "%s%s" deserialisedConfig.UrlBase deserialisedConfig.QSBase
    SchemaBase = deserialisedConfig.SchemaBase
    JsonLdContexts = getPropertySetFromConfig getJsonLd
    Ttls = getPropertySetFromConfig getTtl
    PropPaths = getPropPaths deserialisedConfig
    AnnotationConfig = getAnnotationConfig deserialisedConfig
    RdfTerms = getRdfTerms deserialisedConfig
    LoadRdfArgs = getRdfArgs deserialisedConfig
    TypeName = deserialisedConfig.TypeName
    IndexName = deserialisedConfig.IndexName
  }

