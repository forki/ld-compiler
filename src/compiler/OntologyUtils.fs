module compiler.OntologyUtils

open System
open System.IO
open FSharp.Data
open compiler.OntologyConfig
open Newtonsoft.Json

//let GetConfigFromFile =
//  let file = sprintf "%s\\OntologyConfig.json" __SOURCE_DIRECTORY__
//  //let file = sprintf "%s/OntologyConfig.json" 
//  if File.Exists file then
//    "Hurrah!! I have found the file. And there was much rejoicing"
//  else
//    "Nope"

let GetConfig jsonString =
  let ret = JsonConvert.DeserializeObject<OntologyConfig>(jsonString)
  ret