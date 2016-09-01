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
     Terms = ["2010-10-01"] }   
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

  let expectedHtml = """
<table>
<tr>
<td>First Issued On</td><td>October 2010</td>
</tr>
</table>
"""  
  result.Html |> should equal expectedHtml
