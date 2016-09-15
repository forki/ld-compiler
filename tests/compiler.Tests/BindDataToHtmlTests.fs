module compiler.Tests.BindHtmlTests

open NUnit.Framework
open FsUnit
open FSharp.Data
open compiler.ContentHandle
open compiler.Domain
open compiler.BindDataToHtml

[<Test>]
let ``BindHtmlTests: When IsDisplayed is false do not template properties`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = false
                      Vocab = "DontDisplayThis" }   
  ]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should not' (haveSubstring "DontDisplayThis")

[<Test>]
let ``BindHtmlTests: When IsDisplayed is true insert date type annotation correctly into html`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = true
                      Terms = ["0001-01-01"]
                      Uri = "dateitem" }]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement
  let firstIssuedDate = "January 0001"

  result.Html |> should haveSubstring firstIssuedDate

[<Test>]
let ``BindHtmlTests: When IsDisplayed is true insert non-date type annotation correctly into html `` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = false
                      Terms = ["non date value"]
                      Uri = "nondateitem" }]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should haveSubstring "non date value"

[<Test>]
let ``BindHtmlTests: When IsDisplayed is true and a template is provided for an annotation it is correctly into html`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = true
                      Terms = ["0001-01-01"]
                      Uri = "dateitem"
                      DisplayTemplate = "This is the date {{value |  date: \"MMMM yyyy\" }} put into a string" } ]
  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should haveSubstring "This is the date January 0001 put into a string"
  
[<Test>]
let ``BindHtmlTests: When IsDisplayed is true and a display label is provided for an annotation it is correctly into html`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = true
                      Terms = ["0001-01-01"]
                      Uri = "dateitem"
                      Vocab = "Date Item"
                      DisplayLabel = "A completely different label" } ]
  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should haveSubstring "A completely different label"
  result.Html |> should not' (haveSubstring "Date Item")

[<Test>]
let ``BindHtmlTests: When IsDisplay is true and there are multiple terms in the annotation all are rendered to the HTML`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = false
                      Terms = ["Term1"; "Term2"]
                      Uri = "multipleterms" } ]
  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement
  
  result.Html |> should haveSubstring "Term1"
  result.Html |> should haveSubstring "Term2"
