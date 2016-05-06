#I "../../packages/FSharp.RDF/lib"
#r "../../packages/FSharp.RDF/lib/FSharp.RDF.dll"
#r "../../packages/FSharp.RDF/lib/VDS.Common.dll"
#r "../../packages/FSharp.RDF/lib/dotNetRDF.dll"
#r "../../packages/FSharp.RDF/lib/Newtonsoft.Json.dll"
#r "../../packages/FSharp.RDF/lib/JsonLD.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.dll"
#r "../../packages/FSharp.Data/lib/net40/FSharp.Data.DesignTime.dll"

open VDS.RDF
open VDS.RDF.Writing
open VDS.RDF.Query
open System.Text
open System.IO
open FSharp.Data
open FSharp.RDF
open FSharp.RDF.Store
open FSharp.RDF.JsonLD
open JsonLD.Core
open Newtonsoft.Json.Linq
open Newtonsoft.Json
open FSharp.RDF
open Assertion
open resource
open rdf
open FSharp.RDF


let json = "{}"
let graph = JsonProvider<""" {"@graph":[{"@id":""}] }""">.Parse(json)
