module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions
open Utils

type MetadataItem = {
  label: string
  values: string list
  value_template: string
  values_html: string
}

type MetadataViewModel = {
  Metadata_items : MetadataItem list
}

let private getStandardTemplate isDate =
  match isDate with
  | true -> """{{value |  date: "MMMM yyyy" }}"""
  | _ -> "{{value}}"

let private getTemplate (thisAnnotation:Annotation) =
  match thisAnnotation.DisplayTemplate |> isNullOrWhitespace with
  | true -> getStandardTemplate thisAnnotation.IsDate
  | _ -> thisAnnotation.DisplayTemplate

let private getLabel (thisAnnotation:Annotation) =
  match thisAnnotation.DisplayLabel |> isNullOrWhitespace with
  | true -> thisAnnotation.Property
  | _ -> thisAnnotation.DisplayLabel

let generateTailHtml thisMetadataItem =
  let repeatedTermsTemplate =
    """
    {% for value in values %}
    <hr>
    """ + thisMetadataItem.value_template + """
    {% endfor %}
    """
  let parseThem =  parseTemplate<string list> repeatedTermsTemplate
  parseThem "values" thisMetadataItem.values.Tail

let private generateDataHtml thisMetadataItem =
  let parseThis = parseTemplate<string> thisMetadataItem.value_template
  let headHtml = parseThis "value" thisMetadataItem.values.Head
  let tailHtml = match thisMetadataItem.values.Tail.Length with
                 | 0 -> ""
                 | _ -> generateTailHtml thisMetadataItem
  { thisMetadataItem with values_html = headHtml + tailHtml }

let transformAnnotations (theseAnnotations:Annotation List) =
  theseAnnotations
  |> List.filter (fun a -> a.IsDisplayed)
  |> List.map (fun a -> {
                          label = a |> getLabel
                          values = a.Terms
                          value_template = a |> getTemplate
                          values_html = ""
                        } )
  |> List.map generateDataHtml

let bindDataToHtml thisStatement =
  let metadata = { Metadata_items = transformAnnotations thisStatement.Annotations }

  let outlineTemplate =
    """
       <table id="metadata">
       {% for item in metadata.Metadata_items %}
         <tr>
           <td class="col1">
             {{item.label }}
           </td>
           <td>
             {{item.values_html}}
           </td>
         </tr>
       {% endfor %}
       </table>
    """

  let metadataTable = parseTemplate<MetadataViewModel> outlineTemplate
  let newHtml = metadataTable "metadata" metadata
  { thisStatement with Html = newHtml + thisStatement.Html }




