module compiler.Tests.RDFTests

open compiler.Domain
open compiler.ConfigTypes
open compiler.ConfigUtils
open compiler.RDF
open FSharp.RDF
open resource
open NUnit.Framework
open FsUnit


let defaultAnnotations = [
  { Vocab = "PositionalId"
    Terms = ["qs1-st1"] }
  { Vocab = "National priority"
    Terms = ["yes"] }
  { Vocab = "Changed Priority On"
    Terms = ["2012-10-01"] }
  { Vocab = "First issued"
    Terms = ["2010-06-01"] }   
]

let defaultStatement = {
  Id = "id_goes_here"
  Title = ""
  Abstract = ""
  StandardId = 0
  StatementId = 0
  ObjectAnnotations = []
  DataAnnotations = defaultAnnotations
  Content = ""
  Html = ""
}

let private baseUrl = "http://ld.nice.org.uk/ns/qualitystandard"

let private validations = [
  {
    Uri = "hasPositionalId"
    Label = "PositionalId"
    Validate = true
    Format = "PositionalId:Required"
    PropertyPath=[]
  }
  {
    Uri = "isNationalPriority"
    Label = "National priority"
    Validate = true
    Format = "YesNo:Required"
    PropertyPath=[]
  }
  {
    Uri = "changedPriorityOn"
    Label = "Changed Priority On"
    Validate = true
    Format = "Date:Conditional:National priority:no"
    PropertyPath=[]
  }
  {
    Uri = "wasFirstIssuedOn"
    Label = "First issued"
    Validate = true
    Format = "Date:Required"
    PropertyPath=[]
  }
]

let defaultArgs = {
  VocabMap = Map.ofList []
  TermMap = Map.ofList ["", Map.ofList []]
  BaseUrl = baseUrl
}

let private FindDataProperty (uri:string) resource =
  match resource with
   | DataProperty (Uri.from uri) xsd.string values -> values
   | _ -> []

let private FindObjectProperty (uri:string) resource =
  match resource with
   | ObjectProperty (Uri.from uri) values -> values
   | _ -> []


[<Test>]
let ``RDFTests: Should create resource with type of qualitystatement``() =

  defaultStatement
  |> transformToRDF defaultArgs validations baseUrl
  |> FindObjectProperty "rdf:type" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "http://ld.nice.org.uk/ns/qualitystandard#QualityStatement"
  

[<Test>]
let ``RDFTests: Should create a resource with data property wasFirstIssuedOn``() =

  defaultStatement
  |> transformToRDF defaultArgs validations baseUrl
  |> FindDataProperty "http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "2010-06-01"
  
[<Test>]
let ``RDFTests: Should create resource with subject uri as id``() =
  let statement = {defaultStatement with Id = "id_goes_here" }

  let id = statement 
           |> transformToRDF defaultArgs validations baseUrl
           |> Resource.id 
   
  id.ToString() |> should equal "http://ld.nice.org.uk/ns/qualitystandard/id_goes_here"

[<Test>]
let ``RDFTests: Should create title dataproperty for resource``() =
  let statement = {defaultStatement with Title = "This is the title" }

  let title = statement
              |> transformToRDF defaultArgs validations baseUrl
              |> FindDataProperty "http://ld.nice.org.uk/ns/qualitystandard#title" 

  title |> should equal ["This is the title"]

[<Test>]
let ``RDFTests: Should convert a single annotated term into an objectproperty``() =
  let statement = {defaultStatement with ObjectAnnotations = defaultAnnotations @ [{Vocab="Vocab1"; Terms=[ "Term1" ] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"] |> Map.ofList ] |> Map.ofList}
  let property = 
    statement
    |> transformToRDF args validations baseUrl
    |> FindObjectProperty "http://someuri.com/Vocab1" 
    |> Seq.map (fun p -> p.ToString())
    |> Seq.head

  property |> should equal "http://someuri.com/Vocab1#Term1"

[<Test>]
let ``RDFTests: Should convert multiple annotated terms from one vocab as objectproperties``() =
  let statement = {defaultStatement with ObjectAnnotations = defaultAnnotations @ [{Vocab="Vocab 1"; Terms=[ "Term1"; "Term2" ] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"
                                                     "term2", Uri.from "http://someuri.com/Vocab1#Term2"] |> Map.ofList ] |> Map.ofList}
  let properties = 
    statement
    |> transformToRDF args validations baseUrl
    |> FindObjectProperty "http://someuri.com/Vocab1" 
    |> Seq.map (fun p -> p.ToString())
    |> Seq.toList

  properties |> should equal ["http://someuri.com/Vocab1#Term1"
                              "http://someuri.com/Vocab1#Term2"]

[<Test>]
let ``RDFTests: Should convert annotated terms from multiple vocabs as objectproperties``() =
  let statement = {defaultStatement with ObjectAnnotations = defaultAnnotations @ [{Vocab="Vocab1"; Terms=[ "Term1"] }
                                                                                   {Vocab="Vocab2"; Terms=[ "Term1"] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"
                                           "vocab2", Uri.from "http://someuri.com/Vocab2"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"] |> Map.ofList 
                                          "vocab2", ["term1", Uri.from "http://someuri.com/Vocab2#Term1"] |> Map.ofList ] |> Map.ofList}
  let rdf = statement |> transformToRDF args validations baseUrl

  rdf
  |> FindObjectProperty "http://someuri.com/Vocab1" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "http://someuri.com/Vocab1#Term1"

  rdf
  |> FindObjectProperty "http://someuri.com/Vocab2" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "http://someuri.com/Vocab2#Term1"
