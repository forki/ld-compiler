module compiler.ConfigUtils

open FSharp.RDF
open Newtonsoft.Json
open compiler.Domain
open compiler.ConfigTypes
open compiler.Utils


let private buildUrl baseUrl path =
  sprintf "%s%s" baseUrl path

// move these to ConfigTypes?????
let private getJsonLd item = item.JsonLD
let private getTtl item = item.Schema

let private getPropertySet (config:ConfigFile) getPropFn  =
  config.SchemaDetails
  |> List.map (getPropFn >> buildUrl config.SchemaBase)

let private getPathWithSubclass qsBase (p:PublishItem) =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s>/%s" qsBase p.Uri pp 
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
    | true ->  sprintf "<%s%s>" config.QSBase p.Uri
    | _ -> getPathWithSubclass config.QSBase p

  config.SchemaDetails
  |> List.map (fun f -> f.Publish 
                        |> List.map (fun p -> buildSchemaDetails p))
  |> List.concat
  |> List.filter (fun f -> f <> "")

let private getAnnotationConfig (config:ConfigFile) =
  config.SchemaDetails
  |> List.map (fun f -> f.Publish |> List.filter(fun p -> p.Validate))
  |> List.concat

let private getPropertyForLabel s label =
    match label |> isNullOrWhitespace with
    |true -> s
    |_ -> getProperty label

let private getId (uri:Uri) =
  uri.ToString().Split('/') |> Array.rev |> Array.head

let private vocabLookup uri =
  let owl_class = Uri.from "http://www.w3.org/2002/07/owl#Class"
  let gcd = Graph.loadFrom uri
  Resource.fromType owl_class gcd
  |> List.map (fun r -> Resource.id r |> getId, r)
  |> Map.ofList

let private getRdfTerms (config:ConfigFile) =
  config.SchemaDetails
  |> List.filter (fun sd -> sd.Map )
  |> List.map (fun sd -> sd.Publish
                         |> List.map (fun p -> p.Uri, sprintf "%s%s" config.SchemaBase sd.Schema))
  |> List.concat

let private getRdfTermMap termList =
  termList
  |> List.map (fun t -> (fst t, vocabLookup(snd t)))
  |> Map.ofList

let private rdf_getVocabMap config =
  let getMmkVocabList p =
    p
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.map (fun p -> (p.Uri, sprintf "%s%s" config.QSBase p.Uri))

  let getVocabList config =
    config.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish |> getMmkVocabList))
    |> List.concat

  getVocabList config
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let private getRdfArgs (config:ConfigFile) () =
  let getTermListMap = getRdfTerms >> getRdfTermMap
  { VocabMap = rdf_getVocabMap config
    TermMap = getTermListMap config
    BaseUrl = config.ThingBase
  }

let getCoreTtlUri config =
  config.SchemaDetails
  |> List.find (fun x -> x.Map = false)
  |> fun x -> Uri (sprintf "%s%s" config.SchemaBase x.Schema)

let createConfig jsonString = 
  let deserialisedConfig = JsonConvert.DeserializeObject<ConfigFile>(jsonString)

  let getPropertySetFromConfig = getPropertySet deserialisedConfig

  { BaseUrl = deserialisedConfig.ThingBase
    PropertyBaseUrl = deserialisedConfig.QSBase
    SchemaBase = deserialisedConfig.SchemaBase
    JsonLdContexts = getPropertySetFromConfig getJsonLd
    CoreTtl = getCoreTtlUri deserialisedConfig
    Ttls = getPropertySetFromConfig getTtl
    PropPaths = getPropPaths deserialisedConfig
    AnnotationConfig = getAnnotationConfig deserialisedConfig
    RdfTerms = getRdfTerms deserialisedConfig
    LoadRdfArgs = getRdfArgs deserialisedConfig
    TypeName = deserialisedConfig.TypeName
    IndexName = deserialisedConfig.IndexName
  }

let updateLabelsFromTtl (config:ConfigDetails) =
  let configWithTtl = config |> ConfigDetails.pullCoreTtl
  let ttlContent = match configWithTtl.CoreTtl with
                   | Content c -> c
                   | _ -> ""
  let graph = Graph.loadTtl (fromString ttlContent)

  let getResource (publishItem:PublishItem) =
    let uri = sprintf "%s%s" config.PropertyBaseUrl publishItem.Uri
    let ret =Resource.fromSubject (Uri.from uri) graph
    publishItem, ret |> List.tryHead
  
  let label x d =
    match x with
    | None -> d
    | Some x -> match (|FunctionalDataProperty|_|)
                       (Uri.from "http://www.w3.org/2000/01/rdf-schema#label")
                       (xsd.string) x with
                | Some x -> x
                | _ -> d

  let updatePublishItem (publishItem, resource) =
    { publishItem with Label = (label resource publishItem.Label) }

  let processItem = getResource >> updatePublishItem  

  { config with AnnotationConfig = config.AnnotationConfig |> List.map processItem }