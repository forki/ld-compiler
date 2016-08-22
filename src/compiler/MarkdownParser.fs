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

let private convertToVocab {Name = name; Fields = fields} = {Vocab = name; Terms = fields}

let private HandleAnnotationError annotation state =
  match state with
    | "Invalid" -> printfn "[Error] A statement had an invalid value for the %s annotation" annotation
    | "Missing" -> printfn "[Error] A statement was missing the %s annotation" annotation
    | _ -> printfn "[Error] An error (%s) was encountered processing a stement with the %s annotation" state annotation
  ""

let private extractQSandSTNumbers annotation =
  let PositionIdError = HandleAnnotationError "PositionalId"
  match annotation with
    | Some annotation -> annotation.Terms.Head.Replace("-", "/")
    | None -> PositionIdError "Missing"

let private validateDate (date:string) (inFormat:string) (outFormat:string) (raiseError:string -> string) =
  if (obj.ReferenceEquals(date, null)=false && date.Length > 0) then
    match System.DateTime.TryParseExact(date, inFormat, System.Globalization.CultureInfo.InvariantCulture,System.Globalization.DateTimeStyles.None) with
      | true, x -> x.ToString(outFormat)
      | _ ->  raiseError "Invalid"
  else
   raiseError "Missing"

let private exctractFirstIssued annotation =
  let FirstIssuedError = HandleAnnotationError "First Issued"
  match annotation with
    | Some annotation -> validateDate (annotation.Terms.Head) "dd-MM-yyyy" "MMMM yyyy" FirstIssuedError
    | None -> FirstIssuedError "Missing"

let private removeText (a:string) =
  a.Replace("qs","").Replace("st","")

let private splitPositionalId (positionalId:string) =
  positionalId.Split [|'/'|]

let private standardAndStatementNumbers id = 
  match id with
  | "" -> [|"0";"0"|]
  | _ -> id |> removeText |> splitPositionalId

//let private evaluateValidation (validation, (vocabList:Annotation list)) =
//  let raiseError = HandleAnnotationError validation.Uri
//  if vocabList.IsEmpty && validation.Required then
//    let a = raiseError "Missing"
//  else
    
    

let private findValidationVocab validation vocab =
  let fvl = vocab
            |> List.filter(fun v -> v.Vocab.ToLower().Replace(" ","") = validation.Uri)
  (validation, fvl)

let extractStatement (annotationValidations:PublishItem list) (contentHandle, html) =
  let markdown = Markdown.Parse(contentHandle.Content)

  let abs = extractAbstract html
  let annotations = markdown
                    |> extractAnnotations 
                    |> parseYaml
                    |> List.map convertToVocab

//  let validation = annotationValidations
//                     |> List.map (fun f -> findValidationVocab f annotations)
//                     |> evaluateValidation

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
