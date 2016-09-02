module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions

type MetadataViewModel =
  {
    Label: string 
    Value: string
  }

let private mapMetadataFrom statement =
  let firstIssued = statement.Annotations |> List.find (fun x -> x.Vocab.Replace(" ","").ToLower() = "firstissued") 
  {
    Label="First issued on"
    Value=firstIssued.Terms.Head
  }

let bindDataToHtml statement =
  let metadata = mapMetadataFrom statement 

  let text = 
    """
    <table id="metadata">
    <tr>
      <td>{{metadata.Label }}</td><td>{{metadata.Value |  date: "MMMM yyyy" }}</td>
    </tr>
    </table>
    """ + statement.Html

  let metadataTable = parseTemplate<MetadataViewModel> text 

  let newHtml = metadataTable  "metadata" metadata
  { statement with Html = newHtml }
