module compiler.DotLiquidExtensions

open Microsoft.FSharp.Reflection
open DotLiquid

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
