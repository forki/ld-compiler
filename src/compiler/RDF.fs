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
    match Map.tryFind vocabKey vocabMap with
    | Some vocabUri ->
      match Map.tryFind vocabKey termMap with
      | Some terms ->
        match Map.tryFind termId terms with
        | Some resource -> getTermUriFor resource vocabUri termId
        | None -> Log.Warning (sprintf "Cannot find '%s' in '%s'" term vocab)
                  None
      | None -> Log.Warning (sprintf "Cannot find vocabulary '%s'" vocab)
                None
    | None -> Log.Warning (sprintf "Cannot find vocabulary '%s'" vocab)
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
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard#title" (statement.Title^^xsd.string)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard#abstract" (statement.Abstract^^xsd.string)
               dataProperty !!"http://www.w3.org/2011/content#chars" (statement.Content^^xsd.string)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard#stidentifier" (statement.StatementId^^xsd.integer)
               dataProperty !!"https://nice.org.uk/ontologies/qualitystandard#qsidentifier" (statement.StandardId^^xsd.integer)
               ] @ objectAnnotations @ dataAnnotations) 
  
  r
