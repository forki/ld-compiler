module compiler.Git 

open System.Diagnostics
open compiler.Utils
open compiler.ContentHandle

let private clone destDir repoUrl =
  let args = sprintf "clone %s %s" ( repoUrl.ToString() ) destDir
  let proc = Process.Start("git", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore

let readAll repoUrl () =
  let destDir = "/git"
  clone destDir repoUrl
  findFiles destDir "*.md"
  |> Seq.map (fun f -> {Path = f; Guid=(getGuidFromFilepath f);Content = ""}) //content lazy loaded laterz

let readOne item =
  readHandle item 
  
