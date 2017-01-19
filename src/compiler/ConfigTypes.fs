module compiler.ConfigTypes

open FSharp.Data
open FSharp.RDF
open Assertion
open rdf
open compiler.Domain

type DisplayItem =
  {
    Always: bool
    Condition: string
    Label: string
    Template: string
  }

type PublishItem =
  {
    Uri: string
    Label: string
    Validate: bool
    DataAnnotation: bool
    Display: DisplayItem
    Format: string
    PropertyPath: string list
    UndiscoverableWhen : string
    SuppressContent : bool
  }

type ConfigItem =
  {
    Schema: string
    JsonLD: string
    Map: bool
    Publish: PublishItem list
  }

type ConfigFile =
  {
    SchemaBase: string
    QSBase: string
    ThingBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
  }

type Ttl =
  | Uri of string
  | Content of string

type ConfigDetails =
  {
    BaseUrl: string
    PropertyBaseUrl: string
    SchemaBase: string
    JsonLdContexts : string list
    Ttls : string list
    CoreTtl : Ttl
    PropPaths : string list
    DataPropertyPaths : string list
    ObjectPropertyPaths : string list
    AnnotationConfig : PublishItem List
    RdfTerms : (string * string) List
    LoadRdfArgs : unit -> RDFArgs
    TypeName : string
    IndexName : string
  }
  static member pullCoreTtl (x:ConfigDetails) =
    match x.CoreTtl with
    | Uri u -> {x with CoreTtl = Content (Http.RequestString u) }
    | _ -> x

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
  SuppressContent = false
}

let t_configItem = {
  Schema = ""
  JsonLD = ""
  Map = false
  Publish = []
}

let t_configFile = {
  SchemaBase = null
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
  CoreTtl = Uri ""
  Ttls = []
  PropPaths = []
  DataPropertyPaths = []
  ObjectPropertyPaths = []
  AnnotationConfig = []
  RdfTerms = []
  LoadRdfArgs = t_loadRdfArgs
  TypeName = null
  IndexName = null
}
