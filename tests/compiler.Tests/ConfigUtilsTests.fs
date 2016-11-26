module compiler.Test.ConfigUtilsTests

open NUnit.Framework
open FsUnit
open compiler.ConfigTypes
open compiler.ConfigUtils
open compiler.Test.TestUtilities

let private sampleConfig = """
{
	"SchemaBase": "http://schema/ns/",
	"QSBase": "https://nice.org.uk/ontologies/qualitystandard/",
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
			"Schema": "agegroup.ttl",
			"JsonLD": "agegroup.jsonld ",
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
			"Schema": "conditionordisease.ttl",
			"JsonLD": "conditionordisease.jsonld ",
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
			"Schema": "servicearea.ttl",
			"JsonLD": "servicearea.jsonld ",
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
			"Schema": "factorsaffectinghealthorwellbeing.ttl",
			"JsonLD": "factorsaffectinghealthorwellbeing.jsonld ",
			"Map": true,
			"Publish":
			[
				{
					"Uri": "appliesToFactorsAffectingHealthOrWellbeing",
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
      "http://schema/ns/setting.jsonld "
      "http://schema/ns/agegroup.jsonld "
      "http://schema/ns/conditionordisease.jsonld "
      "http://schema/ns/servicearea.jsonld "
      "http://schema/ns/factorsaffectinghealthorwellbeing.jsonld "
      "http://schema/ns/qualitystandard.jsonld "
  ]

  let config = createConfig sampleConfig

  areListsTheSame expectedContexts config.JsonLdContexts

[<Test>]
let ``ConfigUtilsTests: Should extract schema ttls from config`` () =
 
  let expectedTtls = [
    "http://schema/ns/setting.ttl"
    "http://schema/ns/agegroup.ttl"
    "http://schema/ns/conditionordisease.ttl"
    "http://schema/ns/servicearea.ttl"
    "http://schema/ns/factorsaffectinghealthorwellbeing.ttl"
    "http://schema/ns/qualitystandard.ttl"
  ]
  let config = createConfig sampleConfig

  areListsTheSame expectedTtls config.Ttls


[<Test>]
let ``ConfigUtilsTests: Should extract property paths from config`` () =
  
  let expected_PropPaths = [ 
    "<https://nice.org.uk/ontologies/qualitystandard/appliesToSetting>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/appliesToAgeGroup>/^rdfs:subClassOf*|<https://nice.org.uk/ontologies/qualitystandard/appliesToAgeGroup>/rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/appliesToConditionOrDisease>/^rdfs:subClassOf*|<https://nice.org.uk/ontologies/qualitystandard/appliesToConditionOrDisease>/rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/appliesToServiceArea>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/appliesToFactorsAffectingHealthOrWellbeing>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/title>" 
    "<https://nice.org.uk/ontologies/qualitystandard/abstract>" 
    "<https://nice.org.uk/ontologies/qualitystandard/qsidentifier>" 
    "<https://nice.org.uk/ontologies/qualitystandard/stidentifier>"
    "<https://nice.org.uk/ontologies/qualitystandard/hasPositionalId>"
    "<https://nice.org.uk/ontologies/qualitystandard/isNationalPriority>"
    "<https://nice.org.uk/ontologies/qualitystandard/changedPriorityOn>"
    "<https://nice.org.uk/ontologies/qualitystandard/wasFirstIssuedOn>"
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
    "appliesToSetting",  "http://schema/ns/setting.ttl"
    "appliesToAgeGroup", "http://schema/ns/agegroup.ttl"
    "appliesToConditionOrDisease", "http://schema/ns/conditionordisease.ttl"
    "appliesToServiceArea", "http://schema/ns/servicearea.ttl"
    "appliesToFactorsAffectingHealthOrWellbeing", "http://schema/ns/factorsaffectinghealthorwellbeing.ttl"
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_RdfTerms config.RdfTerms

[<Test>]
let ``ConfigUtilsTests: Should extract the expected BaseUrl from config`` () =
  let expected_BaseUrl = "http://ld.nice.org.uk/things"
  let config = createConfig sampleConfig

  config.BaseUrl |> should equal expected_BaseUrl
