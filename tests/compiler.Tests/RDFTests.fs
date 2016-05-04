module compiler.Tests.RDFTests

open compiler.Domain
open compiler.RDF
open FSharp.RDF
open resource
open NUnit.Framework
open Swensen.Unquote

let defaultStatement = {
  Id = "id_goes_here"
  Title = ""
  Abstract = ""
  StandardId = 0
  StatementId = 0
  Annotations = []
  Content = ""
}

let defaultArgs = {
  VocabMap = Map.ofList []
  TermMap = Map.ofList ["", Map.ofList []]
  BaseUrl = ""
}

let private FindDataProperty (uri:string) resource =
  match resource with
   | DataProperty (Uri.from uri) xsd.string values -> values
   | _ -> []

let private FindObjectProperty (uri:string) resource =
  match resource with
   | ObjectProperty (Uri.from uri) values -> values
   | _ -> []

let private baseUrl = "http://ld.nice.org.uk/qualitystatement" 

[<Test>]
let ``Should create resource with type of qualitystatement``() =
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"}

  let properties = 
    defaultStatement
    |> transformToRDF args
    |> FindObjectProperty "rdf:type" 

  test <@ properties = [Uri.from "http://ld.nice.org.uk/ns/qualitystandard#QualityStatement"] @>
  
[<Test>]
let ``Should create resource with subject uri as id``() =
  let statement = {defaultStatement with Id = "id_goes_here" }
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"}

  let id = statement 
           |> transformToRDF args 
           |> Resource.id 
   
  test <@ id = Uri.from "http://ld.nice.org.uk/qualitystatement/id_goes_here" @>

[<Test>]
let ``Should create title dataproperty for resource``() =
  let statement = {defaultStatement with Title = "This is the title" }
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"}

  let title = statement
              |> transformToRDF args
              |> FindDataProperty "http://ld.nice.org.uk/ns/qualitystandard#title" 

  test <@ title = ["This is the title"] @>

[<Test>]
let ``Should convert a single annotated term into an objectproperty``() =
  let statement = {defaultStatement with Annotations = [{Vocab="Vocab1"; Terms=[ "Term1" ] }]}
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"
                               VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"] |> Map.ofList ] |> Map.ofList}
  let properties = 
    statement
    |> transformToRDF args
    |> FindObjectProperty "http://someuri.com/Vocab1" 

  test <@ properties = [Uri.from "http://someuri.com/Vocab1#Term1"] @>

[<Test>]
let ``Should convert multiple annotated terms from one vocab as objectproperties``() =
  let statement = {defaultStatement with Annotations = [{Vocab="Vocab1"; Terms=[ "Term1"; "Term2" ] }]}
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"
                               VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"
                                                     "term2", Uri.from "http://someuri.com/Vocab1#Term2"] |> Map.ofList ] |> Map.ofList}
  let properties = 
    statement
    |> transformToRDF args
    |> FindObjectProperty "http://someuri.com/Vocab1" 

  test <@ properties = [Uri.from "http://someuri.com/Vocab1#Term1"
                        Uri.from "http://someuri.com/Vocab1#Term2"] @>

[<Test>]
let ``Should convert annotated terms from multiple vocabs as objectproperties``() =
  let statement = {defaultStatement with Annotations = [{Vocab="Vocab1"; Terms=[ "Term1"] }
                                                        {Vocab="Vocab2"; Terms=[ "Term1"] }]}
  let args = {defaultArgs with BaseUrl = "http://ld.nice.org.uk/qualitystatement"
                               VocabMap = ["vocab1", Uri.from "http://someuri.com/Vocab1"
                                           "vocab2", Uri.from "http://someuri.com/Vocab2"] |> Map.ofList
                               TermMap = ["vocab1", ["term1", Uri.from "http://someuri.com/Vocab1#Term1"] |> Map.ofList 
                                          "vocab2", ["term1", Uri.from "http://someuri.com/Vocab2#Term1"] |> Map.ofList ] |> Map.ofList}
  let rdf = statement |> transformToRDF args

  test <@ FindObjectProperty "http://someuri.com/Vocab1" rdf = [Uri.from "http://someuri.com/Vocab1#Term1"] @>
  test <@ FindObjectProperty "http://someuri.com/Vocab2" rdf = [Uri.from "http://someuri.com/Vocab2#Term1"] @>
