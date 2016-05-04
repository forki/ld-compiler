module compiler.JsonLd

open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open System.Text
open System.IO
open FSharp.Data
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.RDF.JsonLD
open FSharp.Text.RegexProvider
open JsonLD.Core
open Newtonsoft.Json.Linq
open Newtonsoft.Json

///Append _id and _type, kill context for now as elastic doesn't like the remotes
///Would whine about type implicit from structure but this would be a bit hypocritical
let private elasiticerise (x : JObject) =
  x.["_id"] <- x.GetValue("@id")
  x.Add("_type",JValue("qualitystatement"))
  x.Remove("@context") |> ignore
  x

let private toJson (jObj : JObject) = 
  let sb = new StringBuilder()
  let sw = new StringWriter(sb)
  use w = new JsonTextWriter(sw)
  jObj.WriteTo w
  (jObj.["_id"].ToString(), sb.ToString())

let private jsonLdOptions () = 
  let opts = JsonLdOptions()
  opts.SetCompactArrays(true)
  opts.SetUseRdfType(false)
  opts.SetUseNativeTypes(true)
  opts.SetEmbed(System.Nullable<_>(true))
  opts.SetExplicit(System.Nullable<_>(false))
  opts

let private jsonLdContext contexts = 
  let contexts = List.map (fun c -> sprintf """ "%s" """ c) contexts
  (JObject.Parse(sprintf """ {
    "@context": [
       {"@language" : "en"},
       %s
     ]
   } """ (String.concat ",\n" contexts)))

let transformToJsonLD contexts resources =

  let opts = jsonLdOptions () 
  let context = Context(jsonLdContext contexts, opts)

  resources
  |> Seq.map (Resource.compatctedJsonLD opts context
              >> elasiticerise
              >> toJson)
