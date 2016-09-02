module compiler.Domain

type Annotation = {
  Property : string
  Vocab : string
  Terms : string list
  IsDisplayed : bool
  IsDate : bool
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
  IsDisplayed = false
  IsDate = false
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
}
