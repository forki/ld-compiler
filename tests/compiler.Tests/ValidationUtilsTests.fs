module compiler.Test.ValidationUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.ConfigTypes
open compiler.ValidationUtils
open compiler.Test.TestUtilities

let private annotationValidations = [
  { t_publishItem with
      Uri= "GUID_postionalId"
      Label="PositionalId"
      Validate= true
      Format= "PositionalId:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "GUID_hasRequired"
      Label="Required"
      Validate= true
      Format= "String:Required"
      DataAnnotation = true
      Display = { t_displayItem with Always = true }
  }
  { t_publishItem with
      Uri= "GUID_datenotrequired"
      Label="Date Not Required"
      Validate= true
      Format= "Date"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "GUID_yesnonotrequired"
      Label="YesNo Not Required"
      Validate= true
      Format= "YesNo"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "GUID_conditionallyrequireddate"
      Label="Date Conditional"
      Validate = true
      Format= "Date:Conditional:GUID_yesnonotrequired:no"
      DataAnnotation = true
      Display = { t_displayItem with
                    Condition = "GUID_yesnonotrequired:no"
                    Label = "Priority"
                    Template = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level."
      }
  }
  { t_publishItem with
      Uri= "GUID_affectsdiscoverability"
      Label="Affects If Discoverable"
      Validate = true
      Format= "YesNo"
      DataAnnotation = true
      UndiscoverableWhen = "no"
  }
  { t_publishItem with
      Uri= "GUID_affectsdiscoverabilityifpopulated"
      Label="Being Populated Affects If Discoverable"
      Validate = true
      DataAnnotation = true
      UndiscoverableWhen = "*Populated*"
  }
  { t_publishItem with
      Uri= "GUID_statementref"
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
    PropertyBaseUrl = "http://ld.nice.org.uk/ns/qualitystandard/"
}

// these are the annotations before munging with the 

let a_positionalId = { annotation with 
                        Property = "GUID_postionalId"
                        Vocab = "GUID_postionalId"
                        Terms = ["qs1-st1"]
                        }

let a_required = { annotation with 
                        Property = "GUID_hasRequired"
                        Vocab = "GUID_hasRequired"
                        Terms = ["A value"] }

let a_datenotrequired = { annotation with 
                            Property = "GUID_datenotrequired"
                            Vocab = "GUID_datenotrequired"
                            Terms = ["01-10-2010"] }

let a_yesnonotrequired = { annotation with 
                            Property = "GUID_yesnonotrequired"
                            Vocab = "GUID_yesnonotrequired" 
                            Terms = ["yes"] }

let a_dateconditional = { annotation with
                            Property = "GUID_conditionallyrequireddate"
                            Vocab = "GUID_conditionallyrequireddate"
                            Terms = ["01-08-2010"] }

let a_statementReference = { annotation with 
                              Property = "GUID_statementref"
                              Vocab = "GUID_statementref"
                              Terms = ["8422158b-302e-4be2-9a19-9085fc09dfe7"] }

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

//let annotation = {
//  Property = ""
//  Vocab = ""
//  Terms = []
//  IsDisplayed = false
//  IsDate = false
//  IsValidated = false
//  Format = null
//  Uri = null
//  IsDataAnnotation = false
//  UndiscoverableWhen = null
//  DisplayLabel = null
//  DisplayTemplate = null
//}

let prepend (annotation:Annotation) =
  sprintf "%s%s" config.PropertyBaseUrl annotation.Uri

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (no conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; a_datenotrequired; a_yesnonotrequired ] }
  
  let dataTransformed = { defaultStatement with Annotations = [ { a_positionalId with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_postionalId"
                                                                                      Property="PositionalId"
                                                                                      IsValidated= true
                                                                                      Format = "PositionalId:Required"
                                                                                      IsDataAnnotation = true }
                                                                { a_required with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_hasRequired"
                                                                                  Property="Required"
                                                                                  IsValidated= true
                                                                                  Format= "String:Required"
                                                                                  IsDataAnnotation = true
                                                                                  IsDisplayed = true}
                                                                { a_datenotrequired with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_datenotrequired"
                                                                                         Property="Date Not Required"
                                                                                         IsValidated= true
                                                                                         Format= "Date"
                                                                                         IsDataAnnotation = true
                                                                                         Terms = ["2010-10-01"]; IsDate = true }
                                                                { a_yesnonotrequired with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_yesnonotrequired"
                                                                                          Property="YesNo Not Required"
                                                                                          IsValidated= true
                                                                                          Format= "YesNo"
                                                                                          IsDataAnnotation = true } ] }
  let resultStatement = validateStatement config data

  areAnnotationListsTheSame dataTransformed.Annotations resultStatement.Annotations


[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (with conditionally required) then validating the statement will return a statement that is identical but with processed dates`` () =
  let data = {defaultStatement with Annotations = [ a_positionalId; a_required; a_datenotrequired; { a_yesnonotrequired with Terms = ["no"] }; a_dateconditional ] }

  let dataTransformed = { defaultStatement with Annotations = [ { a_positionalId with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_postionalId"
                                                                                      Property="PositionalId"
                                                                                      IsValidated= true
                                                                                      Format = "PositionalId:Required"
                                                                                      IsDataAnnotation = true }
                                                                { a_required with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_hasRequired"
                                                                                  Property="Required"
                                                                                  IsValidated= true
                                                                                  Format= "String:Required"
                                                                                  IsDataAnnotation = true
                                                                                  IsDisplayed = true }
                                                                { a_datenotrequired with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_datenotrequired"
                                                                                         Property="Date Not Required"
                                                                                         IsValidated= true
                                                                                         Format= "Date"
                                                                                         IsDataAnnotation = true
                                                                                         Terms = ["2010-10-01"]; IsDate = true }
                                                                { a_yesnonotrequired with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_yesnonotrequired"
                                                                                          Property="YesNo Not Required"
                                                                                          IsValidated= true
                                                                                          Format= "YesNo"
                                                                                          Terms = ["no"]
                                                                                          IsDataAnnotation = true } 
                                                                { a_dateconditional with Uri= "http://ld.nice.org.uk/ns/qualitystandard/GUID_conditionallyrequireddate"
                                                                                         Property="Date Conditional"
                                                                                         IsValidated = true
                                                                                         Format= "Date:Conditional:GUID_yesnonotrequired:no"
                                                                                         IsDataAnnotation = true
                                                                                         Terms = ["2010-08-01"]
                                                                                         IsDate = true
                                                                                         IsDisplayed = true
                                                                                         DisplayLabel = "Priority"
                                                                                         DisplayTemplate = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level." } ] }



  let resultStatement = validateStatement config data

  areAnnotationListsTheSame dataTransformed.Annotations resultStatement.Annotations

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
                                    Property = "GUID_affectsdiscoverability"
                                    Vocab = "GUID_affectsdiscoverability"
                                    Terms = ["yes"] }

let a_conditionalundiscoverable = { a_conditionaldiscoverable with Terms = ["no"]; }

let a_undiscoverablewhenpopulated_notpopulated = { annotation with 
                                                    Property = "GUID_affectsdiscoverabilityifpopulated"
                                                    Vocab = "GUID_affectsdiscoverabilityifpopulated"
                                                    Terms = [] }

let a_undiscoverablewhenpopulated_populated = { a_undiscoverablewhenpopulated_notpopulated with Terms = ["A Value"] }

[<Test>]
let ``ValidationUtilsTests: An conditionally discoverable statement should be discoverable when the condition is not met`` () =

  let result = { defaultStatement with Annotations = [ a_positionalId; a_required; a_conditionaldiscoverable ] }
               |> validateStatement config

  result.IsUndiscoverable |> should equal false

[<Test>]
let ``ValidationUtilsTests: A conditionally discoverable statement should be undiscoverable when the condition is met`` () =

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
