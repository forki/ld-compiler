module compiler.Git 
open System.Diagnostics

let clone repoUrl destDir =
  let args = sprintf "clone %s %s" repoUrl destDir
  let proc = Process.Start("git", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore
