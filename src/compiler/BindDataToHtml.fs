module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions

type MetadataItem = {
    label: string 
    value: string
    is_date: bool
  }

type MetadataViewModel = {
  Metadata_items : MetadataItem list
}

let private mapMetadataFrom statement =
  statement.Annotations 
  |> List.filter (fun x -> x.IsDataAnnotation && x.IsDisplayed) 
  |> List.map (fun x -> 
    {
      label=x.Vocab
      value=x.Terms.Head
      is_date=true
    })

let bindDataToHtml statement =
  let metadata = { Metadata_items = mapMetadataFrom statement }

  let text = 
    """
    <table id="metadata">
    {% for item in metadata.Metadata_items %}
    <tr>
      <td class="col1">{{item.label }}</td>
      <td>{% if item.is_date  %} 
          {{item.value |  date: "MMMM yyyy" }}
          {% else %}
          {{item.value}}
          {% endif %}
      </td>
    </tr>
     {% endfor %}
    </table>
    """ + statement.Html

  let metadataTable = parseTemplate<MetadataViewModel> text 

  let newHtml = metadataTable  "metadata" metadata
  { statement with Html = newHtml }
