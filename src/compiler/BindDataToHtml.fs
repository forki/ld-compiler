module compiler.BindDataToHtml

open System
open System.IO

open compiler.Domain
open compiler.DotLiquidExtensions

type Metadata =
  {
    FirstIssued: string 
  }

let bindDataToHtml statement =
  let firstIssued = statement.Annotations |> List.find (fun x -> x.Vocab.Replace(" ","").ToLower() = "firstissued") 


  let metadataTable = parseTemplate<Metadata> """
<table id="metadata">
<tr>
<td>First issued on</td><td>{{metadata.first_issued |  date: "MMMM yyyy" }}</td>
</tr>
</table>
"""

  let newHtml = metadataTable  "metadata" { FirstIssued=firstIssued.Terms.Head} 
  { statement with Html = newHtml }
