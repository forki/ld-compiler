#I "../../packages/FSharp.RDF/lib"
#r "FSharp.RDF.dll"
#r "dotNetRDF.dll"

open FSharp.RDF
open Assertion
open resource
open rdf

open FSharp.RDF

let mkKey (x : string) = x.Replace(" ", "").ToLowerInvariant()

///Load all resources from uri and make a map of rdfs:label -> resource uri
let vocabLookup uri =
  let rdfslbl = Uri.from "http://www.w3.org/2000/01/rdf-schema#label"
  let gcd = Graph.loadFrom uri
  let onlySome = List.choose id
  Resource.fromPredicate rdfslbl gcd
  |> List.map (fun r ->
       match r with
       | FunctionalDataProperty rdfslbl xsd.string x ->
         Some(mkKey x, Resource.id r)
       | r -> None)
  |> onlySome
  |> Map.ofList


///Map of annotation vocab name to vocabulary
let lookupVocab =
  ([ "setting",
     vocabLookup "https://ld.nice.org.uk/ns/qualitystandard/setting.ttl"

     "agegroup",
     vocabLookup "https://ld.nice.org.uk/ns/qualitystandard/agegroup.ttl"

     "lifestylecondition",
     vocabLookup
       "https://ld.nice.org.uk/ns/qualitystandard/lifestylecondition.ttl"

     "conditionordisease",
     vocabLookup "https://ld.nice.org.uk/ns/qualitystandard/conditionordisease.ttl"

     "servicearea",
     vocabLookup "https://ld.nice.org.uk/ns/qualitystandard/servicearea.ttl" ]
   |> Map.ofList)

///Map of annotation vocabulary name to restricted property
let lookupProperty =
  ([ "setting", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#setting"

     "agegroup",
     Uri.from "http://ld.nice.org.uk/ns/qualitystandard#age"

     "conditionordisease",
     Uri.from "http://ld.nice.org.uk/ns/qualitystandard#condition"

     "servicearea",
     Uri.from "http://ld.nice.org.uk/ns/qualitystandard#serviceArea"

     "lifestylecondition",
     Uri.from "http://ld.nice.org.uk/ns/qualitystandard#lifestyleCondition" ]
   |> Map.ofList)




















  
open FSharp.RDF.Assertion
open rdf

let owlAllValuesFrom property  = function
  | [] -> []
  | ranges -> [ for r in ranges -> objectProperty property r ]

let qsAnnotations ctx =
  let message f x = f x (Tracing.fileLocation ctx.Path)
  let info = message Tracing.info
  let warn = message Tracing.warn
  let onlySome = List.filter Option.isSome >> List.map Option.get

  //Pairs of trace message / annotation uri
  let lookUpScalar vocabKey =
    function
    | Node.Scalar(Scalar.String term) ->
      printfn "%A %A" vocabKey term
      match Map.tryFind (mkKey vocabKey) lookupVocab with
      | Some vocab ->
        match Map.tryFind (mkKey term) vocab with
        | Some uri -> (info (sprintf "Annotating for term %s" term), Some uri)
        | None -> (warn (sprintf "Cannot find '%s' in '%s'" term vocabKey), None)
      | None -> (warn (sprintf "Cannot find vocabulary '%s'" vocabKey), None)
    | _ -> (warn (sprintf "Malformed yaml"), None)

  let extracted =
    match ctx.Content with
    | Map xs ->
      xs
      |> List.map (function
           | k, YNode.List xv ->
             (Map.tryFind (mkKey k) lookupProperty,
              List.map (lookUpScalar (mkKey k)) xv)
           | k, _ -> (None, []))
      |> List.map (function
           | Some k, xs ->
             (List.map fst xs,
              owlAllValuesFrom k ((List.map snd xs |> onlySome)) )
           | _, xs -> (List.map fst xs, []))
    | _ -> []

  { Trace = List.concat (List.map fst extracted)
    Extracted =  List.map snd extracted
                 |> List.map (owl.individual ctx.TargetId [] )}

let qsDC (ctx:ExtractionContext<FSharp.Markdown.MarkdownDocument>) =

  let capture name = 
    let named = function
      | (n,v) when n = name -> Some v
      | _ -> None   
    List.tryPick named ctx.Captured

  let message f x = f x (Tracing.fileLocation ctx.Path)
  let info = message Tracing.info
  let warn = message Tracing.warn

  let titleProp =
    maybe {
      let! qsId = capture "QualityStandardId"
      let! stId  = capture "QualityStatementId"
      let title = sprintf "Quality Statement %s from Quality Standard %s" stId qsId
      return dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" (title^^xsd.string)
    }

  let qsIdProp =
    maybe {
      let! qsId = capture "QualityStandardId"
      return dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#qsidentifier" ((int (qsId)^^xsd.integer))
      }

  let stIdProp =
    maybe {
      let! stId = capture "QualityStatementId"
      return dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#stidentifier" (int (stId)^^xsd.integer)
    }

  let abstractProp = ctx.Content.Paragraphs
                     |> MarkdownParagraph.following
                             (MarkdownParagraph.h3 >>= (MarkdownSpan.re ".*(Q|q)uality.*(S|s)tatement.*"))
                     |> List.map MarkdownParagraph.text
                     |> List.map (fun x -> rdf.dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#abstract" (x^^xsd.string) )
                     |> List.tryHead


  {Extracted = [rdf.resource ctx.TargetId (( Option.toList titleProp ) @ (Option.toList abstractProp ) @ (Option.toList qsIdProp) @ (Option.toList stIdProp))]
   Trace = [
        if Option.isNone titleProp then yield (warn "No title found")
        if Option.isNone qsIdProp then yield (warn "No Quality Standard Id found")
        if Option.isNone stIdProp then yield (warn "No Quality Statement Id found")
        if Option.isNone abstractProp then yield (warn "No abstract found")
  ]}


markdownExtractor "QsDC" qsDC

yamlExtractor "QsAnnotations" qsAnnotations


target "QualityStandards" (dir "qualitystandards")
target "QualityStandardDir" (dir "qs$(QualityStandardId)")
target "QualityStatementDir" (dir "st$(QualityStatementId)")
target "QualityStatement" (file "Statement.md"
                                ["Content";"QsDC";"QsAnnotations";"HtmlFragment"]
                                (Some "/templates/QualityStatement.md")
                                "owl:NamedIndividual")
target "QualityStandard" (file "Standard.md"
                               ["Content"]
                               (Some "/templates/QualityStandard.md")
                               "owl:NamedIndividual")

"QualityStandards"
===> ["QualityStandardDir"
      ===> ["QualityStandard"
            "QualityStatementDir"
                  ===> ["QualityStatement"]]]

gg

#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
open FSharp.Data

let res = Http.RequestString("http://ld.nice.org.uk/ns/compilation.ttl")


let s = [|1;2;3|]
let s1 = s |> Set.ofArray
