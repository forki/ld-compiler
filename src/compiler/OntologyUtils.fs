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
  match File.Exists file with
  | true -> File.ReadAllText(file, Encoding.UTF8 )
  | _ -> ""

let deserializeConfig jsonString =
  JsonConvert.DeserializeObject<OntologyConfig>(jsonString)

let getJsonLdContexts config =
  config.SchemaDetails
  |> List.map (fun f -> (sprintf "%s%s" config.SchemaBase f.JsonLD))

let getSchemaTtls config =
  config.SchemaDetails
  |> List.map (fun f -> (sprintf "%s%s" config.SchemaBase f.Schema))

let private getPathWithSubclass urlBase qsBase p =
  let delimiter = "|"
  let buildPropertyPathUri pp = sprintf "<%s%s#%s>/%s" urlBase qsBase p.Uri pp 
  let concatPropertyPaths acc prop = match acc with
                                     | "" -> prop
                                     | _ -> sprintf "%s%s%s" acc delimiter prop
  p.PropertyPath 
  |> List.map buildPropertyPathUri
  |> List.fold concatPropertyPaths ""  

let getPropPaths config =
  let buildSchemaDetails p = match obj.ReferenceEquals(p.PropertyPath, null) with
                             | true ->  sprintf "<%s%s#%s>" config.UrlBase config.QSBase p.Uri
                             | _ -> getPathWithSubclass config.UrlBase config.QSBase p
  config.SchemaDetails
  |> List.map (fun f -> (f.Publish |> List.map (fun p -> buildSchemaDetails p)))
  |> List.concat

let private getGetMmKey s (l:string) =
    match obj.ReferenceEquals(l, null) with
    |true -> s
    |_ -> l.ToLower().Replace(" ","")

let rdf_getVocabMap config =
  let getMmkVocabList p =
    p
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s#%s" config.UrlBase config.QSBase p.Uri))

  let getVocabList config =
    config.SchemaDetails
    |> List.filter (fun x -> x.Map)
    |> List.map (fun f -> (f.Publish |> getMmkVocabList))
    |> List.concat

  getVocabList config
    |> List.map (fun p -> (fst p, Uri.from(snd p)))
    |> Map.ofList

let rdf_getTermMap config =
  let getTermPublishList pl schema = 
    pl
    |> List.filter (fun p -> obj.ReferenceEquals(p.PropertyPath, null)=false)
    |> List.filter (fun p -> p.PropertyPath.Length > 0)
    |> List.map (fun p -> (getGetMmKey p.Uri p.Label, sprintf "%s%s" config.SchemaBase schema)) 
 
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

let getRdfArgs config =
  {
    BaseUrl = getBaseUrl config
    VocabMap = rdf_getVocabMap config
    TermMap = rdf_getTermMap config
  }

let getAnnotationValidations config =
  config.SchemaDetails
  |> List.filter (fun x -> x.Map=false)
  |> List.map (fun f -> (f.Publish |> List.filter (fun p -> p.Required=true)))
  |> List.concat

