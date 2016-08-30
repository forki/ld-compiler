module compiler.RDF

open compiler.Domain
open FSharp.RDF
open Assertion
open resource
open rdf
open FSharp.RDF

type RDFArgs = {
  VocabMap : Map<string, Uri>     
  TermMap : Map<string, Map<string, Uri>>
  BaseUrl : string
}

let private mkKey (x : string) = x.Replace(" ", "").ToLowerInvariant()

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
       | _ -> None)
  |> onlySome
  |> Map.ofList

let private warn msg x = printf "[WARNING] %s\n" msg; x
let private info msg x = printf "%s\n" msg; x

let private lookupAnnotations vocabMap termMap annotations = 
  let flattenTerms annotation =
    annotation.Terms
    |> List.map (fun term -> (annotation.Vocab, term))

  let lookupTerms (vocab,term) =
    let termMap = termMap
    let vocabKey = mkKey vocab
    let termKey = mkKey term
    match Map.tryFind vocabKey vocabMap with
    | Some vocabUri ->
      match Map.tryFind vocabKey termMap with
      | Some terms ->
        match Map.tryFind termKey terms with
        | Some termUri -> (info (sprintf "Annotating for term %s" term) Some (vocabUri, termUri))
        | None -> (warn (sprintf "Cannot find '%s' in '%s'" term vocab) None)
      | None -> (warn (sprintf "Cannot find vocabulary '%s'" vocab) None)
    | None -> (warn (sprintf "Cannot find vocabulary '%s'" vocab) None)

  annotations
  |> List.collect flattenTerms
  |> List.map lookupTerms
  |> List.filter Option.isSome
  |> List.map Option.get
  |> List.map (fun (p, o) -> objectProperty p o)

open Assertion

let transformToRDF args statement =
  let uri = sprintf "%s/%s" args.BaseUrl statement.Id
  let annotations = lookupAnnotations args.VocabMap args.TermMap statement.Annotations
  let firstIssued = statement.Annotations |> List.find (fun x -> x.Vocab.Replace(" ","").ToLower() = "firstissued") 
  resource !! uri
    ( [a !! "http://ld.nice.org.uk/ns/qualitystandard#QualityStatement"
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" (statement.Title^^xsd.string)
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#abstract" (statement.Abstract^^xsd.string)
       dataProperty !!"http://www.w3.org/2011/content#chars" (statement.Content^^xsd.string)
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#stidentifier" (statement.StatementId^^xsd.integer)
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#qsidentifier" (statement.StandardId^^xsd.integer)
       dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#firstissued" (firstIssued.Terms.Head^^xsd.string)
       ] @ annotations ) 
  
