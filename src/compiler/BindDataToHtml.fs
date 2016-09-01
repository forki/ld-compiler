module compiler.BindDataToHtml

open compiler.Domain
open System
open System.IO
open DotLiquid
open Microsoft.FSharp.Reflection

let parseTemplate<'T> template =
  let rec registerTypeTree ty =
    if FSharpType.IsRecord ty then
      let fields = FSharpType.GetRecordFields(ty)
      Template.RegisterSafeType(ty, [| for f in fields -> f.Name |])
      for f in fields do registerTypeTree f.PropertyType
    elif ty.IsGenericType &&
        ( let t = ty.GetGenericTypeDefinition()
          in t = typedefof<seq<_>> || t = typedefof<list<_>> ) then
      () //registerTypeTree (ty.GetGenericArguments().[0])
      registerTypeTree (ty.GetGenericArguments().[0])
    else () (* printfn "%s" ty.FullName *)

  registerTypeTree typeof<'T>
  let t = Template.Parse(template)
  fun k (v:'T) -> t.Render(Hash.FromDictionary(dict [k, box v]))

type Annot =
  {
  FirstIssued: string 
  }

let bindDataToHtml statement =
  let firstIssued = statement.Annotations |> List.find (fun x -> x.Vocab.Replace(" ","").ToLower() = "firstissued") 


  let t = parseTemplate<Annot> """
<table>
<tr>
<td>First Issued On</td><td>{{annot.first_issued |  date: "MMMM yyyy" }}</td>
</tr>
</table>
"""

  let newHtml = t  "annot" { FirstIssued=firstIssued.Terms.Head} 
  { statement with Html = newHtml }
