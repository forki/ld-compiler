module publish.IntegrationTests.Tests

open NUnit.Framework
open Swensen.Unquote
open FSharp.Data
open System.Diagnostics

let execSynchronousProcess cmd args =
  let procInfo = new ProcessStartInfo(cmd)
  procInfo.Arguments <- args
  procInfo.RedirectStandardOutput <- true
  procInfo.RedirectStandardError <- true
  procInfo.UseShellExecute <- false
  let proc = new Process();
  proc.StartInfo <- procInfo;
  proc.Start() |> ignore
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore
  let stdErr = proc.StandardError.ReadToEnd()
  let stdOut = proc.StandardOutput.ReadToEnd()
  printf "stdErr %s\n" stdErr
  printf "stdOut %s\n" stdOut

[<Test>]
let ``When publishing a statement it should have added a statement to elastic search index`` () =

  let inputFile = "/samples/qs1/st1/Statement.md"
  let outputDir = "/artifacts"
  let args = sprintf "/compiler/publish.exe %s %s" inputFile outputDir
  execSynchronousProcess "mono" args
  let s = Http.RequestString("http://elastic:9200/_count", httpMethod="GET")
  test <@ s.Contains """"count":1""" @>
  
