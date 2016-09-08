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
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri= "hasRequired"
    Label="Required"
    Validate= true
    Format= "String:Required"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri= "hasNotRequireddate"
    Label="Date Not Required"
    Validate= true
    Format= "Date"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri= "hasNotRequiredYesNo"
    Label="YesNo Not Required"
    Validate= true
    Format= "YesNo"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri= "hasConditionallyRequiredDate"
    Label="Date Conditional"
    Validate = true
    Format= "Date:Conditional:YesNo Not Required:no"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
]

let private configItem = {
  Schema = "qualitystandard.ttl"
  JsonLD = "qualitystandard.jsonld "
  Map = false
  Publish = annotationValidations
}

let private config = {
  SchemaBase = "http://schema/ns/"
  UrlBase = "http://ld.nice.org.uk/"
  QSBase ="ns/qualitystandard"
  ThingBase = "resource"
  IndexName = "kb"
  TypeName = "qualitystatement"
  SchemaDetails = [configItem]
}

let a_positionalId = { annotation with Property = "positionalid"; Vocab = "PositionalId"; Terms = ["qs1-st1"]; Format = "PositionalId:Required"; Uri= "http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId"; IsValidated = true; IsDisplayed = false; IsDataAnnotation = true }
let a_required = { annotation with Property = "required";Vocab = "Required"; Terms = ["A value"]; Format = "String:Required"; Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasRequired"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true }
let a_datenotrequired = { annotation with Property = "datenotrequired"; Vocab = "Date Not Required"; Terms = ["01-10-2010"]; Format = "Date"; Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasNotRequireddate"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true }
let a_yesnonotrequired = { annotation with Property = "yesnonotrequired"; Vocab = "YesNo Not Required"; Terms = ["yes"]; Format = "YesNo"; Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasNotRequiredYesNo"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true  }
let a_dateconditional = { annotation with Property = "dateconditional"; Vocab = "Date Conditional"; Terms = ["01-08-2010"]; Format = "Date:Conditional:YesNo Not Required:no"; Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasConditionallyRequiredDate"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true }

//let validRequiredAnnotations = [ a_positionalId; a_required ]

let defaultStatement = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [a_positionalId; a_required]
  Content = "Content"
  Html = "Content"
}

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (no conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; a_datenotrequired; a_yesnonotrequired ] }
  
  let dataTransformed = { defaultStatement with Annotations = [ a_positionalId; a_required; { a_datenotrequired with Terms = ["2010-10-01"]; IsDate = true }; a_yesnonotrequired ] }
  let resultStatement = validateStatement config data

  areListsTheSame dataTransformed.Annotations resultStatement.Annotations


[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (with conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; a_datenotrequired; { a_yesnonotrequired with Terms = ["no"] }; a_dateconditional ] }

  
  let dataTransformed = {defaultStatement with Annotations = [a_positionalId; a_required; { a_datenotrequired with Terms = ["2010-10-01"]; IsDate = true }; { a_yesnonotrequired with Terms = ["no"] }; { a_dateconditional with Terms = ["2010-08-01"]; IsDate = true} ] }
  let resultStatement = validateStatement config data

  areListsTheSame dataTransformed.Annotations resultStatement.Annotations

[<Test>]
let ``ValidationUtilsTests: When a statement has an invalid PositionalId then validating the statement will throw an 'invalid annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ { a_positionalId with Terms = ["st1-qs1"] }; a_required ] }

  let res = try
              validateStatement config data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'PositionalId' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has an blank required annotation then validating the statement will throw a 'missing annotation' exception`` () =

  let res = try
              verifyRequiredAnnotationsExist annotationValidations [ a_positionalId; { a_required with Terms = [] } ] |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "No value provided for the 'Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement is missing required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              verifyRequiredAnnotationsExist annotationValidations [ a_positionalId ] |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a date formatted annotation which is not valid (dd-MM-yyyy) then validating the statement will throw a 'invalid annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; { a_datenotrequired with Terms = ["01 October 2010"] } ] }

  let res = try
              validateStatement config data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'Date Not Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a YesNo formatted annotation which is not yes or no then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; { a_yesnonotrequired with Terms = ["Some Other Value"] } ] }

  let res = try
              validateStatement config data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'YesNo Not Required' annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a conditionally required annotation which is not provided then validating the statement will throw a 'missing annotation' exception`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; { a_yesnonotrequired with Terms = ["no"] } ] }

  let res = try
              verifyRequiredAnnotationsExist annotationValidations  [ a_positionalId; a_required; { a_yesnonotrequired with Terms = ["no"] } ]  |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Date Conditional' annotation"


