module compiler.Test.ConfigurationFileTests

open NUnit.Framework
open FsUnit

open compiler.Utils
open compiler.OntologyConfig
open compiler.OntologyUtils
open FSharp.Data

let sampleConfig = """
{
	"SchemaBase": "http://schema/ns/",
	"UriBase": "http://ld.nice.org.uk/ns/qualitystandard",
    "IndexName": "kb",
    "TypeName": "qualitystatement",
	"SchemaDetails":
	[
		{
			"Schema": "qualitystandard/setting.ttl",
			"JsonLD": "qualitystandard/setting.jsonld ",
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
			"Schema": "qualitystandard.ttl",
			"JsonLD": "qualitystandard.jsonld ",
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

[<Test>]
let ``When I have a json string containing my ontology config it should parse into a compiler.OntologyConfig instance`` () =
  let config = GetConfig sampleConfig

  config.SchemaBase |> should equal "http://schema/ns/"

//[<Test>]
//let ``Should read the OntologyConfig.Json file to retrieve the ontology config string`` () =
//  let config = GetConfigFromFile
//
//  config |> should haveLength (greaterThan 10)