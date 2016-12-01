module compiler.Test.E2EProcessingTests

open NUnit.Framework
open FsUnit
open compiler.ContentHandle
open compiler.ConfigTypes
open compiler.ConfigUtils
open compiler.ValidationUtils
open compiler.MarkdownParser

let configString = """
{
	"SchemaBase": "http://schema/ns/",
	"UrlBase": "https://nice.org.uk/",
	"QSBase": "qualitystandard",
	"ThingBase": "http://ld.nice.org.uk/things",
	"IndexName": "kb",
	"TypeName": "qualitystatement",
	"SchemaDetails":
	[
		{
			"Schema": "setting.ttl",
			"JsonLD": "setting.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "GUID_setting",
					"PropertyPath":
					[
						"^rdfs:subClassOf*"
					]
				}
			]
		},
		{
			"Schema": "agegroup.ttl",
			"JsonLD": "agegroup.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "GUID_agegroup",
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
			"Schema": "conditionordisease.ttl",
			"JsonLD": "conditionordisease.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "GUID_condition",
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
			"Schema": "servicearea.ttl",
			"JsonLD": "servicearea.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "GUID_servicearea",
					"PropertyPath": 
					[
						"^rdfs:subClassOf*"
					]
				}
			]
			
		},
		{
			"Schema": "lifestylecondition.ttl",
			"JsonLD": "lifestylecondition.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "GUID_lifestylecondition",
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
					"Uri": "GUID_PositionalId",
					"Label": "PositionalId",
					"Validate": true,
					"Format": "PositionalId:Required",
                    "Display": {},
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "GUID_NationalPriority",
					"Label": "National priority",
					"Validate": true,
					"Format": "YesNo:Required",
                    "Display": {},
					"DataAnnotation": true,
					"PropertyPath": []
				},
				{
					"Uri": "GUID_ChangedPriorityOn",
					"Label": "Changed Priority On",
					"Validate": true,
					"Format": "Date:Conditional:National priority:no",
                    "Display": {},
					"DataAnnotation": true,
					"PropertyPath": []
				},
				{
					"Uri": "GUID_FirstIssued",
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
let content = """
```
GUID_PositionalId:
  - "qs1-st1"
GUID_NationalPriority:
  - "yes"
GUID_FirstIssued:
  - "01-06-2010"
GUID_AgeGroup:
  - "Adults"
GUID_Setting:
  - "Care home"
GUID_Service area:
  - "Community health care"
GUID_Condition or disease:
  - "Mental health and behavioural conditions"
```
This is the title 
----------------------------------------------

### Abstract 

This is the abstract.

This is some dodgily‑encoded content.
"""
let markdown = { Thing = "8422158b-302e-4be2-9a19-9085fc09dfe7"
                 Content = content.Replace(System.Environment.NewLine, "\n")}

[<Test>]
let ``E2EProcessingTests: Should process the loaded file without error``() =

  let config = createConfig configString

  let res = try
               extractStatement (markdown, content)
               |> validateStatement config
               |> ignore
               "No exception caught"
            with
            | Failure msg -> msg
  res |> should equal "No exception caught"