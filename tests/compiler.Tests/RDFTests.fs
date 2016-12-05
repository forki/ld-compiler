module compiler.Tests.RDFTests

open compiler.Domain
open compiler.ConfigTypes
//open compiler.ConfigUtils
open compiler.RDF
open FSharp.RDF
open Assertion
open rdf
open resource
open NUnit.Framework
open FsUnit

let dataAnnotations = [
  { annotation with
      Property = "firstissued"
      Vocab = "First issued"
      Terms = ["2010-10-01"]
      IsDisplayed = true
      IsDate = true
      IsValidated = true
      IsDataAnnotation = true
      Format = "Date:Required"
      Uri = "https://nice.org.uk/ontologies/qualitystandard#wasFirstIssuedOn"
  }   
]

let defaultStatement = { statement with Id = "id_goes_here" }

let private baseUrl = "https://nice.org.uk/ontologies/qualitystandard"

let defaultArgs = {
  VocabMap = Map.ofList []
  TermMap = Map.ofList ["", Map.ofList []]
  BaseUrl = baseUrl
}

let private FindDataProperty (uri:string) resource =
  match resource with
   | DataProperty (Uri.from uri) FSharp.RDF.xsd.string values -> values
   | _ -> []

let private FindObjectProperty (uri:string) resource =
  match resource with
   | ObjectProperty (Uri.from uri) values -> values
   | _ -> []

let createResourceWithLabel (uri:string) (label:string) =
  let subject = sprintf "%s#%s" uri label
  resource !! subject [ dataProperty !! "http://www.w3.org/2000/01/rdf-schema#label" (label^^xsd.string)]

[<Test>]
let ``RDFTests: Should create resource with type of qualitystatement``() =

  defaultStatement
  |> transformToRDF defaultArgs
  |> FindObjectProperty "rdf:type" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "https://nice.org.uk/ontologies/qualitystandard/e29accb1_afde_4130_bb06_2d2c7bf990db"
  

[<Test>]
let ``RDFTests: Should create a resource with data property wasFirstIssuedOn``() =

  { defaultStatement with Annotations = dataAnnotations }
  |> transformToRDF defaultArgs
  |> FindDataProperty "https://nice.org.uk/ontologies/qualitystandard#wasFirstIssuedOn" 
  |> Seq.map (fun p -> p.ToString())
  |> Seq.head
  |> should equal "2010-10-01"
  
[<Test>]
let ``RDFTests: Should create resource with subject uri as id``() =
  let statement = {defaultStatement with Id = "id_goes_here" }

  let id = statement 
           |> transformToRDF defaultArgs
           |> Resource.id 
   
  id.ToString() |> should equal "https://nice.org.uk/ontologies/qualitystandard/id_goes_here"

[<Test>]
let ``RDFTests: Should create title dataproperty for resource``() =
  let statement = {defaultStatement with Title = "This is the title" }

  let title = statement
              |> transformToRDF defaultArgs
              |> FindDataProperty "https://nice.org.uk/ontologies/qualitystandard/bc8e0db0_5d8a_4100_98f6_774ac0eb1758" 

  title |> should equal ["This is the title"]

[<Test>]
let ``RDFTests: Should convert a single annotated term into an objectproperty``() =
  let vocabUri = "http://someuri.com/Vocab1"
  let statement = {defaultStatement with Annotations = [{ annotation with Vocab="Vocab1"; Terms=[ "Term1" ] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from vocabUri] |> Map.ofList
                               TermMap = ["vocab1", ["term1", createResourceWithLabel vocabUri "Term1"] |> Map.ofList ] |> Map.ofList}
  let property = 
    statement
    |> transformToRDF args
    |> FindObjectProperty "http://someuri.com/Vocab1" 
    |> Seq.map (fun p -> p.ToString())
    |> Seq.head

  property |> should equal "http://someuri.com/Vocab1#Term1"

[<Test>]
let ``RDFTests: Should convert multiple annotated terms from one vocab as objectproperties``() =
  let vocabUri = "http://someuri.com/Vocab1"

  let statement = {defaultStatement with Annotations = [{ annotation with Vocab="Vocab 1"; Terms=[ "Term1"; "Term2" ] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from vocabUri] |> Map.ofList
                               TermMap = ["vocab1", ["term1", createResourceWithLabel vocabUri "Term1"
                                                     "term2", createResourceWithLabel vocabUri "Term2" ] |> Map.ofList ] |> Map.ofList}
  let properties = 
    statement
    |> transformToRDF args
    |> FindObjectProperty "http://someuri.com/Vocab1" 
    |> Seq.map (fun p -> p.ToString())
    |> Seq.toList

  properties |> should equal ["http://someuri.com/Vocab1#Term1"
                              "http://someuri.com/Vocab1#Term2"]

[<Test>]
let ``RDFTests: Should convert annotated terms from multiple vocabs as objectproperties``() =
  let vocab1Uri = "http://someuri.com/Vocab1"
  let vocab2Uri = "http://someuri.com/Vocab2"
  let statement = {defaultStatement with Annotations = [{ annotation with Vocab="Vocab1"; Terms=[ "Term1"] }
                                                        { annotation with Vocab="Vocab2"; Terms=[ "Term1"] }]}
  let args = {defaultArgs with VocabMap = ["vocab1", Uri.from vocab1Uri
                                           "vocab2", Uri.from vocab2Uri] |> Map.ofList
                               TermMap = ["vocab1", ["term1", createResourceWithLabel vocab1Uri "Term1" ] |> Map.ofList 
                                          "vocab2", ["term1", createResourceWithLabel vocab2Uri "Term1" ] |> Map.ofList ] |> Map.ofList}
  let rdf = statement |> transformToRDF args

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
