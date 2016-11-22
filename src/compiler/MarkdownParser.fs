module compiler.MarkdownParser

open Serilog
open NICE.Logging
open FSharp.Markdown
open FSharp.Data
open System.Text.RegularExpressions
open compiler.Domain
open compiler.ContentHandle
open compiler.Utils
open compiler.YamlParser

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

let private convertToAnnotation {Name = name; Fields = fields} =
  { annotation with Property = getProperty name; Vocab = name; Terms = fields }

let private HandleNoPositionalIdAnnotationError =
  Log.Error "A statement was missing the PositionalId annotation"
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
  
let extractStatement (contentHandle, html) =

  let markdown = Markdown.Parse(contentHandle.Content)

  let abs = extractAbstract html
  let annotations = markdown
                    |> extractAnnotations 
                    |> parseYaml
                    |> List.map convertToAnnotation
  
  let id = annotations
            |> List.tryFind (fun x -> x.Vocab.Equals("84efb231_0424_461e_9598_1ef5272a597a"))
            |> extractQSandSTNumbers

  let standardId = (standardAndStatementNumbers id).[0] |> System.Int32.Parse
  let statementId = (standardAndStatementNumbers id).[1] |> System.Int32.Parse 

  let title = sprintf "Quality statement %d from quality standard %d" statementId standardId

  { statement with
      Id = contentHandle.Thing
      Title = title 
      Abstract = abs 
      StandardId = standardId
      StatementId = statementId
      Annotations = annotations
      Content = contentHandle.Content
      Html = html
  }
