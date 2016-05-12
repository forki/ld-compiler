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
open FSharp.Data

let openM path =
  let s = File.ReadAllText path;
  Markdown.Parse s

let extractAbstract (markdown:MarkdownDocument) = 
  let getElements (html:HtmlDocument) =
    html.Elements()

  let getInnerText (node:HtmlNode) =
    node.InnerText()

  let findParagraphWithAbstract () = 
    let h3Index =
      markdown.Paragraphs
      |> Seq.findIndex (function
                        | Heading (3, _) -> true
                        | _ -> false)
    markdown.Paragraphs |> Seq.item ( h3Index+1 )

  let para = findParagraphWithAbstract ()

  MarkdownDocument([para], [] |> Map.ofList)
  |> Markdown.WriteHtml
  |> HtmlDocument.Parse
  |> getElements 
  |> Seq.map getInnerText
  |> String.concat ""


let st8 = openM "../../../ld-content-test/qualitystandards/qs5/st8/Statement.md" |> extractAbstract
let st7 = openM "../../../ld-content-test/qualitystandards/qs5/st7/Statement.md" |> extractAbstract
let st9 = openM "../../../ld-content-test/qualitystandards/qs97/st6/Statement.md"|> extractAbstract
let st1 = openM "../../../ld-content-test/qualitystandards/qs93/st1/Statement.md"|> extractAbstract
let st11 = openM "../../../ld-content-test/qualitystandards/qs93/st6/Statement.md"|> extractAbstract
