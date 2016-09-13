module compiler.ConfigTypes

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