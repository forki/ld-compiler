module compiler.Test.MarkdownTests

open NUnit.Framework
open FsUnit

open compiler
open compiler.ContentHandle
open compiler.Domain
open compiler.Markdown

let sampleMarkdownContent = {
  Path = "qualitystandards/qs1/st2/Statement.md"
  Content = """
```
PositionalId:
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
}

let sampleMarkdownWithoutPositionalId = {
  Path = "qualitystandards/qs1/st2/Statement.md"
  Content = """
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
}

[<Test>]
let ``Should not extract the id from the markdown and give back an empty string`` () =
  let statement = extractStatement (sampleMarkdownWithoutPositionalId, "")

  statement.Id |> should equal ""

[<Test>]
let ``Should extract the id from the markdown filepath`` () =
  let statement = extractStatement (sampleMarkdownContent, "")

  statement.Id |> should equal "qs1/st2"

[<Test>]
let ``Should extract the title from the markdown filepath`` () =
  let statement = extractStatement (sampleMarkdownContent, "")
  
  statement.Title |> should equal "Quality Statement 2 from Quality Standard 1"

[<Test>]
let ``Should extract the abstract from the converted html and remove anchoring the links but keep text`` () =
  let html = """<p>This is the abstract with a <a href="http://somewhere.com">Link</a>.</p>"""
  let statement = extractStatement (sampleMarkdownContent, html)

  statement.Abstract |> should equal "<p>\n  This is the abstract with a Link.\n</p>"

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
  printfn "%A" statement.Annotations

  statement.Annotations |> should equal [{Vocab = "PositionalId";
                                          Terms = ["qs1-st2"];}; {Vocab = "Vocab";
                                                                  Terms = ["Term"];}]

[<Test>]
let ``removeAnchors should remove multiple anchor tags on one line`` () =
  let html = "<a one>one</a> <a two>two</a>"
  removeAnchors html |> should equal "one two"
