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
  

[<Test>]
let ``Should create title dataproperty for resource``() =
  let statement = {emptyStatement with Title = "This is the title" }

  let resource = transformToRDF statement

  test <@ [ "This is the title" ] = FindDataProperty "http://ld.nice.org.uk/ns/qualitystandard#title" resource @>

