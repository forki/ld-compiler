module compiler.Tests.BindHtmlTests

open NUnit.Framework
open FsUnit
open FSharp.Data
open compiler.ContentHandle
open compiler.Domain
open compiler.BindDataToHtml

[<Test>]
let ``Should insert data correctly into html `` () =
  let defaultAnnotations = [
    {Vocab = "First issued"
     Terms = ["0001-01-01"] }   
  ]

  let defaultStatement = {
    Id = "id_goes_here"
    Title = ""
    Abstract = ""
    StandardId = 0
    StatementId = 0
    Annotations = defaultAnnotations
    Content = ""
    Html = ""
  }

  let statement = { defaultStatement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement
  let firstIssuedDate = "January 0001"

  result.Html |> should haveSubstring firstIssuedDate
