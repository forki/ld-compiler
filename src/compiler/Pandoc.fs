module compiler.Pandoc

open Serilog
open NICE.Logging
open compiler.ContentHandle
open compiler.Utils
open System.Text
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
      UseShellExecute=false,
      StandardOutputEncoding=System.Text.Encoding.UTF8)

  let proc = new Process(StartInfo=procInfo)
  
  proc.Start() |> ignore
  let buffer = System.Text.Encoding.UTF8.GetBytes(stdInContent);
  proc.StandardInput.BaseStream.Write(buffer, 0, buffer.Length);
  proc.StandardInput.WriteLine()
  proc.StandardInput.Close()
  let timeout = 10000
  
  proc.WaitForExit(timeout) |> ignore
  let stdErr = proc.StandardError.ReadToEnd()
  let stdOut = proc.StandardOutput.ReadToEnd()
  if stdErr <> "" then
    Log.Error (sprintf "Pandoc: %s\n" stdErr)
  stdOut


let private buildPandocArgs () =
  sprintf "-f markdown -t html5" 

let convertMarkdownToHtml contentHandle =
  Log.Information (sprintf "Converting statement %s to Html" contentHandle.Thing)
  let html = runProcess "pandoc" contentHandle.Content (buildPandocArgs())

  (contentHandle, html)

