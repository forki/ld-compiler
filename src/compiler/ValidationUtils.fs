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

let verifyRequiredAnnotationsExist theseValidations theseAnnotations =
  let validationHasFormat thisValidation =
    match obj.ReferenceEquals(thisValidation.Format, null) with
    | false -> thisValidation, true
    | _ -> thisValidation, false

  let isConditionalReqires validationParts = 
    let condVocab = Array.get validationParts 2
    let condTerm = Array.get validationParts 3
    theseAnnotations
    |> List.filter (fun a -> a.Vocab = condVocab)
    |> List.map (fun a -> a.Terms |> List.filter (fun t -> t = condTerm))
    |> fun al -> match al.Length with
                 | 0 -> false
                 | _ -> true

  let isValidationRequired thisValidation =
    let validationParts = thisValidation.Format.Split [|':'|]
    match validationParts.Length with
    | 0 
    | 1 -> thisValidation, false
    | _ -> match Array.get validationParts 1 with
           | "Required" -> thisValidation, true
           | "Conditional" -> thisValidation, (isConditionalReqires validationParts)
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
                      |> List.map validateDataAnnotation
    Content = statement.Content
    Html = statement.Html
  }