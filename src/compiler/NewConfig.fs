module compiler.NewConfig

open Newtonsoft.Json

type Config = {
  SchemaBase: string
}

let createConfig jsonString = 
  let deserialisedConfig = JsonConvert.DeserializeObject<ConfigTypes.Config>(jsonString)

  {SchemaBase = deserialisedConfig.SchemaBase}

