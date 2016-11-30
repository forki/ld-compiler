module compiler.Test.TestUtilities

open NUnit.Framework
open FsUnit
open compiler.Domain

let areBothTheSame (e:Annotation) (r:Annotation) =
  r.DisplayLabel |> should equal e.DisplayLabel
  r.DisplayTemplate |> should equal e.DisplayTemplate
  r.Format |> should equal e.Format
  r.IsDataAnnotation |> should equal e.IsDataAnnotation
  r.IsDate |> should equal e.IsDate
  r.IsDisplayed |> should equal e.IsDisplayed
  r.IsValidated |> should equal e.IsValidated
  r.Property |> should equal e.Property
  r.Terms |> should equal e.Terms
  r.UndiscoverableWhen |> should equal e.UndiscoverableWhen
  r.Uri |> should equal e.Uri
  r.Vocab |> should equal e.Vocab

let rec areAnnotationListsTheSame (e:Annotation list) (r:Annotation list) =
  match e with
  | [] -> ()
  | x::[] -> areBothTheSame e.Head r.Head
  | x::xs -> areBothTheSame e.Head r.Head
             areAnnotationListsTheSame e.Tail r.Tail

let areListsTheSame (expected: 'a list) (actual: 'a list) =
  actual.Length |> should equal expected.Length

  expected
  |> List.zip actual
  |> List.iter (fun (expected, actual) -> expected |> should equal actual)
