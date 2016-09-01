module compiler.Test.ValidationUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.ConfigTypes
open compiler.ValidationUtils
open compiler.Test.TestUtilities

let private annotationValidations = [
  {
    Uri= "positionalid"
    Label=null
    Validate= true
    Format= "PositionalId:Required"
    PropertyPath=[]
  }
  {
    Uri= "required"
    Label=null
    Validate= true
    Format= "String:Required"
    PropertyPath=[]
  }
  {
    Uri= "notrequireddate"
    Label=null
    Validate= true
    Format= "Date"
    PropertyPath=[]
  }
  {
    Uri= "notrequiredyesno"
    Label=null
    Validate= true
    Format= "YesNo"
    PropertyPath=[]
  }
  {
    Uri= "conditionallyrequireddate"
    Label=null
    Validate = true
    Format= "Date:Conditional:notrequiredyesno:no"
    PropertyPath=[]
  }
]

let validRequiredAnnotations = [ { Vocab = "PositionalId"; Terms = ["qs1-st1"] }
                                 { Vocab = "Required"; Terms = ["A value"] } ]

let defaultStatement = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = validRequiredAnnotations
  Content = "Content"
  Html = "Content"
}

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (no conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required Date"; Terms = ["01-10-2010"] }
                                                                               { Vocab = "Not Required YesNo"; Terms = ["yes"] } ] }
  
  let dataTransformed = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required Date"; Terms = ["2010-10-01"] }
                                                                                          { Vocab = "Not Required YesNo"; Terms = ["yes"] } ] }
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame dataTransformed.Annotations resultStatement.Annotations



[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (with conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required Date"; Terms = ["01-10-2010"] }
                                                                               { Vocab = "Not Required YesNo"; Terms = ["no"] }
                                                                               { Vocab = "Conditionally Required Date"; Terms = ["01-08-2016"] } ] }

  
  let dataTransformed = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required Date"; Terms = ["2010-10-01"] }
                                                                                          { Vocab = "Not Required YesNo"; Terms = ["no"] }
                                                                                          { Vocab = "Conditionally Required Date"; Terms = ["2016-08-01"] } ] }
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame dataTransformed.Annotations resultStatement.Annotations
[<Test>]
let ``ValidationUtilsTests: When a statement has an invalid PositionalId then validating the statement will throw an 'invalid annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ { Vocab = "PositionalId"; Terms = ["st1-qs1"] }
                                                    { Vocab = "Required"; Terms = ["A value"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the PositionalId annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has an blank required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ { Vocab = "PositionalId"; Terms = ["qs1-st1"] }
                                                    { Vocab = "Required"; Terms = [] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the required annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement is missing required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ { Vocab = "PositionalId"; Terms = ["qs1-st1"] } ] }
  
  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the required annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a date formatted annotation which is not valid (dd-MM-yyyy) then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required Date"; Terms = ["01 October 2010"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the notrequireddate annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a YesNo formatted annotation which is not yes or no then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = validRequiredAnnotations @ [ { Vocab = "Not Required YesNo"; Terms = ["Some Other Value"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the notrequiredyesno annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a conditionally required annotation which is not provided then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations =validRequiredAnnotations @  [ { Vocab = "Not Required YesNo"; Terms = ["no"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the conditionallyrequireddate annotation"