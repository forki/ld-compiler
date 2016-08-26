module compiler.Git 

open System.Diagnostics
open compiler.Utils
open compiler.ContentHandle
open compiler.ConfigUtils

let private contentDir = "/git"

let private clone destDir repoUrl =
  let args = sprintf "clone %s %s" ( repoUrl.ToString() ) destDir
  let proc = Process.Start("git", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore

let readAll repoUrl () =
  clone contentDir repoUrl
  findFiles contentDir "*.md"
  |> Seq.map (fun f -> {Thing = f; Content = ""}) //content lazy loaded laterz

let readOne item =
  readHandle item 

let readConfig () = 
  sprintf "%s/OntologyConfig.json" contentDir
  |> getConfigFromFile
  |> deserializeConfig
  
let prepare () =
  tryClean contentDir
  