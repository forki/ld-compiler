module compiler.IntegrationTests.Utils

open CsQuery
open System

type CQ = | CQ of CsQuery.CQ
  with static member select (s:string) (CQ cq)  = cq.Select(s) |> CQ
       static member text (CQ cq) = cq.Text()
       static member attr (s:string) (CQ cq) = cq.Attr s
       static member first (CQ cq) = cq.First() |> CQ
       static member last (CQ cq) = cq.Last() |> CQ
       static member length (CQ cq) = cq.Length
       static member cq (CQ cq) = cq


let parseHtml (resp:string) = CQ.Create(resp) |> CQ

let cleanString (input:string) =
  input
  |> fun(s)->s.Trim()
  |> fun(s)->s.Replace("\n", "")

let termQuery = """{"term" : {"%s" : "%s"}}"""
let shouldQuery = """{"bool" : {
            "should" : [
              %s
            ]
          }}"""
let mustQuery = """{
"from": 0, "size": 1500,
"query": {
  "filtered": {
    "filter" : {
      "bool" : {
        "must" : [
          %s
        ]
      }
    }
  }
},
"sort": [
  { "https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de" : { "order": "desc" }},
  { "https://nice.org.uk/ontologies/qualitystandard/9fcb3758_a4d3_49d7_ab10_6591243caa67" : { "order": "asc" }}
]
}"""

let defaultQuery = """{
"sort": [
  { "https://nice.org.uk/ontologies/qualitystandard/3ff270e4_655a_4884_b186_e033f58759de" : { "order": "desc" }},
  { "https://nice.org.uk/ontologies/qualitystandard/9fcb3758_a4d3_49d7_ab10_6591243caa67" : { "order": "asc" }}
]
}"""

type AggregatedFilter = {
  Vocab: string
  Terms: string list
}

let private insertItemsInto query item1 item2 =
  sprintf (Printf.StringFormat<string->string->string>(query)) item1 item2

let private insertItemInto query item =
  sprintf (Printf.StringFormat<string->string>(query)) item

let private concatToStringWithDelimiter delimiter items = 
  items
  |> Seq.fold (fun acc item ->
                match acc with
                | "" -> item
                | _ -> acc + delimiter + item) ""

let private BuildQuery filters =
  let shouldQuery = 
    filters
    |> Seq.map (fun {Vocab=v; Terms=terms} -> 
                    terms
                    |> Seq.map (fun t -> insertItemsInto termQuery (Uri.UnescapeDataString v) t)
                    |> concatToStringWithDelimiter ",")
    |> Seq.map (fun termQueriesStr -> insertItemInto shouldQuery termQueriesStr)
    |> concatToStringWithDelimiter ","

  let fullQuery = insertItemInto mustQuery shouldQuery

  fullQuery

let getQueryString (filters:AggregatedFilter list) =
  match filters with
  | [] -> defaultQuery
  | _ -> BuildQuery (filters |> List.toSeq)

