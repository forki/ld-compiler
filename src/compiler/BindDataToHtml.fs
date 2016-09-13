module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions
open Utils

type MetadataItem = {
  table: string
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
  match obj.ReferenceEquals(thisAnnotation.DisplayTemplate, null) with
  | true ->  getStandardTemplate thisAnnotation.IsDate
  | _ -> match thisAnnotation.DisplayTemplate.Length with
         | 0 -> getStandardTemplate thisAnnotation.IsDate
         | _ -> thisAnnotation.DisplayTemplate

let private getLabel (thisAnnotation:Annotation) =
  match thisAnnotation.DisplayLabel |> isNullOrWhitespace with
  | true -> thisAnnotation.Vocab
  | _ -> thisAnnotation.DisplayLabel

let private generateDataHtml thisMetadataItem =
  let dataTableTemplate =
    """
      <table id='""" + thisMetadataItem.table + """'>
      {% for value in item.values %}
        <tr>
          <td>
            """ + thisMetadataItem.value_template + """
          </td>
        </tr>
      {% endfor %}
      </table>
    """
  let dataValuesTable = parseTemplate<MetadataItem> dataTableTemplate
  { thisMetadataItem with values_html =  dataValuesTable "item" thisMetadataItem }

let transformAnnotations (theseAnnotations:Annotation List) =
  theseAnnotations
  |> List.filter (fun a -> a.IsDisplayed)
  |> List.map (fun a -> {
                          table = a.Uri
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




