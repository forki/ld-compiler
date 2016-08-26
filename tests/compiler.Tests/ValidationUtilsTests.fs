module compiler.Test.AnnotationValidationTests

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
    Required= true
    Format= "PositionalId"
    OutFormatMask=null
    PropertyPath=[]
  }
  {
    Uri= "required"
    Label=null
    Required= true
    Format= null
    OutFormatMask= null
    PropertyPath=[]
  }
  {
    Uri= "notrequireddate"
    Label=null
    Required= false
    Format= "Date"
    OutFormatMask= "MMMM yyyy"
    PropertyPath=[]
  }

]

//type Annotation = {
//  Vocab : string
//  Terms : string list
//}
//
//type Statement = {
//  Id : string
//  Title : string
//  Abstract : string
//  StandardId : int
//  StatementId : int
//  Annotations : Annotation list
//  Content : string
//  Html : string
//}

let private validStatement = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private validStatement_WithDate = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required Date"; Terms = ["01-10-2010"] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private validStatement_WithDate_Transformed = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required Date"; Terms = ["October 2010"] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private invalidStatemen_BadPositionalId = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["st1-qs1"] }
    {Vocab = "Required"; Terms = ["A value"] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private invalidStatemen_RequiredBlank = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = [] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private invalidStatemen_RequiredMissing = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Not Required Date"; Terms = ["01-10-2010"] }
  ]
  Content = "Content"
  Html = "HTML"
}

let private invalidStatemen_BadDate = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required Date"; Terms = ["01 October 2010"] }
  ]
  Content = "Content"
  Html = "HTML"
}

[<Test>]
let ``AnnotationValidation: When all statement annotations are valid (no dates) then validating the statement will return an identical statement`` () =
  let resultStatement = validateStatement annotationValidations validStatement

  areListsTheSame validStatement.Annotations resultStatement.Annotations

[<Test>]
let ``AnnotationValidation: When all statement annotations are valid (with dates) then validating the statement will return a statement identical but with processed dates`` () =
  let resultStatement = validateStatement annotationValidations validStatement_WithDate

  areListsTheSame validStatement_WithDate_Transformed.Annotations resultStatement.Annotations

[<Test>]
let ``AnnotationValidation: When a statement has an invalid PositionalId then validating the statement will throw an 'invalid annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatemen_BadPositionalId |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "[Validation Error] Invalid value for the PositionalId annotation"

[<Test>]
let ``AnnotationValidation: When a statement has an blank required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatemen_RequiredBlank |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "[Validation Error] Missing the required annotation"

[<Test>]
let ``AnnotationValidation: When a statement is missing required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatemen_RequiredMissing |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "[Validation Error] Missing the required annotation"

[<Test>]
let ``AnnotationValidation: When a statement has a date formatted annotation which is not valid (dd-MM-yyyy) then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatemen_BadDate |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "[Validation Error] Invalid value for the notrequireddate annotation"



  