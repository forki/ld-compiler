module compiler.ConfigTypes

open FSharp.RDF
open Assertion
open rdf
open compiler.Domain

type DisplayItem = {
    Always: bool
    Condition: string
    Label: string
    Template: string
}

type PublishItem = {
    Uri: string
    Label: string
    Validate: bool
    DataAnnotation: bool
    Display: DisplayItem
    Format: string
    PropertyPath: string list
    UndiscoverableWhen : string
}

type ConfigItem = {
    Schema: string
    JsonLD: string
    Map: bool
    Publish: PublishItem list
}


type ConfigFile = {
    SchemaBase: string
    UrlBase: string
    QSBase: string
    ThingBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
}

type ConfigDetails = {
  BaseUrl: string
  PropertyBaseUrl: string
  SchemaBase: string
  JsonLdContexts : string list
  Ttls : string list
  PropPaths : string list
  AnnotationConfig : PublishItem List
  RdfTerms : (string * string) List
  LoadRdfArgs : unit -> RDFArgs
  TypeName : string
  IndexName : string
}
let t_displayItem = {
    Always = false
    Condition = null
    Label = null
    Template = null
}

let t_publishItem = {
  Uri = null
  Label = null
  Validate = false
  DataAnnotation = false
  Display = t_displayItem
  Format = null
  PropertyPath = []
  UndiscoverableWhen = null
}

let t_configItem = {
  Schema = ""
  JsonLD = ""
  Map = false
  Publish = []
}

let t_configFile = {
  SchemaBase = null
  UrlBase = null
  QSBase = null
  ThingBase = null
  IndexName = null
  TypeName = null
  SchemaDetails = []
}

let t_loadRdfArgs () =
  let v = ["string", Uri.from "http://someresource.com"] |> Map.ofList
  let t = ["string", ["string", resource !! "http://someresource.com" [] ] |> Map.ofList] |> Map.ofList
  { VocabMap = v
    TermMap = t
    BaseUrl = null
  }

let t_configDetails = {
  BaseUrl = null
  PropertyBaseUrl = null
  SchemaBase = null
  JsonLdContexts = []
  Ttls = []
  PropPaths = []
  AnnotationConfig = []
  RdfTerms = []
  LoadRdfArgs = t_loadRdfArgs
  TypeName = null
  IndexName = null
}