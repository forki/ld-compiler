module compiler.Domain

type Annotation = {
  Property : string
  Vocab : string
  Terms : string list
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
}


let annotation = {
  Property = ""
  Vocab = ""
  Terms = []
}
