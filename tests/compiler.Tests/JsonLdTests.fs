module compiler.Test.JsonLdTests

open NUnit.Framework
open FsUnit
open compiler.JsonLd
open FSharp.Data
open FSharp.RDF
open Assertion
open resource
open rdf

type JsonLdSchema = JsonProvider<"""
{
  "http://ld.nice.org.uk/ns/qualitystandard#title":"",
  "_id":"",
  "_type":""
}""">

type JsonLdExplicitSchema = JsonProvider<"""
{
  "_id": "",
  "_type": "",
  "qualitystandard/conditionordisease": [
    {
      "@id": "conditionordisease/condition1"
    },
    {
      "@id": "conditionordisease/condition2"
    }
  ],
  "qualitystandard/conditionordiseaseExplicit": [
    {
      "@id": "conditionordisease/condition1"
    }
  ]
}""">


let private qsContexts = ["http://ld.nice.org.uk/ns/qualitystandard.jsonld"]
let private createResource uri =
  resource !! uri 
    [dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" ("Not used"^^xsd.string)]


[<Test>]
let ``Should add a _type field``() =
  let simpleResource = resource !! "http://ld.nice.org.uk/qualitystatement/id_goes_here"  []
  let resourceList = [simpleResource]
  let resourceMap = Map.empty.Add("allResources", Seq.singleton resourceList).Add("explicitResources", Seq.singleton resourceList)
  
  let contexts = []
  let _,json = transformToJsonLD contexts resourceMap |> Seq.head

  let jsonld = JsonLdSchema.Parse(json)

  jsonld.Type.JsonValue.AsString() |> should equal "qualitystatement"

[<Test>]
let ``Should add a explicit field to a statement``() =

  let resourceAll = 
    resource !! "http://ld.nice.org.uk/things/1" 
        [objectProperty !!"https://nice.org.uk/ontologies/qualitystandard/conditionordisease" ( Uri.from("https://nice.org.uk/ontologies/conditionordisease/condition1") );
         objectProperty !!"https://nice.org.uk/ontologies/qualitystandard/conditionordisease" ( Uri.from("https://nice.org.uk/ontologies/conditionordisease/condition2") )
        ]

  let resourceExplicit = 
    resource !! "http://ld.nice.org.uk/things/1"  
        [objectProperty !!"https://nice.org.uk/ontologies/qualitystandard/conditionordisease" ( Uri.from("https://nice.org.uk/ontologies/conditionordisease/condition1") )]

  let resourceList = [resourceAll]
  let resourceListExplicit = [resourceExplicit]  

  let resourceMap = Map.empty.Add("allResources", Seq.singleton resourceList).Add("explicitResources", Seq.singleton resourceListExplicit)
  
  let contexts = []
  let _,json = transformToJsonLD contexts resourceMap |> Seq.head
  
  let jsonld = JsonLdExplicitSchema.Parse(json)

  jsonld.Type.JsonValue.AsString() |> should equal "qualitystatement"
    
//  let addExplicitToResource xs statements:List<Statement> = 
//    statements
//    |> List.map (fun statement -> 
//                    match statement with
//                    | (FSharp.RDF.P(pUri), O(Node.Uri(o), xr)) -> 
//                        printf "pUri %A" pUri
//                        (FSharp.RDF.P(pUri), O(Node.Uri(o), xr))
//                    | _ -> statement)
//        
//  let addExplicit resources:List<Resource> = 
//    let xs = ref List.Empty
//    resources
//        |> List.map(fun r ->
//            match r with
//            | R(s, statements) ->
//            R(s, statements |> addExplicitToResource xs ))
