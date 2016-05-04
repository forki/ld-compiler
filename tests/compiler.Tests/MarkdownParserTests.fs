module compiler.Test.MarkdownTests

open NUnit.Framework
open Swensen.Unquote

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

This is the abstract

This is some content
"""
}

[<Test>]
let ``Should extract the id from the markdown filepath`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.Id = "qs1/st2" @>

[<Test>]
let ``Should extract the title from the markdown filepath`` () =
  let statement = extractStatement sampleMarkdownContent
  
  test <@ statement.Title = "Quality Statement 2 from Quality Standard 1" @>

[<Test>]
let ``Should extract the abstract from the markdown first paragraph`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.Abstract = "This is the abstract" @>

[<Test>]
let ``Should extract the content from whole markdown file`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.Content = sampleMarkdownContent.Content @>

[<Test>]
let ``Should extract the standard number from file path`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.StandardId = 1 @>

[<Test>]
let ``Should extract the statement number from file path`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.StatementId = 2 @>

[<Test>]
let ``Should extract the annotations from code block`` () =
  let statement = extractStatement sampleMarkdownContent

  test <@ statement.Annotations = [{Vocab = "Vocab"; Terms = ["Term"]}] @>

