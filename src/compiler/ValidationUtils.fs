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

let private processDate name field =
  let validateDate (date:string) (raiseError:string -> string) =
    if (obj.ReferenceEquals(date, null)=false && date.Length > 0) then
      match System.DateTime.TryParseExact(date, "dd-MM-yyyy", System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None) with
      | true, x -> x.ToString("yyyy-MM-dd")
      | _ ->  raiseError "Invalid"
    else
      raiseError "Missing"
  let raiseDateError = raiseError name

  validateDate field raiseDateError

let private validatePositionalId (posnId:string) =
  let posnIdError = raiseError "PositionalId"

  let valid (prefix:string) (part:string) =
    let compare = sprintf "%s%s" prefix (part.Replace(prefix,""))
    System.String.Equals(compare, part)

  let validateParts qs st =
    match (valid "qs" qs) && (valid "st" st) with
    | true -> sprintf "%s-%s" qs st
    | _ -> posnIdError "Invalid"

  let idParts = posnId.Split [|'-'|] |> Array.toList

  match idParts.Length with
  | 2 -> validateParts (idParts |> List.head) (idParts |> List.tail |> List.head)
  | _ -> posnIdError "Invalid"

let private validateYesNo name value =
  match value with
  | "yes"
  | "no" -> value
  | _ -> raiseError name "Invalid"

let private validateValue validation (value:string) =
  let matchValidationType validation format value =
    match format with
    | "Date" -> processDate validation.Label value
    | "PositionalId" -> validatePositionalId value
    | "YesNo" -> validateYesNo validation.Label value
    | _ -> value

  let matchValidationHead value (formatOption:string Option) =
      match formatOption.IsNone with
      | true -> value
      | false -> matchValidationType validation formatOption.Value value

  match obj.ReferenceEquals(validation.Format, null) with
  | true -> value
  | _ ->  validation.Format.Split [|':'|] |> Array.tryHead |> matchValidationHead value

let private validateMandatoryAnnotations validations annotations = 
  let validateAnnotationExists annotations mandatoryValidation =

    let a = annotations |> List.filter (fun a -> a.Vocab = mandatoryValidation.Label)
      
    match a.Length with
    | 0 -> raiseError mandatoryValidation.Label "Missing"
    | _ -> match a.Head.Vocab.Length with
           | 0 -> raiseError mandatoryValidation.Label "Blank"
           | _ -> ()
 
  let assessAnnotations (validationParts:string array) =
    let assessTerms annotationTerms thisTerm =
      annotationTerms |> List.filter (fun t -> t = thisTerm)
                      |> List.length > 0

    let annotationVocab = Array.get validationParts 2
    let annotationTerm = Array.get validationParts 3
    annotations |> List.filter (fun a -> a.Vocab = annotationVocab)
                |> List.map (fun a -> assessTerms a.Terms annotationTerm)
                |> List.contains true
 
  let shouldProcessValidation validation =
    let validationParts = validation.Format.Split [|':'|]
    match validationParts.Length with
    | 2 -> match Array.get validationParts 1 with
           | "Required" -> validation, true
           | _ -> validation, false
    | 4 -> match Array.get validationParts 1 with
           | "Conditional" -> validation, (assessAnnotations validationParts)
           | _ -> validation, false
    | _ -> validation, false

  let activeValidation validation =
    match obj.ReferenceEquals(validation.Format, null) with
    | false -> shouldProcessValidation validation
    | _ -> validation, false

  validations |> List.map (fun v -> activeValidation v)
              |> List.filter (fun t -> snd t)
              |> List.map (fun t -> fst t)
              |> List.map (fun v -> validateAnnotationExists annotations v)
              |> ignore
  annotations

let private validateProvidedAnnotations validations annotations =
  let validateAnnotation validations (thisAnnotation:Annotation) =
    let relevantValidation = validations |> List.filter (fun v -> v.Label = thisAnnotation.Vocab)
    match relevantValidation.Length with
    | 0 -> thisAnnotation
    | _ -> { thisAnnotation with Terms = thisAnnotation.Terms |> List.map (fun t -> validateValue relevantValidation.Head t) }

  annotations |> List.map (fun a -> validateAnnotation validations a)

//let validateStatement validations (statement:Statement) =
//  {
//    Id = statement.Id
//    Title = statement.Title
//    Abstract = statement.Abstract
//    StandardId = statement.StandardId
//    StatementId = statement.StatementId
//    ObjectAnnotations = statement.ObjectAnnotations
//                        |> List.filter (fun x -> x.Terms.Length > 0)
//    DataAnnotations = statement.DataAnnotations
//                      |> List.filter (fun x -> x.Terms.Length > 0)
//                      |> validateMandatoryAnnotations validations
//                      |> validateProvidedAnnotations validations
//    Content = statement.Content
//    Html = statement.Html
//  }

///////////////////////////////// this is the new shit //////////////////////////////////////////////

let verifyRequiredAnnotationsExist (theseValidations:PublishItem List) (theseAnnotations:Annotation List) =
  let validationHasFormat thisValidation =
    match obj.ReferenceEquals(thisValidation.Format, null) with
    | false -> thisValidation, true
    | _ -> thisValidation, false

  let isConditionalReqires (validationParts:string array) = 
    let condVocab = Array.get validationParts 2
    let condTerm = Array.get validationParts 3
    theseAnnotations
    |> List.filter (fun a -> a.Vocab = condVocab)
    |> List.map (fun a -> a.Terms |> List.filter (fun t -> t = condTerm))
    |> fun al -> match al.Length with
                 | 0 -> false
                 | _ -> true

  let isValidationRequired (thisValidation:PublishItem) =
    let validationParts = thisValidation.Format.Split [|':'|]
    match validationParts.Length with
    | 0 
    | 1 -> thisValidation, false
    | _ -> match Array.get validationParts 1 with
           | "Required" -> thisValidation, true
           | "Conditional" -> thisValidation, (isConditionalReqires validationParts)
           | _ -> thisValidation, false

  let isRequiresAnnotationInList (thisValidation:PublishItem) =
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

let validateProvidedDataAnnotations (theseAnnotations:Annotation List) =

  theseAnnotations

let validateStatement (statement:Statement) =
  {
    Id = statement.Id
    Title = statement.Title
    Abstract = statement.Abstract
    StandardId = statement.StandardId
    StatementId = statement.StatementId
    ObjectAnnotations = statement.ObjectAnnotations
                        |> List.filter (fun x -> x.Terms.Length > 0)
    DataAnnotations = statement.DataAnnotations
                      |> validateProvidedDataAnnotations
    Content = statement.Content
    Html = statement.Html
  }