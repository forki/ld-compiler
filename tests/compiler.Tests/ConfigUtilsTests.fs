module compiler.Test.ConfigUtilsTests

open NUnit.Framework
open FsUnit
open compiler.ConfigTypes
open compiler.ConfigUtils
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
					"Uri": "appliesToSetting",
					"Label": "Setting",
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
					"Uri": "appliesToAgeGroup",
					"Label": "Age group",
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
					"Uri": "appliesToConditionOrDisease",
					"Label": "Condition or disease",
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
					"Uri": "appliesToServiceArea",
					"Label": "Service area",
					"PropertyPath": 
					[
						"^rdfs:subClassOf*"
					]
				}
			]
			
		},
		{
			"Schema": "qualitystandard/factorsaffectinghealthorwellbeing.ttl",
			"JsonLD": "qualitystandard/factorsaffectinghealthorwellbeing.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "appliesToFactorAffectingHealthOrWellbeing",
					"Label": "Factors affecting health or wellbeing",
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
let ``ConfigUtilsTests: should extract schema base from config`` () =
  let config = createConfig sampleConfig
  config.SchemaBase |> should equal "http://schema/ns/"


[<Test>]
let ``ConfigUtilsTests: Should extract jsonld contexts from config`` () =

  let expectedContexts = [
      "http://schema/ns/qualitystandard/setting.jsonld "
      "http://schema/ns/qualitystandard/agegroup.jsonld "
      "http://schema/ns/qualitystandard/conditionordisease.jsonld "
      "http://schema/ns/qualitystandard/servicearea.jsonld "
      "http://schema/ns/qualitystandard/factorsaffectinghealthorwellbeing.jsonld "
      "http://schema/ns/qualitystandard.jsonld "
  ]

  let config = createConfig sampleConfig

  areListsTheSame expectedContexts config.JsonLdContexts

[<Test>]
let ``ConfigUtilsTests: Should extract schema ttls from config`` () =
 
  let expectedTtls = [
    "http://schema/ns/qualitystandard/setting.ttl"
    "http://schema/ns/qualitystandard/agegroup.ttl"
    "http://schema/ns/qualitystandard/conditionordisease.ttl"
    "http://schema/ns/qualitystandard/servicearea.ttl"
    "http://schema/ns/qualitystandard/factorsaffectinghealthorwellbeing.ttl"
    "http://schema/ns/qualitystandard.ttl"
  ]
  let config = createConfig sampleConfig

  areListsTheSame expectedTtls config.Ttls


[<Test>]
let ``ConfigUtilsTests: Should extract property paths from config`` () =
  
  let expected_PropPaths = [ 
    "<http://ld.nice.org.uk/ns/qualitystandard#appliesToSetting>/^rdfs:subClassOf*" 
    "<http://ld.nice.org.uk/ns/qualitystandard#appliesToAgeGroup>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#appliesToAgeGroup>/rdfs:subClassOf*" 
    "<http://ld.nice.org.uk/ns/qualitystandard#appliesToConditionOrDisease>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#appliesToConditionOrDisease>/rdfs:subClassOf*" 
    "<http://ld.nice.org.uk/ns/qualitystandard#appliesToServiceArea>/^rdfs:subClassOf*" 
    "<http://ld.nice.org.uk/ns/qualitystandard#appliesToFactorAffectingHealthOrWellbeing>/^rdfs:subClassOf*" 
    "<http://ld.nice.org.uk/ns/qualitystandard#title>" 
    "<http://ld.nice.org.uk/ns/qualitystandard#abstract>" 
    "<http://ld.nice.org.uk/ns/qualitystandard#qsidentifier>" 
    "<http://ld.nice.org.uk/ns/qualitystandard#stidentifier>"
    "<http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId>"
    "<http://ld.nice.org.uk/ns/qualitystandard#isNationalPriority>"
    "<http://ld.nice.org.uk/ns/qualitystandard#changedPriorityOn>"
    "<http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn>"
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_PropPaths config.PropPaths

[<Test>]
let ``ConfigUtilsTests: Should extract annotation vaidations from config`` () =
  let display_wasFirstIssuedOn = { t_displayItem with
                                     Always = true }
  let display_changedPriorityOn = { t_displayItem with
                                      Label = "Priority"
                                      Condition = "National priority:no"
                                      Template = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level." }

  let expected_AnnotationConfig = [
    { t_publishItem with
        Uri = "hasPositionalId"
        Label = "PositionalId"
        Validate = true
        Format = "PositionalId:Required"
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "isNationalPriority"
        Label = "National priority"
        Validate = true
        Format = "YesNo:Required"
        DataAnnotation = true
        UndiscoverableWhen = "no"
    }
    { t_publishItem with
        Uri = "changedPriorityOn"
        Label = "Changed Priority On"
        Validate = true
        Format = "Date:Conditional:National priority:no"
        Display = display_changedPriorityOn
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "wasFirstIssuedOn"
        Label = "First issued"
        Validate = true
        Format = "Date:Required"
        Display = display_wasFirstIssuedOn
        DataAnnotation = true
    }
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_AnnotationConfig config.AnnotationConfig

[<Test>]
let ``ConfigUtilsTests: Should extract Rdf Term Map details from config`` () =

  let expected_RdfTerms = [
    "setting",  "http://schema/ns/qualitystandard/setting.ttl"
    "agegroup", "http://schema/ns/qualitystandard/agegroup.ttl"
    "conditionordisease", "http://schema/ns/qualitystandard/conditionordisease.ttl"
    "servicearea", "http://schema/ns/qualitystandard/servicearea.ttl"
    "factorsaffectinghealthorwellbeing", "http://schema/ns/qualitystandard/factorsaffectinghealthorwellbeing.ttl"
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_RdfTerms config.RdfTerms

[<Test>]
let ``ConfigUtilsTests: Should extract the expected BaseUrl from config`` () =
  let expected_BaseUrl = "http://ld.nice.org.uk/resource"
  let config = createConfig sampleConfig

  config.BaseUrl |> should equal expected_BaseUrl
