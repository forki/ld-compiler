module compiler.RDF

open Serilog
open NICE.Logging
open FSharp.RDF
open resource
open Assertion
open rdf
open compiler.Domain
open compiler.Utils
open compiler.ConfigTypes

///Load all resources from uri and make a map of rdfs:label -> resource uri

let private lookupAnnotations vocabMap termMap annotations = 
  let flattenTerms annotation =
    annotation.Terms
    |> List.map (fun term -> (annotation.Vocab, term))

  let getTermUriFor resource vocabUri termId =
    let rdfslbl = Uri.from "http://www.w3.org/2000/01/rdf-schema#label"
    let termUri = Resource.id resource
    match resource with 
    | DataProperty rdfslbl FSharp.RDF.xsd.string label -> 
      Log.Information("Annotating for term {@termId} {@termLabel}", termId, label |> List.head)
      Some (vocabUri, termUri)
    | _ -> None

  let lookupTerms (vocab,term) =
    let termMap = termMap
    let vocabKey = getProperty vocab
    let termId = getProperty term

    (* printf "vocabMap => %A vocabKey => %A" vocabMap vocabKey*)

    match Map.tryFind vocabKey vocabMap with
    | Some vocabUri ->
      match Map.tryFind vocabKey termMap with
      | Some terms ->
        match Map.tryFind termId terms with
        | Some resource -> getTermUriFor resource vocabUri termId
        | None -> Log.Warning (sprintf "Cannot find '%s' in '%s'" term vocab)
                  None
      | None -> Log.Warning (sprintf "Cannot find vocabulary from termMap '%s'" vocab)
                None
    | None -> Log.Warning (sprintf "Cannot find vocabulary from vocabMap '%s'" vocab)
              None

  annotations
  |> List.collect flattenTerms
  |> List.map lookupTerms
  |> List.filter Option.isSome
  |> List.map Option.get
  |> List.map (fun (p, o) -> objectProperty p o)

open Assertion

let private generateDataAnnotations (dataAnnotations:Annotation List) =
  dataAnnotations
  |> List.map (fun a -> a.Uri, a.Terms)
  |> List.map (fun at -> at |> snd
                            |> List.map (fun t -> (fst at), t))
  |> List.concat
  |> List.map (fun (a,t) -> dataProperty !!a (t^^xsd.string))

let transformToRDF args statement =

  let dataAnnotations = statement.Annotations
                        |> List.filter (fun a -> a.IsDataAnnotation)
                        |> generateDataAnnotations 
                          
  let objectAnnotations = statement.Annotations
                          |> List.filter (fun a -> a.IsDataAnnotation = false)
                          |> lookupAnnotations args.VocabMap args.TermMap
  
  let uri = sprintf "%s/%s" args.BaseUrl statement.Id
 
  let r = resource !! uri
            ( [a !! "https://nice.org.uk/ontologies/qualitystandard#QualityStatement"
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard/bc8e0db0_5d8a_4100_98f6_774ac0eb1758" (statement.Title^^xsd.string)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard/1efaaa6a_c81a_4bd6_b598_c626b21c71fd" (statement.Abstract^^xsd.string)
               dataProperty !!"http://www.w3.org/2011/content#chars" (statement.Content^^xsd.string)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard/9fcb3758_a4d3_49d7_ab10_6591243caa67" (statement.StatementId^^xsd.integer)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de" (statement.StandardId^^xsd.integer)
               ] @ objectAnnotations @ dataAnnotations) 
  
  r
