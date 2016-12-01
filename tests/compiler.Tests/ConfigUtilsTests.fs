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
					"Uri": "GUID_appliesToSetting",
					"Label": "// Setting",
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
					"Uri": "GUID_appliesToAgeGroup",
					"Label": "// Age group",
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
					"Uri": "GUID_appliesToConditionOrDisease",
					"Label": "// Condition or disease",
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
					"Uri": "GUID_appliesToServiceArea",
					"Label": "// Service area",
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
					"Uri": "GUID_appliesToFactorsAffectingHealthOrWellbeing",
					"Label": "// Factors affecting health or wellbeing",
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
					"Uri": "GUID_title",
					"Label": "// Title"
				},
				{
					"Uri": "GUID_abstract",
					"Label": "// Abstract"
				},
				{
					"Uri": "GUID_qsidentifier",
					"Label": "// qsidentifier"
				},
				{
					"Uri": "GUID_stidentifier",
					"Label": "// stidentifier"
				},
				{
					"Uri": "GUID_hasPositionalId",
					"Label": "// PositionalId",
					"Validate": true,
					"Format": "PositionalId:Required",
                    "Display": {},
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "GUID_isNationalPriority",
					"Label": "// National priority",
					"Validate": true,
					"Format": "YesNo:Required",
                    "Display": {},
                    "DataAnnotation": true,
                    "UndiscoverableWhen": "no",
                    "PropertyPath": []
				},
				{
					"Uri": "GUID_changedPriorityOn",
					"Label": "// Changed Priority On",
					"Validate": true,
					"Format": "Date:Conditional:GUID_isNationalPriority:no",
                    "Display": {
                        "Condition": "GUID_isNationalPriority:no",
                        "Label": "Priority",
                        "Template": "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level."
                    },
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "GUID_wasFirstIssuedOn",
					"Label": "// First issued",
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

let sampleQsTtl = """@prefix : <https://nice.org.uk/ontologies/qualitystandard/> .
@prefix owl: <http://www.w3.org/2002/07/owl#> .
@prefix rdf: <http://www.w3.org/1999/02/22-rdf-syntax-ns#> .
@prefix xml: <http://www.w3.org/XML/1998/namespace> .
@prefix xsd: <http://www.w3.org/2001/XMLSchema#> .
@prefix rdfs: <http://www.w3.org/2000/01/rdf-schema#> .

<https://nice.org.uk/ontologies/qualitystandard> a owl:Ontology ;
	owl:imports <http://www.w3.org/2004/02/skos/core> .
# 
# 
# #################################################################
# #
# #    Datatypes
# #
# #################################################################
# 
# 
# http://www.w3.org/2001/XMLSchema#date

xsd:date a rdfs:Datatype .
# 
# 
# 
# #################################################################
# #
# #    Object Properties
# #
# #################################################################
# 
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToFactorsAffectingHealthOrWellbeing

:GUID_appliesToFactorsAffectingHealthOrWellbeing a owl:ObjectProperty ;
	rdfs:subPropertyOf :693a50d5_304a_4e97_97f3_8f047429ae85 ;
	rdfs:label "Factors affecting health or wellbeing"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to factor affecting health or wellbeing"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToConditionOrDisease

:GUID_appliesToConditionOrDisease a owl:ObjectProperty ;
	rdfs:subPropertyOf :693a50d5_304a_4e97_97f3_8f047429ae85 ;
	rdfs:label "Condition or disease"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to condition or disease"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToAgeGroup

:GUID_appliesToAgeGroup a owl:ObjectProperty ;
	rdfs:subPropertyOf :693a50d5_304a_4e97_97f3_8f047429ae85 ;
	rdfs:label "Age group"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to age group"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToSetting

:GUID_appliesToSetting a owl:ObjectProperty ;
	rdfs:subPropertyOf :693a50d5_304a_4e97_97f3_8f047429ae85 ;
	rdfs:label "Setting"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to setting"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/693a50d5_304a_4e97_97f3_8f047429ae85

:693a50d5_304a_4e97_97f3_8f047429ae85 a owl:ObjectProperty ;
	rdfs:label "applies to"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToServiceArea

:GUID_appliesToServiceArea a owl:ObjectProperty ;
	rdfs:subPropertyOf :693a50d5_304a_4e97_97f3_8f047429ae85 ;
	rdfs:label "Service area"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "applies to service area"@en .
# 
# 
# 
# #################################################################
# #
# #    Data properties
# #
# #################################################################
# 
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_title

:GUID_title a owl:DatatypeProperty ;
	rdfs:range xsd:string ;
	rdfs:label "title"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "has standard number and statement number"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_isNationalPriority

:GUID_isNationalPriority a owl:DatatypeProperty ;
	rdfs:range xsd:boolean ;
	rdfs:label "National priority"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "is national priority"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_changedPriorityOn

:GUID_changedPriorityOn a owl:DatatypeProperty ;
	rdfs:range xsd:date ;
	rdfs:label "Priority changed"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "has changed priority on date"@en .
#
# https://nice.org.uk/ontologies/qualitystandard/GUID_wasFirstIssuedOn

:GUID_wasFirstIssuedOn a owl:DatatypeProperty ;
	rdfs:range xsd:date ;
	rdfs:label "First issued"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "has first issued date"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_abstract

:GUID_abstract a owl:DatatypeProperty ;
	rdfs:range xsd:string ;
	rdfs:label "abstract"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "has statement text"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_abstract

:GUID_abstract a owl:DatatypeProperty ;
	rdfs:range xsd:integer ;
	rdfs:label "qsidentifier"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_hasPositionalId

:GUID_hasPositionalId a owl:DatatypeProperty ;
	rdfs:range xsd:string ;
	rdfs:label "PositionalId"@en ;
	<http://www.w3.org/2004/02/skos/core#prefLabel> "has positional ID"@en .
# 
# https://nice.org.uk/ontologies/qualitystandard/GUID_stidentifier

:GUID_stidentifier a owl:DatatypeProperty ;
	rdfs:range xsd:integer ;
	rdfs:label "stidentifier"@en .
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
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToSetting>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToAgeGroup>/^rdfs:subClassOf*|<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToAgeGroup>/rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToConditionOrDisease>/^rdfs:subClassOf*|<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToConditionOrDisease>/rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToServiceArea>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_appliesToFactorsAffectingHealthOrWellbeing>/^rdfs:subClassOf*" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_title>" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_abstract>" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_qsidentifier>" 
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_stidentifier>"
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_hasPositionalId>"
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_isNationalPriority>"
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_changedPriorityOn>"
    "<https://nice.org.uk/ontologies/qualitystandard/GUID_wasFirstIssuedOn>"
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_PropPaths config.PropPaths

[<Test>]
let ``ConfigUtilsTests: Should get the Core TTL URI from config`` () =
  let config = createConfig sampleConfig
  config.CoreTtl |> should equal (Uri "http://schema/ns/qualitystandard.ttl")

[<Test>]
let ``ConfigUtilsTests: Should extract annotation vaidations from config`` () =
  let display_wasFirstIssuedOn = { t_displayItem with
                                     Always = true }
  let display_changedPriorityOn = { t_displayItem with
                                      Label = "Priority"
                                      Condition = "GUID_isNationalPriority:no"
                                      Template = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level." }

  let expected_AnnotationConfig = [
    { t_publishItem with
        Uri = "GUID_hasPositionalId"
        Label = "// PositionalId"
        Validate = true
        Format = "PositionalId:Required"
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "GUID_isNationalPriority"
        Label = "// National priority"
        Validate = true
        Format = "YesNo:Required"
        DataAnnotation = true
        UndiscoverableWhen = "no"
    }
    { t_publishItem with
        Uri = "GUID_changedPriorityOn"
        Label = "// Changed Priority On"
        Validate = true
        Format = "Date:Conditional:GUID_isNationalPriority:no"
        Display = display_changedPriorityOn
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "GUID_wasFirstIssuedOn"
        Label = "// First issued"
        Validate = true
        Format = "Date:Required"
        Display = display_wasFirstIssuedOn
        DataAnnotation = true
    }
  ]

  let config = createConfig sampleConfig
  
  areListsTheSame expected_AnnotationConfig config.AnnotationConfig

[<Test>]
let ``ConfigUtilsTests: Should replace the labels in the Config with the correct ones from the Ttl`` () =
  let config = createConfig sampleConfig
               |> fun x -> { x with CoreTtl = Content sampleQsTtl }
               |> updateLabelsFromTtl
  
  let display_wasFirstIssuedOn = { t_displayItem with
                                     Always = true }
  let display_changedPriorityOn = { t_displayItem with
                                      Label = "Priority"
                                      Condition = "GUID_isNationalPriority:no"
                                      Template = "In {{value |  date: \"MMMM yyyy\" }} the priority of this statement changed. It is no longer considered a national priority for improvement but may still be useful at a local level." }
  let expected_AnnotationConfig = [
    { t_publishItem with
        Uri = "GUID_hasPositionalId"
        Label = "PositionalId"
        Validate = true
        Format = "PositionalId:Required"
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "GUID_isNationalPriority"
        Label = "National priority"
        Validate = true
        Format = "YesNo:Required"
        DataAnnotation = true
        UndiscoverableWhen = "no"
    }
    { t_publishItem with
        Uri = "GUID_changedPriorityOn"
        Label = "Priority changed"
        Validate = true
        Format = "Date:Conditional:GUID_isNationalPriority:no"
        Display = display_changedPriorityOn
        DataAnnotation = true
    }
    { t_publishItem with
        Uri = "GUID_wasFirstIssuedOn"
        Label = "First issued"
        Validate = true
        Format = "Date:Required"
        Display = display_wasFirstIssuedOn
        DataAnnotation = true
    }
  ]
  let updatedConfig = config |> updateLabelsFromTtl

  updatedConfig.AnnotationConfig |> should equal expected_AnnotationConfig

[<Test>]
let ``ConfigUtilsTests: Should extract Rdf Term Map details from config`` () =

  let expected_RdfTerms = [
    "GUID_appliesToSetting",  "http://schema/ns/setting.ttl"
    "GUID_appliesToAgeGroup", "http://schema/ns/agegroup.ttl"
    "GUID_appliesToConditionOrDisease", "http://schema/ns/conditionordisease.ttl"
    "GUID_appliesToServiceArea", "http://schema/ns/servicearea.ttl"
    "GUID_appliesToFactorsAffectingHealthOrWellbeing", "http://schema/ns/factorsaffectinghealthorwellbeing.ttl"
  ]

  let config = createConfig sampleConfig

  areListsTheSame expected_RdfTerms config.RdfTerms

[<Test>]
let ``ConfigUtilsTests: Should extract the expected BaseUrl from config`` () =
  let expected_BaseUrl = "http://ld.nice.org.uk/things"
  let config = createConfig sampleConfig

  config.BaseUrl |> should equal expected_BaseUrl
