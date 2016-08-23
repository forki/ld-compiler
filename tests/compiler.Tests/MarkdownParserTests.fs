module compiler.Test.MarkdownTests

open NUnit.Framework
open FsUnit

open compiler
open compiler.ContentHandle
open compiler.Domain
open compiler.Markdown
open compiler.OntologyConfig

let nl:string = System.Environment.NewLine
let content = """
```
PositionalId:
  - "qs1-st2"
First Issued:
  - "01-10-2010"
NotRequired:
  - "01-11-2000"
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

let contentMandatoryMissing = """
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
let sampleMarkdownMandatoryMissing = {
//  Path = "qs2/st2/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.md"
  Thing = "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  Content = contentMandatoryMissing.Replace(nl,"\n")
}

let contentMandatoryEmpty = """
```
PositionalId:
First Issued:
Vocab:
  - "Term"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract with a [Link](http://somelinkhere.com).

This is some content
"""
let sampleMarkdownMandatoryEmpty = {
//  Path = "qs2/st2/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.md"
  Thing = "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  Content = contentMandatoryEmpty.Replace(nl,"\n")
}

let contentMandatoryInvalid = """
```
PositionalId:
  - "st2-qs1"
First Issued:
  - "Invalid"
NotRequired:
  - "Invalid"
Vocab:
  - "Term"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract with a [Link](http://somelinkhere.com).

This is some content
"""
let sampleMarkdownMandatoryInvalid = {
//  Path = "qs2/st2/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.md"
  Thing = "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  Content = contentMandatoryInvalid.Replace(nl,"\n")
}

let private annotationValidations = [
  {
    Uri= "positionalid"
    Label=null
    Required= true
    Format= "PositionalId"
    OutFormatMask=null
    PropertyPath=[]
  }
  {
    Uri= "firstissued"
    Label=null
    Required= true
    Format= "Date"
    OutFormatMask= "MMMM yyyy"
    PropertyPath=[]
  }
  {
    Uri= "notrequired"
    Label=null
    Required= false
    Format= "Date"
    OutFormatMask= "MMMM yyyy"
    PropertyPath=[]
  }
]

[<Test>]
let ``When markdown doesn't contain a PositionalId the extraction should return zero Standard and Statement Ids`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownMandatoryMissing, "")

  statement.StandardId |> should equal 0
  statement.StatementId |> should equal 0

[<Test>]
let ``When markdown does not contain a required annotation it is not presented in the extracted annotations but any provided annotations are`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownMandatoryMissing, "")

  statement.Annotations |> List.filter (fun a -> a.Vocab = "PositionalId") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "First Issued") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "Vocab") |> List.length |> should equal 1

[<Test>]
let ``When markdown does not contain values for required annotation it is not presented in the extracted annotations but any provided annotations are`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownMandatoryEmpty, "")

  statement.Annotations |> List.filter (fun a -> a.Vocab = "PositionalId") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "First Issued") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "Vocab") |> List.length |> should equal 1

[<Test>]
let ``When markdown contain bad values for annotations it is not presented in the extracted annotations but any provided annotations are`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownMandatoryInvalid, "")

  statement.Annotations |> List.filter (fun a -> a.Vocab = "PositionalId") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "First Issued") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "NotRequired") |> List.length |> should equal 0
  statement.Annotations |> List.filter (fun a -> a.Vocab = "Vocab") |> List.length |> should equal 1

[<Test>]
let ``Should extract the id from the markdown filename`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")

  statement.Id |> should equal "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"

[<Test>]
let ``Should build the title from the positionalid field in metadata`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")
  
  statement.Title |> should equal "Quality statement 2 from quality standard 1"

[<Test>]
let ``Should extract the abstract from the converted html and remove anchoring the links but keep text`` () =
  let html = """<p>This is the abstract with a <a href="http://somewhere.com">Link</a>.</p>"""
  let statement = extractStatement annotationValidations (sampleMarkdownContent, html)
  
  statement.Abstract.Replace(nl, "\n") |> should equal "<p>\n  This is the abstract with a Link.\n</p>"

[<Test>]
let ``Should extract the content from whole markdown file`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")

  statement.Content |> should equal sampleMarkdownContent.Content

[<Test>]
let ``Should extract the standard number from file path`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")

  statement.StandardId |> should equal 1

[<Test>]
let ``Should extract the statement number from file path`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")

  statement.StatementId |> should equal 2

[<Test>]
let ``Should extract the annotations from code block`` () =
  let statement = extractStatement annotationValidations (sampleMarkdownContent, "")
  printfn "%A" statement.Annotations

  statement.Annotations |> should equal [
                                          {Vocab = "PositionalId"; Terms = ["qs1-st2"];};
                                          {Vocab = "First Issued"; Terms = ["October 2010"];};
                                          {Vocab = "NotRequired"; Terms = ["November 2000"];};
                                          {Vocab = "Vocab"; Terms = ["Term"];}
                                        ]

[<Test>]
let ``removeAnchors should remove multiple anchor tags on one line`` () =
  let html = "<a one>one</a> <a two>two</a>"
  removeAnchors html |> should equal "one two"