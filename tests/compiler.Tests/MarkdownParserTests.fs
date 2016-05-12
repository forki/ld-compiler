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
let ``Should extract the id from the markdown filepath`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.Id |> should equal "qs1/st2"

[<Test>]
let ``Should extract the title from the markdown filepath`` () =
  let statement = extractStatement sampleMarkdownContent
  
  statement.Title |> should equal "Quality Statement 2 from Quality Standard 1"

[<Test>]
let ``Should extract the abstract from the markdown first paragraph and convert to html`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.Abstract |> should equal "This is the abstract with a Link."

[<Test>]
let ``Should extract the content from whole markdown file`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.Content |> should equal sampleMarkdownContent.Content

[<Test>]
let ``Should extract the standard number from file path`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.StandardId |> should equal 1

[<Test>]
let ``Should extract the statement number from file path`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.StatementId |> should equal 2

[<Test>]
let ``Should extract the annotations from code block`` () =
  let statement = extractStatement sampleMarkdownContent

  statement.Annotations |> should equal [{Vocab = "Vocab"; Terms = ["Term"]}]

