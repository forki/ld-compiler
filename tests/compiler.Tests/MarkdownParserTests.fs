module compiler.Test.MarkdownTests

open NUnit.Framework
open FsUnit

open compiler
open compiler.ContentHandle
open compiler.Domain
open compiler.MarkdownParser
open compiler.ConfigTypes

let nl:string = System.Environment.NewLine
let content = """
```
84efb231_0424_461e_9598_1ef5272a597a:
  - "qs1-st2"
Vocab:
  - "Term"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract with a [Link](http://somelinkhere.com).

This is some content
"""
let sampleMarkdownContent = {
//  Path = "qs2/st2/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.md"
  Thing = "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  Content = content.Replace(nl,"\n")
}

let contentNoPositionId = """
```
Vocab:
  - "Term"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract with a [Link](http://somelinkhere.com).

This is some content
"""
let sampleMarkdownWithoutPositionalId = {
  Thing = "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  Content = contentNoPositionId.Replace(nl,"\n")
}

//DEFUNCT TEST
//[<Test>]
//let ``When markdown doesn't contain a PositionalId the extraction should return an empty string`` () =
//  let statement = extractStatement (sampleMarkdownWithoutPositionalId, "")
//
//  statement.Id |> should equal ""

[<Test>]
let ``Should extract the id from the markdown filename`` () =
  let statement = extractStatement (sampleMarkdownContent, "")

  statement.Id |> should equal "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"

[<Test>]
let ``Should build the title from the positionalid field in metadata`` () =
  let statement = extractStatement (sampleMarkdownContent, "")
  
  statement.Title |> should equal "Quality statement 2 from quality standard 1"

[<Test>]
let ``Should extract the abstract from the converted html and remove anchoring the links but keep text`` () =
  let html = """<p>This is the abstract with a <a href="http://somewhere.com">Link</a>.</p>"""
  let statement = extractStatement (sampleMarkdownContent, html)
  
  statement.Abstract.Replace(nl, "\n") |> should equal "<p>\n  This is the abstract with a Link.\n</p>"

[<Test>]
let ``Should extract the content from whole markdown file`` () =
  let statement = extractStatement (sampleMarkdownContent, "")

  statement.Content |> should equal sampleMarkdownContent.Content

[<Test>]
let ``Should extract the standard number from file path`` () =
  let statement = extractStatement (sampleMarkdownContent, "")

  statement.StandardId |> should equal 1

[<Test>]
let ``Should extract the statement number from file path`` () =
  let statement = extractStatement (sampleMarkdownContent, "")

  statement.StatementId |> should equal 2

[<Test>]
let ``Should extract the annotations from code block`` () =
  let statement = extractStatement (sampleMarkdownContent, "")
  printfn "%A" (statement.Annotations |> List.filter (fun a -> a.IsDataAnnotation))
  let resultAnnotations = [ { annotation with Property = "84efb231_0424_461e_9598_1ef5272a597a"; Vocab = "84efb231_0424_461e_9598_1ef5272a597a"; Terms = ["qs1-st2"] }
                            { annotation with Property = "vocab"; Vocab = "Vocab"; Terms = ["Term"] } ]
      
  statement.Annotations |> should equal resultAnnotations

[<Test>]
let ``removeAnchors should remove multiple anchor tags on one line`` () =
  let html = "<a one>one</a> <a two>two</a>"
  removeAnchors html |> should equal "one two"
