module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions

type MetadataViewModel =
  {
    FirstIssued: string 
  }

let private mapMetadataFrom statement =
  let firstIssued = statement.Annotations |> List.find (fun x -> x.Vocab.Replace(" ","").ToLower() = "firstissued") 
  {
    FirstIssued=firstIssued.Terms.Head
  }

let bindDataToHtml statement =
  let metadata = mapMetadataFrom statement 

  let metadataTable = parseTemplate<MetadataViewModel> """
<table id="metadata">
<tr>
<td>First issued on</td><td>{{metadata.first_issued |  date: "MMMM yyyy" }}</td>
</tr>
</table>
"""

  let newHtml = metadataTable  "metadata" metadata
  { statement with Html = newHtml }
