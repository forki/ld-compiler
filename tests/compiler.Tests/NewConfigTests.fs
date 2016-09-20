module NewConfigTests

open NUnit.Framework
open FsUnit
open compiler.NewConfig
open compiler.Test.TestUtilities

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
					"Uri": "hasPositionalId",
					"Label": "PositionalId",
					"Validate": true,
					"Format": "PositionalId:Required",
                    "Display": {},
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "isNationalPriority",
					"Label": "National priority",
					"Validate": true,
					"Format": "YesNo:Required",
                    "Display": {},
                    "DataAnnotation": true,
                    "UndiscoverableWhen": "no",
                    "PropertyPath": []
				},
				{
					"Uri": "changedPriorityOn",
					"Label": "Changed Priority On",
					"Validate": true,
					"Format": "Date:Conditional:National priority:no",
                    "Display": {
                        "Condition": "National priority:no",
                        "Label": "Priority",
                        "Template": "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level."
                    },
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "wasFirstIssuedOn",
					"Label": "First issued",
					"Validate": true,
					"Format": "Date:Required",
                    "Display": {
                        "Always": true
                    },
                    "DataAnnotation": true,
                    "PropertyPath": []
				}
			]
			
		}
	]

}
"""

[<Test>]
let ``NewConfigTests: should extract schema base from config`` () =
  let config = createConfig sampleConfig
  config.SchemaBase |> should equal "http://schema/ns/"


[<Test>]
let ``NewConfigTests: Should extract jsonld contexts from config`` () =

  let expectedContexts = [
      "http://schema/ns/qualitystandard/setting.jsonld "
      "http://schema/ns/qualitystandard/agegroup.jsonld "
      "http://schema/ns/qualitystandard/conditionordisease.jsonld "
      "http://schema/ns/qualitystandard/servicearea.jsonld "
      "http://schema/ns/qualitystandard/lifestylecondition.jsonld "
      "http://schema/ns/qualitystandard.jsonld "
  ]
  let config = createConfig sampleConfig

  areListsTheSame expectedContexts config.JsonLdContexts

[<Test>]
let ``NewConfigTests: Should extract schema ttls from config`` () =
 
  let expectedTtls = [
    "http://schema/ns/qualitystandard/setting.ttl"
    "http://schema/ns/qualitystandard/agegroup.ttl"
    "http://schema/ns/qualitystandard/conditionordisease.ttl"
    "http://schema/ns/qualitystandard/servicearea.ttl"
    "http://schema/ns/qualitystandard/lifestylecondition.ttl"
    "http://schema/ns/qualitystandard.ttl"
  ]
  let config = createConfig sampleConfig

  areListsTheSame expectedTtls config.Ttls
