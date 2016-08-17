module compiler.Test.ConfigurationFileTests

open NUnit.Framework
open FsUnit

open compiler.Utils
open compiler.OntologyConfig
open compiler.OntologyUtils
open FSharp.Data
open compiler.RDF
open FSharp.RDF

let private sampleConfig = """
{
	"SchemaBase": "http://schema/ns/",
	"UrlBase": "http://ld.nice.org.uk/",
	"QSBase": "ns/qualitystandard",
	"ThingBase": "resource",
	"IndexName": "kb",
    "TypeName": "qualitystatement",
	"SchemaDetails":
	[
		{
			"Schema": "qualitystandard/setting.ttl",
			"JsonLD": "qualitystandard/setting.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "setting",
					"PropertyPath":
					[
						"^rdfs:subClassOf*"
					]
				}
			]
		},
		{
			"Schema": "qualitystandard/conditionordisease.ttl",
			"JsonLD": "qualitystandard/conditionordisease.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "condition",
					"Label": "Condition Or Disease",
					"PropertyPath": 
					[
						"^rdfs:subClassOf*",
						"rdfs:subClassOf*"
					]
				}
			]
			
		},
		{
			"Schema": "qualitystandard.ttl",
			"JsonLD": "qualitystandard.jsonld ",
			"Map": false,
			"Publish":
			[
				{
					"Uri": "title"
				},
				{
					"Uri": "abstract"
				},
				{
					"Uri": "positionalid"
				},
				{
					"Uri": "firstissued"
				}
			]
			
		}
	]
}
"""

let private expected_Jsonld = [
  "http://schema/ns/qualitystandard/conditionordisease.jsonld "
  "http://schema/ns/qualitystandard/setting.jsonld "
  "http://schema/ns/qualitystandard.jsonld "
]

let private expected_Ttl = [
  "http://schema/ns/qualitystandard/conditionordisease.ttl"
  "http://schema/ns/qualitystandard/setting.ttl"
  "http://schema/ns/qualitystandard.ttl"
]

let private expected_PPath = [
  "<http://ld.nice.org.uk/ns/qualitystandard#condition>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#condition>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#setting>/^rdfs:subClassOf*"
  "<http://ld.nice.org.uk/ns/qualitystandard#title>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#abstract>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#positionalid>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#firstissued>"
]

// For Test ``Should return the full expected RDF Arguments details``
// Trying as integration test  as get URI error here
//let private expected_RdfArgs = {
//  BaseUrl = "http://ld.nice.org.uk/resource"    
//  VocabMap = 
//    ([ "setting", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#setting"
//       "conditionordisease", Uri.from "http://ld.nice.org.uk/ns/qualitystandard#condition" ] |> Map.ofList)
//  TermMap = 
//    ([ "setting", vocabLookup "http://schema/ns/qualitystandard/setting.ttl"
//       "conditionordisease", vocabLookup "http://schema/ns/qualitystandard/conditionordisease.ttl" ] |> Map.ofList)
//}

let CompareLists e a =
  let se = set e
  let sa = set a

  ((Set.difference se sa) + (Set.difference sa se)) |> Set.toList |> List.fold (+) ""

[<Test>]
let ``When I have a json string containing my ontology config it should parse into a compiler.OntologyConfig instance`` () =
  let config = GetConfig sampleConfig

  config.SchemaBase |> should equal "http://schema/ns/"

[<Test>]
let ``Should return all but only the expected jsonld file paths`` () =
  let result:string list = GetJsonLdContext (GetConfig sampleConfig)

  CompareLists expected_Jsonld result |> should equal ""

[<Test>]
let ``Should return all but only the expected schema ttl file paths`` () =
  let result = GetSchemaTtl (GetConfig sampleConfig)

  CompareLists expected_Ttl result |> should equal ""

[<Test>]
let ``Should return all but only the expected property paths URIs`` () =
  let result = GetPropPaths (GetConfig sampleConfig)

  CompareLists expected_PPath result |> should equal ""

// Trying as integration test  as get URI error here
//[<Test>]
//let ``Should return the full expected RDF Arguments details`` () =
//  let result = GetRdfArgs (GetConfig sampleConfig)
//
//  result.BaseUrl |> should equal "http://ld.nice.org.uk/resource"
//  result.TermMap |> should equal expected_RdfArgs.TermMap
//  result.VocabMap |> should equal expected_RdfArgs.VocabMap