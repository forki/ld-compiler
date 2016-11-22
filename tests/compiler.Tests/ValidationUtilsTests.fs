﻿module compiler.Test.ValidationUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.ConfigTypes
open compiler.ValidationUtils
open compiler.Test.TestUtilities

let private annotationValidations = [
  { t_publishItem with
      Uri= "hasPositionalId"
      Label="PositionalId"
      Validate= true
      Format= "PositionalId:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "hasRequired"
      Label="Required"
      Validate= true
      Format= "String:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "hasNotRequireddate"
      Label="Date Not Required"
      Validate= true
      Format= "Date"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "hasNotRequiredYesNo"
      Label="YesNo Not Required"
      Validate= true
      Format= "YesNo"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "hasConditionallyRequiredDate"
      Label="Date Conditional"
      Validate = true
      Format= "Date:Conditional:YesNo Not Required:no"
      DataAnnotation = true
      Display = { t_displayItem with
                    Condition = "YesNo Not Required:no"
                    Label = "Priority"
                    Template = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level."
      }
  }
  { t_publishItem with
      Uri= "affectsdiscoverability"
      Label="Affects If Discoverable"
      Validate = true
      Format= "YesNo"
      DataAnnotation = true
      UndiscoverableWhen = "no"
  }
  { t_publishItem with
      Uri= "affectsdiscoverabilityifpopulated"
      Label="Being Populated Affects If Discoverable"
      Validate = true
      DataAnnotation = true
      UndiscoverableWhen = "*Populated*"
  }
  { t_publishItem with
      Uri= "statementref"
      Label="Reference to A Statement"
      Validate = true
      Format= "Statement"
      DataAnnotation = true
  }
]

let private configItem = {
  Schema = "qualitystandard.ttl"
  JsonLD = "qualitystandard.jsonld "
  Map = false
  Publish = annotationValidations
}

//let private config = {
//  t_config with
//    SchemaBase = "http://schema/ns/"
//    UrlBase = "http://ld.nice.org.uk/"
//    QSBase ="ns/qualitystandard"
//    ThingBase = "resource"
//    IndexName = "kb"
//    TypeName = "qualitystatement"
//    SchemaDetails = [configItem]
//}
let private config = {
  t_configDetails with
    AnnotationConfig = annotationValidations
    PropertyBaseUrl = "http://ld.nice.org.uk/ns/qualitystandard"
}

let a_positionalId = { annotation with 
                        Property = "positionalid"; 
                        Vocab = "PositionalId"; 
                        Terms = ["qs1-st1"]; 
                        Format = "PositionalId:Required"; 
                        Uri= "http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId"; 
                        IsValidated = true; 
                        IsDisplayed = false; 
                        IsDataAnnotation = true }

let a_required = { annotation with 
                        Property = "required";
                        Vocab = "Required"; 
                        Terms = ["A value"]; 
                        Format = "String:Required"; 
                        Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasRequired"; 
                        IsValidated= true; 
                        IsDisplayed = false; 
                        IsDataAnnotation = true }

let a_datenotrequired = { annotation with 
                            Property = "datenotrequired"; 
                            Vocab = "Date Not Required"; 
                            Terms = ["01-10-2010"]; 
                            Format = "Date"; 
                            Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasNotRequireddate"; 
                            IsValidated= true; 
                            IsDisplayed = false; 
                            IsDataAnnotation = true }

let a_yesnonotrequired = { annotation with 
                            Property = "yesnonotrequired"; 
                            Vocab = "YesNo Not Required"; 
                            Terms = ["yes"]; 
                            Format = "YesNo"; 
                            Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasNotRequiredYesNo"; 
                            IsValidated= true; 
                            IsDisplayed = false; 
                            IsDataAnnotation = true  }

let a_dateconditional = { annotation with
                            Property = "dateconditional"
                            Vocab = "Date Conditional"
                            Terms = ["01-08-2010"]
                            Format = "Date:Conditional:YesNo Not Required:no"
                            Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasConditionallyRequiredDate"
                            IsValidated= true
                            IsDisplayed = false
                            IsDataAnnotation = true
                            DisplayLabel = "Priority"
                            DisplayTemplate = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level."
                          }

let a_statementReference = { annotation with 
                              Property = "statementref"; 
                              Vocab = "Reference to A Statement"; 
                              Terms = ["8422158b-302e-4be2-9a19-9085fc09dfe7"]; 
                              Format = "Statement"; 
                              Uri = "http://ld.nice.org.uk/ns/qualitystandard#statementref"; 
                              IsValidated= true; 
                              IsDisplayed = false; 
                              IsDataAnnotation = true }

let defaultStatement = {
  statement with
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

  
  let dataTransformed = {defaultStatement with Annotations = [a_positionalId; a_required; { a_datenotrequired with Terms = ["2010-10-01"]; IsDate = true }; { a_yesnonotrequired with Terms = ["no"] }; { a_dateconditional with Terms = ["2010-08-01"]; IsDate = true; IsDisplayed = true} ] }
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
  let data =  [ a_positionalId; a_required; { a_yesnonotrequired with Terms = ["no"] } ]
  let res = try
              verifyRequiredAnnotationsExist annotationValidations data  |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the 'Date Conditional' annotation"

[<Test>]
let ``ValidationUtilsTests: Given an annotation has an invalid GUID should throw an exception`` () =
  let data = { defaultStatement with Annotations = [ a_positionalId; a_required; { a_statementReference with Terms = ["Clearly an invalid Guid"]} ] }

  let res = try
              validateStatement config data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the 'Reference to A Statement' annotation"









let a_conditionaldiscoverable = { annotation with 
                                    Property = "affectsdiscoverability"; 
                                    Vocab = "Affects If Discoverable"; 
                                    Terms = ["yes"]; 
                                    Format = "YesNo"; 
                                    Uri= "http://ld.nice.org.uk/ns/qualitystandard#hasThingThatAffectsDiscoverability"; 
                                    IsValidated = true; 
                                    IsDisplayed = false; 
                                    IsDataAnnotation = true }

let a_conditionalundiscoverable = { a_conditionaldiscoverable with Terms = ["no"]; }

let a_undiscoverablewhenpopulated_notpopulated = { annotation with 
                                                    Property = "affectsdiscoverabilityifpopulated"; 
                                                    Vocab = "Being Populated Affects If Discoverable"; 
                                                    Terms = []; 
                                                    Uri= "http://ld.nice.org.uk/ns/qualitystandard#affectsdiscoverabilityifpopulated"; 
                                                    IsValidated = true; 
                                                    IsDisplayed = true; 
                                                    IsDataAnnotation = true }

let a_undiscoverablewhenpopulated_populated = { a_undiscoverablewhenpopulated_notpopulated with Terms = ["A Value"] }

[<Test>]
let ``ValidationUtilsTests: An conditionally discoverable statement should be discoverable when the condition is not met`` () =

  let result = { defaultStatement with Annotations = [ a_positionalId; a_required; a_conditionaldiscoverable ] }
               |> validateStatement config

  result.IsUndiscoverable |> should equal false

[<Test>]
let ``ValidationUtilsTests: An conditionally discoverable statement should be undiscoverable when the condition is met`` () =

  let result = { defaultStatement with Annotations = [ a_positionalId; a_required; a_conditionalundiscoverable ] }
               |> validateStatement config

  result.IsUndiscoverable |> should equal true

[<Test>]
let ``ValidationUtilsTests: An undiscoverable when populated - unpopulated - statement should be discoverable`` () =

  let result = { defaultStatement with Annotations = [ a_positionalId; a_required; a_undiscoverablewhenpopulated_notpopulated ] }
               |> validateStatement config

  result.IsUndiscoverable |> should equal false
  
[<Test>]
let ``ValidationUtilsTests: An undiscoverable when populated - populated - statement should be undiscoverable`` () =

  let result = { defaultStatement with Annotations = [ a_positionalId; a_required; a_undiscoverablewhenpopulated_populated ] }
               |> validateStatement config

  result.IsUndiscoverable |> should equal true
