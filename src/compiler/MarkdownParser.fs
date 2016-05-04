module compiler.Markdown

open compiler.Domain
open compiler.ContentHandle
open compiler.YamlParser
open System.Text.RegularExpressions
open FSharp.Markdown

let private extract pattern input =
  let m = Regex.Match(input,pattern) 
  m.Groups.[1].Value

let private extractAbstract (markdown:MarkdownDocument) = 
  let found = Seq.item 3 markdown.Paragraphs
  match found with
    | Paragraph [Literal text] -> text
    | _ -> ""

let private extractAnnotations (markdown:MarkdownDocument) = 
  let found = Seq.item 0 markdown.Paragraphs
  match found with
    | CodeBlock (text,_,_) -> text
    | _ -> ""

let private convertToVocab {Name = name; Fields = fields} = {Vocab = name; Terms = fields}

let extractStatement contentHandle =
  let markdown = Markdown.Parse(contentHandle.Content)

  let standardId = extract "qs(\d+)" contentHandle.Path |> System.Int32.Parse
  let statementId = extract "st(\d+)" contentHandle.Path |> System.Int32.Parse
  let id = sprintf "qs%d/st%d" standardId statementId
  let title = sprintf "Quality Statement %d from Quality Standard %d" statementId standardId
  let abs = extractAbstract markdown
  let annotations = markdown
                    |> extractAnnotations 
                    |> parseYaml
                    |> List.map convertToVocab

  {Id = id
   Title = title 
   Abstract = abs 
   StandardId = standardId
   StatementId = statementId
   Annotations = annotations
   Content = contentHandle.Content}
