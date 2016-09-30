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
      Uri= "hasPositionalId"
      Label="PositionalId"
      Validate= true
      Format= "PositionalId:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "wasFirstIssuedOn"
      Label="First issued"
      Validate= true
      Display = { t_displayItem with
                    Always = true
                }
      Format= "Date:Required"
      DataAnnotation = true
  }
  { t_publishItem with
      Uri= "isNationalPriority"
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
    PropertyBaseUrl = "http://ld.nice.org.uk/ns/qualitystandard"
}

let a_positionalid = { annotation with Property = "hasPositionalId"; Vocab = "PositionalId"; Terms = ["qs1-st1"] }
let va_positionalid = { a_positionalid with Format = "PositionalId:Required"; Uri= "hasPositionalId"; IsValidated = true; IsDisplayed = false; IsDataAnnotation = true }
let a_nationalpriority = { annotation with Property = "isNationalPriority"; Vocab = "National priority"; Terms = ["yes"]; }
let va_nationalpriority = { a_nationalpriority with Format = "YesNo:Required"; Uri = "isNationalPriority"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true; UndiscoverableWhen = "no" }
let a_firstissued = { annotation with Property = "wasFirstIssuedOn"; Vocab = "First issued"; Terms = ["01-10-2000"] }
let va_firstissued = { a_firstissued with Format = "Date:Required"; Uri = "wasFirstIssuedOn"; IsValidated= true; IsDisplayed = true; IsDataAnnotation = true }

[<Test>]
let ``ValidationUtilsTests: When the config file data is added to the read annotations that is complete`` () =

  let result = [ a_positionalid; a_nationalpriority; a_firstissued ]
               |> List.map (addConfigToAnnotation config.AnnotationConfig)
 
  areListsTheSame [ va_positionalid; va_nationalpriority; va_firstissued ] result

[<Test>]
let ``ValidationUtilsTests: When the uri is appended with the annotations that is as expected`` () =

  let result = [ va_positionalid; va_nationalpriority; va_firstissued ]
               |> List.map (addUriToAnnotation config.PropertyBaseUrl)
  
  areListsTheSame [ { va_positionalid with Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId" }; { va_nationalpriority with Uri = "http://ld.nice.org.uk/ns/qualitystandard#isNationalPriority" }; { va_firstissued with Uri = "http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn" } ] result
