module compiler.Tests.BindHtmlTests

open NUnit.Framework
open FsUnit
open FSharp.Data
open compiler.ContentHandle
open compiler.Domain
open compiler.BindDataToHtml

[<Test>]
let ``When IsDisplayed is false do not template properties`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = false
                      Vocab = "DontDisplayThis" }   
  ]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should not' (haveSubstring "DontDisplayThis")

[<Test>]
let ``When IsDisplayed is true insert date type annotation correctly into html`` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = true
                      Terms = ["0001-01-01"] }]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement
  let firstIssuedDate = "January 0001"

  result.Html |> should haveSubstring firstIssuedDate

[<Test>]
let ``When IsDisplayed is true insert non-date type annotation correctly into html `` () =
  let defaultAnnotations = [
    { annotation with IsDisplayed = true
                      IsDataAnnotation = true
                      IsDate = false
                      Terms = ["non date value"] }]

  let statement = { statement with Annotations = defaultAnnotations }
  let result = bindDataToHtml statement

  result.Html |> should haveSubstring "non date value"
