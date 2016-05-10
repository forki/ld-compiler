#I "../../packages/FSharp.RDF/lib"
#r "../../packages/FSharp.RDF/lib/FSharp.RDF.dll"
#r "../../packages/FSharp.RDF/lib/VDS.Common.dll"
#r "../../packages/FSharp.RDF/lib/dotNetRDF.dll"
#r "../../packages/FSharp.RDF/lib/Newtonsoft.Json.dll"
#r "../../packages/FSharp.RDF/lib/JsonLD.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll"
#r "../../packages/FSharp.Formatting/lib/net40/FSharp.Markdown.dll"

open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open System.Text
open System.IO
open FSharp.Data
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.RDF.JsonLD
open JsonLD.Core
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open FSharp.RDF
open Assertion
open resource
open rdf
open FSharp.RDF

open FSharp.Markdown

module MarkdownUtils =

  let rec textN =
    function
    | MarkdownSpan.Literal x -> x
    | MarkdownSpan.Strong xs -> text xs
    | MarkdownSpan.Emphasis xs -> text xs
    | MarkdownSpan.EmbedSpans xs -> text (xs.Render())
    | MarkdownSpan.HardLineBreak -> "\n"
    | _ -> ""

  and text = List.map textN >> String.concat ""

  let rec pTextN =
    function
    | MarkdownParagraph.Paragraph xs -> text xs
    | MarkdownParagraph.CodeBlock(x, _, _)
    | MarkdownParagraph.InlineBlock(x)
    | MarkdownParagraph.InlineBlock x -> x
    | MarkdownParagraph.Heading(_, xs) | MarkdownParagraph.Paragraph xs | MarkdownParagraph.Span xs ->
      text xs
    | MarkdownParagraph.QuotedBlock xs -> pText xs
    | MarkdownParagraph.ListBlock(_, xs) -> pText' xs
    | MarkdownParagraph.TableBlock(x, _, xs) -> ""
    | MarkdownParagraph.EmbedParagraphs(xs) -> ""
    | _ -> ""

  and pText = List.map pTextN >> String.concat ""
  and pText' = List.map pText >> String.concat ""

  let getParagraphText = pTextN

let openM path =
  let s = File.ReadAllText path;
  Markdown.Parse s

let extractAbstract (markdown:MarkdownDocument) = 
  let p = markdown.Paragraphs |> Seq.item 3 
  let doc = MarkdownDocument([p], [] |> Map.ofList)
  Markdown.WriteHtml(doc)

let st8 = openM "../../../ld-content-test/qualitystandards/qs5/st8/Statement.md" |> extractAbstract
let st7 = openM "../../../ld-content-test/qualitystandards/qs5/st7/Statement.md" |> extractAbstract
