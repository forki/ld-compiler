module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions

type MetadataItem = {
    Label: string 
    Value: string
    IsDate: bool
  }

type MetadataViewModel = {
  MetadataItems : MetadataItem list
}

let private mapMetadataFrom statement =
  statement.Annotations 
  |> List.filter (fun x -> x.IsDisplayed) 
  |> List.map (fun x -> 
    {
      Label=x.Vocab
      Value=x.Terms.Head
      IsDate=true
    })

let bindDataToHtml statement =
  let metadata = { MetadataItems = mapMetadataFrom statement }

  let text = 
    """
    <table id="metadata">
    {% for item in metadata.MetadataItems %}
    <tr>
      <td class="col1">{{item.Label }}</td>
      <td>{% if item.IsDate  %} 
          {{item.Value |  date: "MMMM yyyy" }}
          {% else %}
          {{item.Value}}
          {% endif %}
      </td>
    </tr>
     {% endfor %}
    </table>
    """ + statement.Html

  let metadataTable = parseTemplate<MetadataViewModel> text 

  let newHtml = metadataTable  "metadata" metadata
  { statement with Html = newHtml }
