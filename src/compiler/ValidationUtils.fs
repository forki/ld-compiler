module compiler.ValidationUtils

open compiler.Domain
open compiler.ConfigUtils
open compiler.ConfigTypes

let private raiseError annotation state =
  match state with
  | "Invalid" -> sprintf "Invalid value for the '%s' annotation" annotation
  | "Blank" -> sprintf "No value provided for the '%s' annotation" annotation
  | "Missing" -> sprintf "Missing the '%s' annotation" annotation
  | _ -> sprintf "Error (%s) encountered while processing the '%s' annotation" state annotation
  |> failwith


let private doesAnnotationWithTermExist vocab term theseAnnotations =
  theseAnnotations
  |> List.filter (fun a -> a.Vocab = vocab)
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
    let foundAnnotations = theseAnnotations |> List.filter (fun a -> a.Vocab = thisValidation.Label)
    match foundAnnotations.Length with
    | 0 -> raiseError thisValidation.Label "Missing"
    | _ -> match foundAnnotations.Head.Terms.Length with
           | 0 -> raiseError thisValidation.Label "Blank"
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
    | _ ->  raiseError thisAnnotation.Vocab "Invalid"
    
  let processedTerms = thisAnnotation.Terms
                         |> List.map (fun t -> tryparseDate t)
  { thisAnnotation with Terms = processedTerms; IsDate = true}
 
let private processPositionalId thisAnnotation =
  let posnIdError = raiseError "PositionalId"

  let valid prefix (part:string) =
    let compare = sprintf "%s%s" prefix (part.Replace(prefix,""))
    System.String.Equals(compare, part)

  let validateParts qs st =
    match (valid "qs" qs) && (valid "st" st) with
    | true -> ()
    | _ -> posnIdError "Invalid"

  let splitAndProcessPositionalId (positionalId:string) =
    let idParts = positionalId.Split [|'-'|] |> Array.toList

    match idParts.Length with
    | 2 -> validateParts (idParts |> List.head) (idParts |> List.tail |> List.head)
    | _ -> posnIdError "Invalid"
         
  match thisAnnotation.Terms.Length with
  | 1 -> thisAnnotation.Terms.Head |> splitAndProcessPositionalId
  | _ -> posnIdError "Invalid"
    
  thisAnnotation

let private processYesNo thisAnnotation =
  let assessTerm term =
    match term with
    | "yes"
    | "no" -> ()
    | _ -> raiseError thisAnnotation.Vocab "Invalid"

  thisAnnotation.Terms
  |> List.map (fun t -> assessTerm t)
  |> ignore

  thisAnnotation

let private validateDataAnnotation (thisAnnotation:Annotation) =
  let validationParts = thisAnnotation.Format.Split [|':'|]   
  match validationParts.Length with
    | 0 -> thisAnnotation
    | _ -> match Array.get validationParts 0 with
           | "Date" -> processDates thisAnnotation
           | "PositionalId" -> processPositionalId thisAnnotation
           | "YesNo" -> processYesNo thisAnnotation
           | _ -> thisAnnotation

let private validateAnnotation (thisAnnotation:Annotation) =
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
         | _ -> hasUndiscoverableTerms thisAnnotation

let private hasUndiscoverableAnnotations theseAnnotations =
  theseAnnotations
  |> List.map isAnnotationUndiscoverable
  |> List.contains true

let private addIsUndiscoverable thisStatement =
  let isUndiscoverable = thisStatement.Annotations |> hasUndiscoverableAnnotations
  { thisStatement with IsUndiscoverable = isUndiscoverable}


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

let validateStatement (config:Config) (thisStatement:Statement) =
  let propertyBaseUrl = config |> getPropertyBaseUrl
  let annotationConfig = config
                         |> getAnnotationConfig
                         |> addConditionalDisplayFlag thisStatement

  thisStatement.Annotations
  |> verifyRequiredAnnotationsExist annotationConfig
  |> ignore

  { thisStatement with 
      Id = thisStatement.Id
      Title = thisStatement.Title
      Abstract = thisStatement.Abstract
      StandardId = thisStatement.StandardId
      StatementId = thisStatement.StatementId
      Annotations = thisStatement.Annotations
                      |> List.map (addConfigToAnnotation annotationConfig)
                      |> List.map (addUriToAnnotation propertyBaseUrl)
                      |> List.map validateAnnotation
                      |> List.filter (fun x -> x.Terms.Length > 0)
      Content = thisStatement.Content
      Html = thisStatement.Html
  } |> addIsUndiscoverable

