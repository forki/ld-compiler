module compiler.ValidationUtils

open System.Text.RegularExpressions
open compiler.Domain
open compiler.Utils
open compiler.ConfigTypes
open compiler.AnnotationUtils

let private raiseError label guid state =
  match state with
  | "Invalid" -> sprintf "Invalid value for the '%s (%s)' annotation" label guid
  | "Blank" -> sprintf "No value provided for the '%s (%s)' annotation" label guid
  | "Missing" -> sprintf "Missing the '%s (%s)' annotation" label guid
  | _ -> sprintf "Error (%s) encountered while processing the '%s (%s)' annotation" state label guid
  |> failwith


let private doesAnnotationWithTermExist vocab term (theseAnnotations:Annotation list) =
  theseAnnotations
  |> List.filter (fun a -> a.Property = vocab)
  |> List.map (fun a -> a.Terms |> List.filter (fun t -> t = term))
  |> List.concat
  |> fun al -> match al.Length with
               | 0 -> false
               | _ -> true

let verifyRequiredAnnotationsExist theseValidations theseAnnotations =
  let validationHasFormat thisValidation =
    match obj.ReferenceEquals(thisValidation.Format, null) with
    | false -> thisValidation, true
    | _ -> thisValidation, false

  let isConditionalReqired validationParts = 
    let condVocab = Array.get validationParts 2
    let condTerm = Array.get validationParts 3
    doesAnnotationWithTermExist condVocab condTerm theseAnnotations

  let isValidationRequired thisValidation =
    let validationParts = thisValidation.Format.Split [|':'|]
    match validationParts.Length with
    | 0 
    | 1 -> thisValidation, false
    | _ -> match Array.get validationParts 1 with
           | "Required" -> thisValidation, true
           | "Conditional" -> thisValidation, (isConditionalReqired validationParts)
           | _ -> thisValidation, false

  let isRequiresAnnotationInList thisValidation =
    let foundAnnotations = theseAnnotations |> List.filter (fun a -> a.Vocab = thisValidation.Uri)
    match foundAnnotations.Length with
    | 0 -> raiseError thisValidation.Label thisValidation.Uri "Missing"
    | _ -> match foundAnnotations.Head.Terms.Length with
           | 0 -> raiseError thisValidation.Label thisValidation.Uri "Blank"
           | _ -> ()

  theseValidations
  |> List.map validationHasFormat
  |> List.filter (fun v -> snd v )
  |> List.map (fun v -> v |> fst |> isValidationRequired)
  |> List.filter (fun v -> snd v )
  |> List.map (fun v -> v |> fst |> isRequiresAnnotationInList)
  |> ignore

  theseAnnotations

let private processDates thisAnnotation =
  let tryparseDate date = 
    match System.DateTime.TryParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None) with
    | true, x -> x.ToString("yyyy-MM-dd")
    | _ ->  raiseError thisAnnotation.Property thisAnnotation.Vocab "Invalid"
    
  let processedTerms = thisAnnotation.Terms
                         |> List.map (fun t -> tryparseDate t)
  { thisAnnotation with Terms = processedTerms; IsDate = true}
 
let private processPositionalId thisAnnotation =
  let posnIdError = raiseError thisAnnotation.Property thisAnnotation.Vocab

  let regexPositionalId pid =
    Regex.Match(pid,"^qs[1-9]\\d*-st[1-9]\\d*$")
    |> fun x -> match x.Success with
                | true -> ()
                | _ -> posnIdError "Invalid"
         
  match thisAnnotation.Terms.Length with
  | 1 -> thisAnnotation.Terms.Head |> regexPositionalId
  | _ -> posnIdError "Missing"
    
  thisAnnotation

let private processYesNo thisAnnotation =
  let assessTerm term =
    match term with
    | "yes"
    | "no" -> ()
    | _ -> raiseError thisAnnotation.Property thisAnnotation.Vocab "Invalid"

  thisAnnotation.Terms
  |> List.map (fun t -> assessTerm t)
  |> ignore

  thisAnnotation

let private processStatementReference thisAnnotation =
  let isInvaid = thisAnnotation.Terms
                 |> List.map (fun t -> System.Guid.TryParse(t) |> fst)
                 |> List.contains false
  match isInvaid with
  | true -> raiseError thisAnnotation.Property thisAnnotation.Vocab "Invalid"
  | _ -> thisAnnotation
  
let private validateDataAnnotationFormat (thisAnnotation:Annotation) =
  let validationParts = thisAnnotation.Format.Split [|':'|]   
  match validationParts.Length with
    | 0 -> thisAnnotation
    | _ -> match Array.get validationParts 0 with
           | "Date" -> processDates thisAnnotation
           | "PositionalId" -> processPositionalId thisAnnotation
           | "YesNo" -> processYesNo thisAnnotation
           | "Statement" -> processStatementReference thisAnnotation
           | _ -> thisAnnotation

let private validateDataAnnotation (thisAnnotation:Annotation) =
  match thisAnnotation.Format |> isNullOrWhitespace with
  | true -> thisAnnotation
  | _ -> thisAnnotation |> validateDataAnnotationFormat

let private validateAnnotation thisAnnotation =
  match thisAnnotation.IsDataAnnotation with
  | true -> validateDataAnnotation thisAnnotation
  | _ -> thisAnnotation

let private hasUndiscoverableTerms thisAnnotation =
  thisAnnotation.Terms |> List.contains thisAnnotation.UndiscoverableWhen

let private isAnnotationUndiscoverable (thisAnnotation:Annotation) =
  match obj.ReferenceEquals(thisAnnotation.UndiscoverableWhen, null) with
  | true -> false
  | _ -> match thisAnnotation.UndiscoverableWhen with
         | "" -> false
         | "*Populated*" -> match thisAnnotation.Terms.Length with
                            | 0 -> false
                            | _ -> true
         | _ -> hasUndiscoverableTerms thisAnnotation

let private hasUndiscoverableAnnotations theseAnnotations =
  theseAnnotations
  |> List.map isAnnotationUndiscoverable
  |> List.contains true

let private addIsUndiscoverable thisStatement =
  let isUndiscoverable = thisStatement.Annotations |> hasUndiscoverableAnnotations

  { thisStatement with IsUndiscoverable = isUndiscoverable}

let private isContentSuppressed (thisAnnotation:Annotation) =
  match obj.ReferenceEquals(thisAnnotation.SuppressContent, null) with
  | true -> false
  | _ -> thisAnnotation.SuppressContent 

let private hasSuppressContent theseAnnotations =
  theseAnnotations
  |> List.map isContentSuppressed
  |> List.contains true

let private configureSuppressContent thisStatement =
  let isSuppressContent= thisStatement.Annotations |> hasSuppressContent
  { thisStatement with IsSuppressContent = isSuppressContent  }

let getDisplayFlagFromAnnotations (theseAnnotations:Annotation List) (thisPublishItem:PublishItem) =
  let doesConditionalExist conditionParts = 
    let condVocab = Array.get conditionParts 0
    let condTerm = Array.get conditionParts 1
    doesAnnotationWithTermExist condVocab condTerm 

  let updatedDisplayItem displayFlag = { thisPublishItem.Display with Always = displayFlag }
  let updatedPublishItem displayFlag = { thisPublishItem with Display = updatedDisplayItem displayFlag }

  let conditionParts = thisPublishItem.Display.Condition.Split [|':'|]
  match conditionParts.Length with
  | 2 -> theseAnnotations |> doesConditionalExist conditionParts |> updatedPublishItem
  | _ -> thisPublishItem

let private setDisplayFlagFromCondition (theseAnnotations:Annotation List) (thisPublishItem:PublishItem) =
  match obj.ReferenceEquals(thisPublishItem.Display.Condition, null) with
  | true -> thisPublishItem
  | _ -> getDisplayFlagFromAnnotations theseAnnotations thisPublishItem

let private setDisplayFlag (theseAnnotations:Annotation List) (thisPublishItem:PublishItem) =
  match obj.ReferenceEquals(thisPublishItem.Display, null) with
  | true -> thisPublishItem
  | _ -> match thisPublishItem.Display.Always with
         | true -> thisPublishItem
         | _ -> setDisplayFlagFromCondition theseAnnotations thisPublishItem
  
let private addConditionalDisplayFlag (thisStatement:Statement) (annotationConfig:PublishItem list) =
  annotationConfig
  |> List.map (fun x -> setDisplayFlag thisStatement.Annotations x )

let validateStatement config (thisStatement:Statement) =
  let annotationConfig = config.AnnotationConfig
                         |> addConditionalDisplayFlag thisStatement

  thisStatement.Annotations
  |> verifyRequiredAnnotationsExist annotationConfig
  |> ignore

  let processAnnotations =
    addConfigToAnnotation annotationConfig
    >> addUriToAnnotation config.PropertyBaseUrl
    >> validateAnnotation

  { thisStatement with 
      Id = thisStatement.Id
      Title = thisStatement.Title
      Abstract = thisStatement.Abstract
      StandardId = thisStatement.StandardId
      StatementId = thisStatement.StatementId
      Annotations = thisStatement.Annotations
                      |> List.map processAnnotations 
                      |> List.filter (fun x -> x.Terms.Length > 0)
      Content = thisStatement.Content
      Html = thisStatement.Html
  } |> addIsUndiscoverable |> configureSuppressContent

