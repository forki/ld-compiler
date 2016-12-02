module compiler.IntegrationTests.Utils

open CsQuery

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

