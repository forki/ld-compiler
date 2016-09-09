module compiler.Test.FilteringUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.ConfigTypes
open compiler.Test.TestUtilities
open compiler.FilteringUtils

let undiscoverableList = [ { UndiscoverableLabel = "Affects If Discoverable"; AnnotationValue = "no" } ]

let a_discoverable = { annotation with Property = "affectsdiscoverability"; Vocab = "Affects If Discoverable"; Terms = ["yes"]; Format = "YesNo:Required"; Uri= "http://ld.nice.org.uk/ns/qualitystandard#hasThingThatAffectsDiscoverability"; IsValidated = true; IsDisplayed = false; IsDataAnnotation = true }
let a_undiscoverable = { a_discoverable with Terms = ["no"]; }

let s_discoverable = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [a_discoverable]
  Content = "Content"
  Html = "Content"
}
let s_undiscoverable = { s_discoverable with Title = "Quality statement 2 from quality standard 1"; Annotations = [a_undiscoverable]}


[<Test>]
let ``FilteringUtilsTests: An undiscoverable statement should not be discoverable`` () =

  let result = s_undiscoverable
               |> isStatementUndiscoverable undiscoverableList 
               |> snd

  result |> should equal true


[<Test>]
let ``FilteringUtilsTests: A discoverable statement should  be discoverable`` () =

  let result = s_discoverable
               |> isStatementUndiscoverable undiscoverableList 
               |> snd

  result |> should equal false