module compiler.Pandoc

open compiler.Domain
open compiler.Utils
open System.Diagnostics
open System.IO

let private runProcess cmd ( stdInContent:string ) args = 
  let procInfo =
    new ProcessStartInfo(
      cmd,
      Arguments=args,
      RedirectStandardInput=true,
      RedirectStandardOutput=true,
      RedirectStandardError=true,
      UseShellExecute=false)

  let proc = new Process(StartInfo=procInfo)
  
  proc.Start() |> ignore
  proc.StandardInput.Write(stdInContent)
  proc.StandardInput.Close()
  let timeout = 10000
  
  proc.WaitForExit(timeout) |> ignore
  let stdErr = proc.StandardError.ReadToEnd()
  let stdOut = proc.StandardOutput.ReadToEnd()
  printf "stdOut %s\n" stdOut
  printf "stdErr %s\n" stdErr

let private buildPandocArgs outputDir statement =
  let outputFile = prepareAsFile "notused" outputDir ".html" (statement.Id, "")
  sprintf "-f markdown -t html5 -o %s" outputFile.Path

let convertMarkdownToHtml outputDir statement =
  printf "Converting statement %s to Html" statement.Id
  statement
  |> buildPandocArgs outputDir
  |> runProcess "pandoc" statement.Content

  statement

