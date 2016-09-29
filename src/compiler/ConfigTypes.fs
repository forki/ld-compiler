module compiler.ConfigTypes

open compiler.Domain
open FSharp.RDF

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

type NewConfig = {
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

type Config = {
    SchemaBase: string
    UrlBase: string
    QSBase: string
    ThingBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
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

let t_config = {
  SchemaBase = null
  UrlBase = null
  QSBase = null
  ThingBase = null
  IndexName = null
  TypeName = null
  SchemaDetails = []
}

//type RDFArgs = {
//  VocabMap : Map<string, Uri>     
//  TermMap : Map<string, Map<string, Uri>>
//  BaseUrl : string
//}
let t_loadRdfArgs () =
  let v = ["string", Uri.from "Uri"] |> Map.ofList
  let t = ["string", v] |> Map.ofList
  { VocabMap = v
    TermMap = t
    BaseUrl = null
  }

let t_newconfig = {
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