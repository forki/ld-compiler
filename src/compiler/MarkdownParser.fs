module compiler.Markdown

open compiler.Domain
open compiler.ContentHandle
open compiler.YamlParser
open compiler.Utils
open compiler.ConfigTypes
open compiler.ConfigUtils
open compiler.ValidationUtils
open System.Text.RegularExpressions
open FSharp.Markdown
open FSharp.Data

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

let private isDate name =
  name.Equals("firstissued")

let private convertToAnnotation {Name = name; Fields = fields} = {
  Property = getProperty name
  Vocab = name
  Terms = fields
  IsDisplayed = false 
  IsDate = name |> getProperty |> isDate
  IsValidated = false
  Format = null
  Uri = null
  IsDataAnnotation = false
}

let private HandleNoPositionalIdAnnotationError =
  printfn "[Error] A statement was missing the PositionalId annotation"
  ""

let private extractQSandSTNumbers annotation =
  match annotation with
    | Some annotation -> annotation.Terms.Head.Replace("-", "/")
    | None -> HandleNoPositionalIdAnnotationError

let private removeText (a:string) =
  a.Replace("qs","").Replace("st","")

let private splitPositionalId (positionalId:string) =
  positionalId.Split [|'/'|]

let private standardAndStatementNumbers id = 
  match id with
  | "" -> [|"0";"0"|]
  | _ -> id |> removeText |> splitPositionalId
  
let extractStatement config (contentHandle, html) =
  let propertyBaseUrl = config |> getPropertyBaseUrl
  let annotationConfig = config |> getAnnotationConfig

//  let isDataAnnotation (annotation:Annotation) =
//    match annotationConfig
//          |> List.tryFind (fun v -> (v.Validate && v.Label = annotation.Vocab))
//          with
//          | Some PublishItem -> true
//          | _ -> false

  let markdown = Markdown.Parse(contentHandle.Content)

  let abs = extractAbstract html
  let annotations = markdown
                    |> extractAnnotations 
                    |> parseYaml
                    |> List.map convertToAnnotation
                    |> List.map (addConfigToAnnotation annotationConfig)
                    |> List.map (addUriToAnnotation propertyBaseUrl)
  
  verifyRequiredAnnotationsExist annotationConfig annotations |> ignore
  
  let id = annotations
            |> List.tryFind (fun x -> x.Vocab.Equals("PositionalId"))
            |> extractQSandSTNumbers

  let standardId = (standardAndStatementNumbers id).[0] |> System.Int32.Parse
  let statementId = (standardAndStatementNumbers id).[1] |> System.Int32.Parse 

  let dataAnnotations = annotations |> List.filter (fun a -> a.IsDataAnnotation)
  let objectAnnotations = annotations |> List.filter (fun a -> a.IsDataAnnotation = false)

  let title = sprintf "Quality statement %d from quality standard %d" statementId standardId

  {
    Id = contentHandle.Thing
    Title = title 
    Abstract = abs 
    StandardId = standardId
    StatementId = statementId
    ObjectAnnotations = objectAnnotations
    DataAnnotations = dataAnnotations
    Content = contentHandle.Content
    Html = html
  }
