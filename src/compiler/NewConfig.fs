module compiler.NewConfig

open Newtonsoft.Json
open ConfigTypes

type NewConfig = {
  SchemaBase: string
  JsonLdContexts : string list
  Ttls : string list
}

let private buildUrl baseUrl path =
  sprintf "%s%s" baseUrl path

// move these to ConfigTypes?????
let private getJsonLd item = item.JsonLD
let private getTtl item = item.Schema

let private getPropertySet (config:Config) getPropFn  =
  config.SchemaDetails
  |> List.map (getPropFn >> buildUrl config.SchemaBase)

let createConfig jsonString = 
  let deserialisedConfig = JsonConvert.DeserializeObject<Config>(jsonString)

  let getPropertySetFromConfig = getPropertySet deserialisedConfig

  {SchemaBase = deserialisedConfig.SchemaBase
   JsonLdContexts = getPropertySetFromConfig getJsonLd
   Ttls = getPropertySetFromConfig getTtl}

