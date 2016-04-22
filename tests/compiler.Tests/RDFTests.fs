module publish.Tests.RDFTests

open publish.Domain
open publish.RDF
open FSharp.RDF
open resource
open NUnit.Framework
open Swensen.Unquote

let emptyStatement = {
  Id = "id_goes_here"
  Title = ""
  Abstract = ""
  StandardId = 0
  StatementId = 0
  Annotations = []
  Content = ""
}

let private FindDataProperty (uri:string) resource =
  match resource with
   | DataProperty (Uri.from uri) xsd.string values -> values
   | _ -> []

let private baseUrl = "http://ld.nice.org.uk/qualitystatement" 
  
[<Test>]
let ``Should create resource with subject uri as id``() =
  let statement = {emptyStatement with Id = "id_goes_here" }

  let resource = transformToRDF baseUrl statement

  test <@ match resource with
          | Is (Uri.from "http://ld.nice.org.uk/qualitystatement/id_goes_here") -> true
          | _ -> false @>

[<Test>]
let ``Should create title dataproperty for resource``() =
  let statement = {emptyStatement with Title = "This is the title" }

  let resource = transformToRDF baseUrl statement

  test <@ [ "This is the title" ] = FindDataProperty "http://ld.nice.org.uk/ns/qualitystandard#title" resource @>

