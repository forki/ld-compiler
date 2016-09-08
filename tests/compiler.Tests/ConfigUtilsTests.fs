module compiler.Test.ConfigUtilsTests

open NUnit.Framework
open FsUnit

open compiler.Domain
open compiler.Utils
open compiler.ConfigTypes
open compiler.ConfigUtils
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
					"Uri": "hasPositionalId",
					"Label": "PositionalId",
					"Validate": true,
					"Format": "PositionalId:Required",
                    "Display": false,
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "isNationalPriority",
					"Label": "National priority",
					"Validate": true,
					"Format": "YesNo:Required",
                    "Display": false,
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "changedPriorityOn",
					"Label": "Changed Priority On",
					"Validate": true,
					"Format": "Date:Conditional:National priority:no",
                    "Display": false,
                    "DataAnnotation": true,
                    "PropertyPath": []
				},
				{
					"Uri": "wasFirstIssuedOn",
					"Label": "First issued",
					"Validate": true,
					"Format": "Date:Required",
                    "Display": true,
                    "DataAnnotation": true,
                    "PropertyPath": []
				}
			]
			
		}
	],
    "Undiscoverables":
    [
        {
            "UndiscoverableLabel": "National priority",
            "AnnotationValue": "no"
        }
    ]

}
"""

let private expected_Jsonld = [
  "http://schema/ns/qualitystandard/setting.jsonld "
  "http://schema/ns/qualitystandard/agegroup.jsonld "
  "http://schema/ns/qualitystandard/conditionordisease.jsonld "
  "http://schema/ns/qualitystandard/servicearea.jsonld "
  "http://schema/ns/qualitystandard/lifestylecondition.jsonld "
  "http://schema/ns/qualitystandard.jsonld "
]

let private expected_Ttl = [
  "http://schema/ns/qualitystandard/setting.ttl"
  "http://schema/ns/qualitystandard/agegroup.ttl"
  "http://schema/ns/qualitystandard/conditionordisease.ttl"
  "http://schema/ns/qualitystandard/servicearea.ttl"
  "http://schema/ns/qualitystandard/lifestylecondition.ttl"
  "http://schema/ns/qualitystandard.ttl"
]

let private expected_PropPaths = [ 
  "<http://ld.nice.org.uk/ns/qualitystandard#setting>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#age>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#age>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#condition>/^rdfs:subClassOf*|<http://ld.nice.org.uk/ns/qualitystandard#condition>/rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#servicearea>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#lifestylecondition>/^rdfs:subClassOf*" 
  "<http://ld.nice.org.uk/ns/qualitystandard#title>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#abstract>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#qsidentifier>" 
  "<http://ld.nice.org.uk/ns/qualitystandard#stidentifier>"
  "<http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId>"
  "<http://ld.nice.org.uk/ns/qualitystandard#isNationalPriority>"
  "<http://ld.nice.org.uk/ns/qualitystandard#changedPriorityOn>"
  "<http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn>"
]

let private expected_PropertyValidations = [
  {
    Uri = "hasPositionalId"
    Label = "PositionalId"
    Validate = true
    Format = "PositionalId:Required"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri = "isNationalPriority"
    Label = "National priority"
    Validate = true
    Format = "YesNo:Required"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri = "changedPriorityOn"
    Label = "Changed Priority On"
    Validate = true
    Format = "Date:Conditional:National priority:no"
    Display = false
    DataAnnotation = true
    PropertyPath=[]
  }
  {
    Uri = "wasFirstIssuedOn"
    Label = "First issued"
    Validate = true
    Format = "Date:Required"
    Display = true
    DataAnnotation = true
    PropertyPath=[]
  }
]

let baseAnnotations = [ { annotation with Property = "positionalid"; Vocab = "PositionalId"; Terms = ["qs1-st1"] }
                        { annotation with Property = "nationalpriority";Vocab = "National priority"; Terms = ["yes"] }
                        { annotation with Property = "firstissued";Vocab = "First issued"; Terms = ["01-10-2000"] } ]

let a_positionalid = { annotation with Property = "positionalid"; Vocab = "PositionalId"; Terms = ["qs1-st1"]; Format = "PositionalId:Required"; Uri= "hasPositionalId"; IsValidated = true; IsDisplayed = false; IsDataAnnotation = true }
let a_nationalpriority = { annotation with Property = "nationalpriority"; Vocab = "National priority"; Terms = ["yes"]; Format = "YesNo:Required"; Uri = "isNationalPriority"; IsValidated= true; IsDisplayed = false; IsDataAnnotation = true }
let a_firstissued = { annotation with Property = "firstissued"; Vocab = "First issued"; Terms = ["01-10-2000"]; Format = "Date:Required"; Uri = "wasFirstIssuedOn"; IsValidated= true; IsDisplayed = true; IsDataAnnotation = true }

let private expected_BaseUrl = "http://ld.nice.org.uk/resource"

let private expected_undiscoverables = [ { UndiscoverableLabel = "National priority"; AnnotationValue = "no"} ]

[<Test>]
let ``ConfigUtilsTests: Should extract schema base from config`` () =
  let config = deserializeConfig sampleConfig

  config.SchemaBase |> should equal "http://schema/ns/"

[<Test>]
let ``ConfigUtilsTests: Should extract jsonld contexts from config`` () =
  let result:string list = deserializeConfig sampleConfig
                           |> getJsonLdContexts

  areListsTheSame expected_Jsonld result
[<Test>]
let ``ConfigUtilsTests: Should extract schema ttls from config`` () =
  let result = deserializeConfig sampleConfig
               |> getSchemaTtls

  areListsTheSame expected_Ttl result

[<Test>]
let ``ConfigUtilsTests: Should extract property paths from config`` () =
  let result = deserializeConfig sampleConfig
               |> getPropPaths
  
  areListsTheSame expected_PropPaths result

[<Test>]
let ``ConfigUtilsTests: Should read the expected BaseUrl from config`` () =
  deserializeConfig sampleConfig
  |> getBaseUrl
  |> should equal expected_BaseUrl

[<Test>]
let ``ConfigUtilsTests: Should read the expected property validations from config`` () =
  let result = deserializeConfig sampleConfig
               |> getAnnotationConfig

  areListsTheSame expected_PropertyValidations result

[<Test>]
let ``ValidationUtilsTests: When the config file data is added to the read annotations that is complete`` () =
  let annotationConfig = deserializeConfig sampleConfig
                         |> getAnnotationConfig

  let result = baseAnnotations
               |> List.map (addConfigToAnnotation annotationConfig)
 
  areListsTheSame [ a_positionalid; a_nationalpriority; a_firstissued ] result

[<Test>]
let ``ValidationUtilsTests: When the uri is appended with the annotations that is as expected`` () =
  let propertyBaseUrl = deserializeConfig sampleConfig
                        |> getPropertyBaseUrl

  let result = [ a_positionalid; a_nationalpriority; a_firstissued ]
               |> List.map (addUriToAnnotation propertyBaseUrl)
  
  areListsTheSame [ { a_positionalid with Uri = "http://ld.nice.org.uk/ns/qualitystandard#hasPositionalId" }; { a_nationalpriority with Uri = "http://ld.nice.org.uk/ns/qualitystandard#isNationalPriority" }; { a_firstissued with Uri = "http://ld.nice.org.uk/ns/qualitystandard#wasFirstIssuedOn" } ] result

  
[<Test>]
let ``ValidationUtilsTests: the list of undiscoverable annotation labels and values are retrieved from the config file`` () =
  let config = deserializeConfig sampleConfig

  areListsTheSame expected_undiscoverables config.Undiscoverables