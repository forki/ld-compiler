module compiler.Test.TestUtilities

open NUnit.Framework
open FsUnit
open Newtonsoft.Json

let areListsTheSame (e: 'a list) (a: 'a list) =
  let serialize xl = xl |> List.map (fun x -> JsonConvert.SerializeObject(x))

  let str_e = e |> serialize
  let str_a = a |> serialize
  let se = set str_e
  let sa = set str_a

  let diff = ((Set.difference se sa) + (Set.difference sa se)) |> Set.toList |> List.fold (+) ""
  diff |> should equal ""  