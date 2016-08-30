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
    OutFormatMask= "yyyy-MM-dd"
    PropertyPath=[]
  }
  {
    Uri= "notrequiredyesno"
    Label=null
    Required= false
    Format= "YesNo"
    OutFormatMask= null
    PropertyPath=[]
  }
  {
    Uri= "conditionallyrequireddate"
    Label=null
    Required= false
    Format= "Conditional:Date:notrequiredyesno:no"
    OutFormatMask= null
    PropertyPath=[]
  }
]






let private invalidStatement_RequiredBlank = {
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

let private invalidStatement_RequiredMissing = {
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

let private invalidStatement_BadDate = {
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

let private invalidStatement_BadYesNo = {
  Id = System.Guid.NewGuid().ToString()
  Title = "Quality statement 1 from quality standard 1"
  Abstract = "Abstract"
  StandardId = 1
  StatementId = 1
  Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required YesNo"; Terms = ["Some Other Value"] }
  ]
  Content = "Content"
  Html = "HTML"
}

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (no dates) then validating the statement will return an identical statement`` () =
  let data = {
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
  
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame data.Annotations resultStatement.Annotations

[<Test>]
let ``ValidationUtilsTests: When all statement annotations are valid (with dates) then validating the statement will return a statement identical but with processed dates`` () =
  let data = {
    Id = System.Guid.NewGuid().ToString()
    Title = "Quality statement 1 from quality standard 1"
    Abstract = "Abstract"
    StandardId = 1
    StatementId = 1
    Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required Date"; Terms = ["01-10-2010"] }
    {Vocab = "Not Required YesNo"; Terms = ["no"] }
    {Vocab = "Conditionally Required Date"; Terms = ["01-08-2016"]}
    ]
    Content = "Content"
    Html = "HTML"
  }
  
  let dataTransformed = {
    Id = System.Guid.NewGuid().ToString()
    Title = "Quality statement 1 from quality standard 1"
    Abstract = "Abstract"
    StandardId = 1
    StatementId = 1
    Annotations = [
    {Vocab = "PositionalId"; Terms = ["qs1-st1"] }
    {Vocab = "Required"; Terms = ["A value"] }
    {Vocab = "Not Required Date"; Terms = ["2010-10-01"] }
    {Vocab = "Not Required YesNo"; Terms = ["no"] }
    {Vocab = "Conditionally Required Date"; Terms = ["2016-08-01"]}
    ]
    Content = "Content"
    Html = "HTML"
  }
  let resultStatement = validateStatement annotationValidations data

  areListsTheSame dataTransformed.Annotations resultStatement.Annotations

[<Test>]
let ``ValidationUtilsTests: When a statement has an invalid PositionalId then validating the statement will throw an 'invalid annotation' exception`` () =
  let data = {
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

  let res = try
              validateStatement annotationValidations data |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the PositionalId annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has an blank required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatement_RequiredBlank |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the required annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement is missing required annotation then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatement_RequiredMissing |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Missing the required annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a date formatted annotation which is not valid (dd-MM-yyyy) then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatement_BadDate |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the notrequireddate annotation"

[<Test>]
let ``ValidationUtilsTests: When a statement has a YesNo formatted annotation which is not yes or no then validating the statement will throw a 'missing annotation' exception`` () =
  let res = try
              validateStatement annotationValidations invalidStatement_BadYesNo |> ignore
              "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "Invalid value for the notrequiredyesno annotation"



  