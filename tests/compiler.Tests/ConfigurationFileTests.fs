module compiler.Test.ConfigurationFileTests

open NUnit.Framework
open FsUnit

open compiler.Utils
open compiler.OntologyConfig
open compiler.OntologyUtils
open compiler.Test.TestUtilities
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
			"Schema": "qualitystandard/agegroup.ttl",
			"JsonLD": "qualitystandard/agegroup.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "age",
					"Label": "Age Group",
					"PropertyPath":
					[
						"^rdfs:subClassOf*",
						"rdfs:subClassOf*"
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
			"Schema": "qualitystandard/servicearea.ttl",
			"JsonLD": "qualitystandard/servicearea.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "servicearea",
					"PropertyPath": 
					[
						"^rdfs:subClassOf*"
					]
				}
			]
			
		},
		{
			"Schema": "qualitystandard/lifestylecondition.ttl",
			"JsonLD": "qualitystandard/lifestylecondition.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "lifestylecondition",
					"PropertyPath":
					[
						"^rdfs:subClassOf*"
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
					"Uri": "qsidentifier"
				},
				{
					"Uri": "stidentifier"
				},
                {
					"Uri": "positionalid",
					"Required": true,
					"Format": "PositionalId",
                    "PropertyPath": []
				},
				{
					"Uri": "firstissued",
					"Required": true,
					"Format": "Date",
					"OutFormatMask": "MMMM yyyy",
                    "PropertyPath": []
				}
			]
			
		}
	]
}
"""

let private expected_Jsonld = [
  "http://schema/ns/qualitystandard.jsonld "
  "http://schema/ns/qualitystandard/conditionordisease.jsonld "
  "http://schema/ns/qualitystandard/agegroup.jsonld "
  "http://schema/ns/qualitystandard/lifestylecondition.jsonld "
  "http://schema/ns/qualitystandard/setting.jsonld "
  "http://schema/ns/qualitystandard/servicearea.jsonld "
]

let private expected_Ttl = [
  "http://schema/ns/qualitystandard.ttl"
  "http://schema/ns/qualitystandard/agegroup.ttl"
  "http://schema/ns/qualitystandard/conditionordisease.ttl"
  "http://schema/ns/qualitystandard/lifestylecondition.ttl"
  "http://schema/ns/qualitystandard/setting.ttl"
  "http://schema/ns/qualitystandard/servicearea.ttl"
]

let private expected_PPath = [ 
  "<http://ld.nice.org.uk/ns/qualitystandard#age>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#age>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#condition>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#condition>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#setting>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#servicearea>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#lifestylecondition>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#title>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#abstract>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#qsidentifier>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#stidentifier>"
]

let private expected_Vocab_Property = [
  "setting", "http://ld.nice.org.uk/ns/qualitystandard#setting"
  "agegroup", "http://ld.nice.org.uk/ns/qualitystandard#age"
  "conditionordisease", "http://ld.nice.org.uk/ns/qualitystandard#condition"
  "servicearea", "http://ld.nice.org.uk/ns/qualitystandard#servicearea"
  "lifestylecondition", "http://ld.nice.org.uk/ns/qualitystandard#lifestylecondition"
]

let private expected_Vocab_Terms = [
  "setting", "http://schema/ns/qualitystandard/setting.ttl"
  "agegroup", "http://schema/ns/qualitystandard/agegroup.ttl"
  "lifestylecondition", "http://schema/ns/qualitystandard/lifestylecondition.ttl"
  "conditionordisease", "http://schema/ns/qualitystandard/conditionordisease.ttl"
  "servicearea", "http://schema/ns/qualitystandard/servicearea.ttl"
]

let private expected_AnnotationValidations = [
  {
    Uri= "positionalid"
    Label=null
    Required= true
    Format= "PositionalId"
    OutFormatMask=null
    PropertyPath=[]
  }
  {
    Uri= "firstissued"
    Label=null
    Required= true
    Format= "Date"
    OutFormatMask= "MMMM yyyy"
    PropertyPath=[]
  }
]

let private expected_BaseUrl = "http://ld.nice.org.uk/resource"

//[<Test>]
//let ``When I have a json string containing my ontology config it should parse into a compiler.OntologyConfig instance`` () =
//  let config = deserializeConfig sampleConfig
//
//  config.SchemaBase |> should equal "http://schema/ns/"
//
//[<Test>]
//let ``Should extract jsonld contexts from config`` () =
//  let result:string list = deserializeConfig sampleConfig
//                             |> getJsonLdContexts
//
//  areListsTheSame expected_Jsonld result
//[<Test>]
//let ``Should extract schema ttl from config`` () =
//  let result = deserializeConfig sampleConfig
//                 |> getSchemaTtls
//
//  areListsTheSame expected_Ttl result
//
//[<Test>]
//let ``Should extract property paths from config`` () =
//  let result = deserializeConfig sampleConfig
//                 |> getPropPaths
//
//  areListsTheSame expected_PPath result
//
//[<Test>]
//let ``Should extract vocab to property map from configs`` () =
//  let result = deserializeConfig sampleConfig
//                 |> getVocabList
//                 |> List.map (fun x -> (JsonConvert.SerializeObject(x)))
//  let expected_Vocab_serialised = expected_Vocab_Property
//                                    |> List.map (fun x -> (JsonConvert.SerializeObject(x)))
//
//  areListsTheSame expected_Vocab_serialised result

//[<Test>]
//let ``Should extract vocab to terms map from configs`` () =
//  let result = deserializeConfig sampleConfig
//                 |> getTermList
//                 |> List.map (fun x -> (JsonConvert.SerializeObject(x)))
//  let expected_TermMap_serialised = expected_Vocab_Terms
//                                    |> List.map (fun x -> (JsonConvert.SerializeObject(x)))
//
//  areListsTheSame expected_TermMap_serialised result

//// Need to build an integration test for the RDF Arguments
//[<Test>]
//let ``Should extract vocab to terms map from configs`` () =
//  let result = deserializeConfig sampleConfig
//                 |> getRdfArgs
//                 |> JsonConvert.SerializeObject
//  let expected_RdfArgs = "NEEDS CONSTRUCTING"
//
//  areListsTheSame [expected_RdfArgs] [result]


[<Test>]
let ``Should read the expected BaseUrl from config`` () =
  deserializeConfig sampleConfig
  |> getBaseUrl
  |> should equal expected_BaseUrl

[<Test>]
let ``Should read the expected annotation validations from config`` () =
  let result = deserializeConfig sampleConfig
                 |> getAnnotationValidations

  areListsTheSame expected_AnnotationValidations result

