module compiler.Domain

type Annotation = {
  Vocab : string
  Terms : string list
}

type Statement = {
  Id : string
  Title : string
  Abstract : string
  StandardId : int
  StatementId : int
  ObjectAnnotations : Annotation list
  DataAnnotations : Annotation list
  Content : string
  Html : string
}


