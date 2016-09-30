module compiler.NewConfig

open Newtonsoft.Json
open ConfigTypes

type NewConfig = {
  SchemaBase: string
  JsonLdContexts : string list
  Ttls : string list
  PropPaths : string list
}

let private buildUrl baseUrl path =
  sprintf "%s%s" baseUrl path

// move these to ConfigTypes?????
let private getJsonLd item = item.JsonLD
let private getTtl item = item.Schema

let private getPropertySet config getPropFn  =
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


let createConfig jsonString = 
  let deserialisedConfig = JsonConvert.DeserializeObject<ConfigFile>(jsonString)

  let getPropertySetFromConfig = getPropertySet deserialisedConfig

  {SchemaBase = deserialisedConfig.SchemaBase
   JsonLdContexts = getPropertySetFromConfig getJsonLd
   Ttls = getPropertySetFromConfig getTtl
   PropPaths = getPropPaths deserialisedConfig
   }

