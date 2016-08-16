module compiler.OntologyConfig

type PublishItem = {
    Uri: string
    Label: string
    PropertyPath: string list
}

type ConfigItem = {
    Schema: string
    JsonLD: string
    Publish: PublishItem list

}

type OntologyConfig = {
    SchemaBase: string
    UriBase: string
    IndexName: string
    TypeName: string
    SchemaDetails: ConfigItem list
}