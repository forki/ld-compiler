#I "../../packages/FSharp.RDF/lib"
#r "../../packages/FSharp.RDF/lib/FSharp.RDF.dll"
#r "../../packages/FSharp.RDF/lib/VDS.Common.dll"
#r "../../packages/FSharp.RDF/lib/dotNetRDF.dll"
#r "../../packages/FSharp.RDF/lib/Newtonsoft.Json.dll"
#r "../../packages/FSharp.RDF/lib/JsonLD.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll"
#r "../../packages/FSharp.Formatting/lib/net40/FSharp.Markdown.dll"
#r "bin/Release/publish.dll"

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
open compiler.YamlParser
open System

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

open compiler.Domain
let convertToVocab {Name = name; Fields = fields} = {Vocab = name; Terms = fields}
(* let removeText (a:string) =*)
(*   a.Replace("qs","").Replace("st","")*)

(* let splitPositionalId (positionalId:string) =*)
(*   positionalId.Split [|'-'|]*)

(* let getPositionalId (elem: Vocab) =*)
(*   List.find elem.name == "PositionalId" *)

let HandleNoPositionalIdAnnotationError =
  printfn "[Error]"
  ""

let extractQSandSTNumbers annotation =
  match annotation with
    | Some annotation -> annotation.Terms.Head.Replace("-", "/")
    | None -> HandleNoPositionalIdAnnotationError

openM "../../../ld-content-test/qualitystandards/qs93/st6/Statement.md"
  |> extractAnnotations 
  |> parseYaml 
  |> List.map convertToVocab
  |> List.tryFind (fun x -> x.Vocab.Equals("PositionalId"))
  |> extractQSandSTNumbers
  (* |> (fun y -> y.Terms.Head)*)
