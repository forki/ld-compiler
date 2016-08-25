module compiler.ValidationUtils

open compiler.OntologyConfig

// Generic raise error START
let private raiseError annotation state =
  match state with
  | "Invalid" -> sprintf "[Validation Error] Invalid value for the %s annotation" annotation
  | "Blank" -> sprintf "[Validation Error] Blank value for the %s annotation" annotation
  | "Missing" -> sprintf "[Validation Error] Missing the %s annotation" annotation
  | _ -> sprintf "[Validation Error] Error (%s) encountered while processing the %s annotation" state annotation
  |> failwith
// Generic raise error END

// Process Date START
// Will validate & convert to appropriate format
// Dates must be provided dd-MM-yyyy to be valid
let private processDate name field outFormat =
  let validateDate (date:string) (inFormat:string) (outFormat:string) (raiseError:string -> string) =
    if (obj.ReferenceEquals(date, null)=false && date.Length > 0) then
      match System.DateTime.TryParseExact(date, inFormat, System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None) with
      | true, x -> x.ToString(outFormat)
      | _ ->  raiseError "Invalid"
    else
      raiseError "Missing"
  let raiseDateError = raiseError name

  validateDate (field) "dd-MM-yyyy" outFormat raiseDateError
// Process Date END

// Vaidate PositionalId START
let validatePositionalId (posnId:string) =
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
// PositionalId END

// Validate YesNo START
let private processYesNo name field =
  let raiseYesNoError = raiseError name
  match field with
  | "yes" -> field
  | "no" -> field
  | _ -> raiseYesNoError (sprintf "%s is invalid for a YesNo annotation" field)
// Validate YesNo END

//let processField validation field =
//  match validation.Format with
//  | "Date" -> processDate validation.Uri field validation.OutFormatMask
//  | "YesNo" -> processYesNo validation.Uri field
//  | "PositionalId" -> validatePositionalId field
//  | _ -> field
//
//let private processFields validation fields =
//  fields |> List.map (fun f -> processField validation f)

// validateStatement - function used by Compiler
let validateStatement validations statement =
  statement