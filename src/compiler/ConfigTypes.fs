module compiler.ConfigTypes

type PublishItem = {
    Uri: string
    Label: string
    Validate: bool
    DataAnnotation: bool
    Display: bool
    Format: string
    PropertyPath: string list
}

type ConfigItem = {
    Schema: string
    JsonLD: string
    Map: bool
    Publish: PublishItem list
}

type UndiscoverableItem = {
    UndiscoverableLabel: string
    AnnotationValue: string
}

type Config = {
    SchemaBase: string
    UrlBase: string
    QSBase: string
    ThingBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
    Undiscoverables: UndiscoverableItem list
}