module compiler.ConfigTypes

type PublishItem = {
    Uri: string
    Label: string
    Validate: bool
    DataAnnotation: bool
    Display: bool
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

type Config = {
    SchemaBase: string
    UrlBase: string
    QSBase: string
    ThingBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
}

let t_publishItem = {
  Uri = null
  Label = null
  Validate = false
  DataAnnotation = false
  Display = false
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