module compiler.Test.TestUtilities

open NUnit.Framework
open FsUnit

let areListsTheSame (expected: 'a list) (actual: 'a list) =
  actual.Length |> should equal expected.Length

  expected
  |> List.zip actual
  |> List.iter (fun (expected, actual) -> expected |> should equal actual)
