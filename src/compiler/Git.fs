module compiler.Git 

open System.Diagnostics
open System.IO
open System.Text
open compiler.ContentHandle
open compiler.Utils
open compiler.ConfigUtils
open compiler.ConfigTypes

let private contentDir = "/git"

let private clone destDir repoUrl =
  let args = sprintf "clone %s %s" ( repoUrl.ToString() ) destDir
  let proc = Process.Start("git", args)
  let timeout = 100000

  proc.WaitForExit(timeout) |> ignore

  
let private getConfigFromFile file =
  match File.Exists file with
  | true -> File.ReadAllText(file, Encoding.UTF8 )
  | _ -> ""

let readAll repoUrl () =
  clone contentDir repoUrl
  findFiles contentDir "*.md"
  |> Seq.map (fun f -> {Thing = f; Content = ""}) //content lazy loaded laterz

let readOne item =
  readHandle item 

let readConfig () = 
  sprintf "%s/OntologyConfig.json" contentDir
  |> getConfigFromFile
  |> createConfig
  |> updateLabelsFromTtl

let prepare () =
  tryClean contentDir