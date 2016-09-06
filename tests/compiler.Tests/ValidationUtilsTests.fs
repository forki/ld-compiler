module compiler.Test.ValidationUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.ConfigTypes
open compiler.ValidationUtils
open compiler.Test.TestUtilities

let private annotationValidations = [
  {
    Uri= "hasPositionalId"
    Label="PositionalId"
    Validate= true
    Format= "PositionalId:Required"
    PropertyPath=[]
  }
  {
    Uri= "hasRequired"
    Label="Required"
    Validate= true
    Format= "String:Required"
    PropertyPath=[]
  }
  {
    Uri= "hasNotRequireddate"
    Label="Date Not Required"
    Validate= true
    Format= "Date"
    PropertyPath=[]
  }
  {
    Uri= "hasNotRequiredYesNo"
    Label="YesNo Not Required"
    Validate= true
    Format= "YesNo"
    PropertyPath=[]
  }
  {
    Uri= "hasConditionallyRequiredDate"
    Label="Date Conditional"
    Validate = true
    Format= "Date:Conditional:YesNo Not Required:no"
    PropertyPath=[]
  }
]

let validRequiredAnnotations = [ { annotation with Vocab = "PositionalId"; Terms = ["qs1-st1"] }
                                 { annotation with Vocab = "Required"; Terms = ["A value"] } ]

let defaultStatement = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  ObjectAnnotations = []
  DataAnnotations = validRequiredAnnotations
  Content = "Content"
  Html = "Content"
}

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (no conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "Date Not Required"; Terms = ["01-10-2010"] }
                                                                                   { annotation with Vocab = "YesNo Not Required"; Terms = ["yes"] } ] }
  
  let dataTransformed = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "Date Not Required"; Terms = ["2010-10-01"] }
                                                                                              { annotation with Vocab = "YesNo Not Required"; Terms = ["yes"] } ] }
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame dataTransformed.DataAnnotations resultStatement.DataAnnotations


[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (with conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "Date Not Required"; Terms = ["01-10-2010"] }
                                                                                   { annotation with Vocab = "YesNo Not Required"; Terms = ["no"] }
                                                                                   { annotation with Vocab = "Date Conditional"; Terms = ["01-08-2016"] } ] }

  
  let dataTransformed = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "Date Not Required"; Terms = ["2010-10-01"] }
                                                                                              { annotation with Vocab = "YesNo Not Required"; Terms = ["no"] }
                                                                                              { annotation with Vocab = "Date Conditional"; Terms = ["2016-08-01"] } ] }
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame dataTransformed.DataAnnotations resultStatement.DataAnnotations

[<Test>]
let ``ValidationUtilsTests: When a statement has an invalid PositionalId then validating the statement will throw an 'invalid annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = [ { annotation with Vocab = "PositionalId"; Terms = ["st1-qs1"] }
                                                        { annotation with Vocab = "Required"; Terms = ["A value"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'PositionalId' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has an blank required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = [ { annotation with Vocab = "PositionalId"; Terms = ["qs1-st1"] }
                                                        { annotation with Vocab = "Required"; Terms = [] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement is missing required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = [ { annotation with Vocab = "PositionalId"; Terms = ["qs1-st1"] } ] }
  
  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a date formatted annotation which is not valid (dd-MM-yyyy) then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "Date Not Required"; Terms = ["01 October 2010"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'Date Not Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a YesNo formatted annotation which is not yes or no then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = validRequiredAnnotations @ [ { annotation with Vocab = "YesNo Not Required"; Terms = ["Some Other Value"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'YesNo Not Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a conditionally required annotation which is not provided then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with DataAnnotations = validRequiredAnnotations @  [ { annotation with Vocab = "YesNo Not Required"; Terms = ["no"] } ] }

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Date Conditional' annotation"
