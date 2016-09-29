module compiler.Domain

open FSharp.RDF

type Annotation = {
  Property : string
  Vocab : string
  Terms : string list
  IsDisplayed : bool
  IsDate : bool
  IsValidated : bool
  Format : string
  Uri : string
  IsDataAnnotation : bool
  UndiscoverableWhen : string
  DisplayLabel: string
  DisplayTemplate: string
}

type Statement = {
  Id : string
  Title : string
  Abstract : string
  StandardId : int
  StatementId : int
  Annotations : Annotation list
  Content : string
  Html : string
  IsUndiscoverable : bool
}

type RDFArgs = {
  VocabMap : Map<string, Uri>     
  TermMap : Map<string, Map<string, Uri>>
  BaseUrl : string
}

let annotation = {
  Property = ""
  Vocab = ""
  Terms = []
  IsDisplayed = false
  IsDate = false
  IsValidated = false
  Format = null
  Uri = null
  IsDataAnnotation = false
  UndiscoverableWhen = null
  DisplayLabel = null
  DisplayTemplate = null
}

let statement = {
  Id = ""
  Title = ""
  Abstract = ""
  StandardId = 0
  StatementId = 0
  Annotations = []
  Content = ""
  Html = ""
  IsUndiscoverable = false
}
