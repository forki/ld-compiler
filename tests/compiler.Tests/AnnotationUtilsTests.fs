module compiler.Test.AnnotationUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.Utils
open compiler.ConfigTypes
open compiler.AnnotationUtils
open compiler.Test.TestUtilities
open FSharp.Data
open compiler.RDF
open FSharp.RDF

let private annotationValidations = [
  { t_publishItem with
      Uri= "GUID_hasPositionalId"
      Label="PositionalId"
      Validate= true
      Format= "PositionalId:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "GUID_wasFirstIssuedOn"
      Label="First issued"
      Validate= true
      Display = { t_displayItem with
                    Always = true
                }
      Format= "Date:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "GUID_isNationalPriority"
      Label="National priority"
      Validate= true
      Format= "YesNo:Required"
      DataAnnotation = true
      UndiscoverableWhen = "no"
  }
]

let private configItem = {
  Schema = "qualitystandard.ttl"
  JsonLD = "qualitystandard.jsonld "
  Map = false
  Publish = annotationValidations
}

let private config = {
  t_configDetails with
    AnnotationConfig = annotationValidations
    PropertyBaseUrl = "https://nice.org.uk/ontologies/qualitystandard/"
}

let a_positionalid = { annotation with Property = "PositionalId"; Vocab = "GUID_hasPositionalId"; Terms = ["qs1-st1"] }
let va_positionalid = { a_positionalid with Format = "PositionalId:Required"; Uri= "GUID_hasPositionalId"; IsValidated = true; IsDisplayed = false; IsDataAnnotation = true }
let a_nationalpriority = { annotation with Property = "National priority"; Vocab = "GUID_isNationalPriority"; Terms = ["yes"]; }
let va_nationalpriority = { a_nationalpriority with Format = "YesNo:Required"; Uri = "GUID_isNationalPriority"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true; UndiscoverableWhen = "no" }
let a_firstissued = { annotation with Property = "First issued"; Vocab = "GUID_wasFirstIssuedOn"; Terms = ["01-10-2000"] }
let va_firstissued = { a_firstissued with Format = "Date:Required"; Uri = "GUID_wasFirstIssuedOn"; IsValidated= true; IsDisplayed = true; IsDataAnnotation = true }

[<Test>]
let ``AnnotationUtilsTests: When the config file data is added to the read annotations that is complete`` () =

  let result = [ a_positionalid; a_nationalpriority; a_firstissued ]
               |> List.map (addConfigToAnnotation config.AnnotationConfig)
 
  let expected = [ va_positionalid; va_nationalpriority; va_firstissued ] 
  areAnnotationListsTheSame expected result

[<Test>]
let ``AnnotationUtilsTests: When the uri is appended with the annotations that is as expected`` () =

  let result = [ va_positionalid; va_nationalpriority; va_firstissued ]
               |> List.map (addUriToAnnotation config.PropertyBaseUrl)
  let expected = [ { va_positionalid with Uri = "https://nice.org.uk/ontologies/qualitystandard/GUID_hasPositionalId" }
                   { va_nationalpriority with Uri = "https://nice.org.uk/ontologies/qualitystandard/GUID_isNationalPriority" }
                   { va_firstissued with Uri = "https://nice.org.uk/ontologies/qualitystandard/GUID_wasFirstIssuedOn" } ]


  areListsTheSame expected result
