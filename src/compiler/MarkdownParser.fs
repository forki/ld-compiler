module compiler.Markdown

open compiler.Domain
open compiler.ContentHandle
open compiler.YamlParser
open System.Text.RegularExpressions
open FSharp.Markdown
open FSharp.Data
open compiler.OntologyConfig

let private extract pattern input =
  let m = Regex.Match(input,pattern) 
  m.Groups.[1].Value

let rec removeAnchors html =
  let m = Regex.Match(html, "<a .*?>")
  match m.Success with
  | true -> removeAnchors (html.Replace(m.Groups.[0].Value,"").Replace("</a>",""))
  | false -> html

let private extractAbstract (html:string) = 
  let findParagraphWithAbstract () = 
    try
      let h = HtmlDocument.Parse(html)
      let p = h.Descendants ["p"] |> Seq.head 
      p.ToString()
    with _ -> ""

  findParagraphWithAbstract ()
  |> removeAnchors

let private extractAnnotations (markdown:MarkdownDocument) = 
  let found = Seq.item 0 markdown.Paragraphs
  match found with
  | CodeBlock (text,_,_) -> text
  | _ -> ""

let private raiseError annotation state =
  match state with
  | "Invalid" -> sprintf "[Validation Error] Invalid value for the %s annotation" annotation
  | "Blank" -> sprintf "[Validation Error] Blank value for the %s annotation" annotation
  | "Missing" -> sprintf "[Validation Error] Missing the %s annotation" annotation
  | _ -> sprintf "[Validation Error] Error (%s) encountered while processing the %s annotation" state annotation
  |> failwith

let private extractQSandSTNumbers annotation =
  match annotation with
  | Some annotation -> annotation.Terms.Head.Replace("-", "/")
  | None -> ""

let private validateDate (date:string) (inFormat:string) (outFormat:string) (raiseError:string -> string) =
  if (obj.ReferenceEquals(date, null)=false && date.Length > 0) then
    match System.DateTime.TryParseExact(date, inFormat, System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None) with
    | true, x -> x.ToString(outFormat)
    | _ ->  raiseError "Invalid"
  else
   raiseError "Missing"

let private processDate name field outFormat =
  let raiseDateError = raiseError name

  validateDate (field) "dd-MM-yyyy" outFormat raiseDateError
 
let private removeText (a:string) =
  a.Replace("qs","").Replace("st","")

let private splitPositionalId (positionalId:string) =
  positionalId.Split [|'/'|]

let private standardAndStatementNumbers id = 
  match id with
  | "" -> [|"0";"0"|]
  | _ -> id |> removeText |> splitPositionalId

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
  
let processField validation field =
  match validation.Format with
  | "Date" -> processDate validation.Uri field validation.OutFormatMask
  | "PositionalId" -> validatePositionalId field
  | _ -> field

let private processFields validation fields =
  fields |> List.map (fun f -> processField validation f) 

let private convertToVocabValidate validationList section =
  let validation = validationList |> List.filter (fun v -> v.Uri.ToLower().Replace(" ","") = section.Name.ToLower().Replace(" ",""))

  match validation.Length with
  | 0 -> { Vocab = section.Name; Terms = section.Fields }
  | _ -> { Vocab = section.Name; Terms = (section.Fields |> processFields validation.Head |> List.filter (fun f -> f.Length > 0 )) }

let private validateAnnotationExists (annotations:Section List) mandatoryValidation =
  let a = annotations |> List.filter (fun a -> a.Name.ToLower().Replace(" ","") = mandatoryValidation.Uri.ToLower().Replace(" ",""))
  
  match a.Length with
  | 0 -> raiseError mandatoryValidation.Uri "Missing"
  | _ -> match a.Head.Fields.Length with
         | 0 -> raiseError mandatoryValidation.Uri "Blank"
         | _ -> ""
   
let private validateMandatoryAnnotations validations (annotations:Section List) = 
  validations |> List.filter (fun v -> v.Required)
              |> List.map (fun v -> validateAnnotationExists annotations v)
              |> ignore
  annotations

let extractStatement (annotationValidations:PublishItem list) (contentHandle, html) =
  let markdown = Markdown.Parse(contentHandle.Content)

  let abs = extractAbstract html
  let annotations = markdown
                    |> extractAnnotations 
                    |> parseYaml
                    |> validateMandatoryAnnotations annotationValidations
                    |> List.map (fun section -> convertToVocabValidate annotationValidations section)
                    |> List.filter (fun a -> a.Terms.Length > 0)
                    
  let positionalId = annotations
                     |> List.tryFind (fun x -> x.Vocab.Equals("PositionalId"))
                     |> extractQSandSTNumbers

  let standardId = (standardAndStatementNumbers positionalId).[0] |> System.Int32.Parse
  let statementId = (standardAndStatementNumbers positionalId).[1] |> System.Int32.Parse

  let title = sprintf "Quality statement %d from quality standard %d" statementId standardId

  {
    Id = contentHandle.Thing
    Title = title 
    Abstract = abs 
    StandardId = standardId
    StatementId = statementId
    Annotations = annotations
    Content = contentHandle.Content
    Html = html
  }
