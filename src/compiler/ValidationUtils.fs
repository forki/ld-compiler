﻿module compiler.ValidationUtils

open compiler.Domain
open compiler.ConfigUtils
open compiler.ConfigTypes

let private raiseError annotation state =
  match state with
  | "Invalid" -> sprintf "Invalid value for the %s annotation" annotation
  | "Blank" -> sprintf "Blank value for the %s annotation" annotation
  | "Missing" -> sprintf "Missing the %s annotation" annotation
  | _ -> sprintf "Error (%s) encountered while processing the %s annotation" state annotation
  |> failwith

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

let private validateValue validation value =
  match validation.Format with
  | "Date" -> processDate validation.Uri value validation.OutFormatMask
  | "PositionalId" -> validatePositionalId value
  | _ -> value

let private validateMandatoryAnnotations validations annotations = 
  let validateAnnotationExists annotations mandatoryValidation =
    let a = annotations |> List.filter (fun a -> a.Vocab.ToLower().Replace(" ","") = mandatoryValidation.Uri.ToLower().Replace(" ",""))
      
    match a.Length with
    | 0 -> raiseError mandatoryValidation.Uri "Missing"
    | _ -> match a.Head.Vocab.Length with
           | 0 -> raiseError mandatoryValidation.Uri "Blank"
           | _ -> ()

  validations |> List.filter (fun v -> v.Required)
              |> List.map (fun v -> validateAnnotationExists annotations v)
              |> ignore
  annotations

let private validateProvidedAnnotations validations annotations =
  let validateAnnotation validations (annotation:Annotation) =
    let relevantValidation = validations |> List.filter (fun v -> v.Uri.ToLower().Replace(" ","") = annotation.Vocab.ToLower().Replace(" ",""))
    match relevantValidation.Length with
    | 0 -> annotation
    | _ -> { Vocab = annotation.Vocab; Terms = annotation.Terms |> List.map (fun t -> validateValue relevantValidation.Head t) }

  annotations |> List.map (fun a -> validateAnnotation validations a)

let validateStatement validations (statement:Statement) =
  {
    Id = statement.Id
    Title = statement.Title
    Abstract = statement.Abstract
    StandardId = statement.StandardId
    StatementId = statement.StatementId
    Annotations = statement.Annotations
                    |> List.filter (fun x -> x.Terms.Length > 0)
                    |> validateMandatoryAnnotations validations
                    |> validateProvidedAnnotations validations
    Content = statement.Content
    Html = statement.Html
  }