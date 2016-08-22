module compiler.Preamble

open compiler.Stardog
open compiler.ContentHandle
open compiler.Utils
open FSharp.Data
open System.IO

let downloadSchema schemas outputDir =
  let download (schema:string) =
    {Thing = sprintf "%s/%s" outputDir (schema.Remove(0,schema.LastIndexOf('/')+1))
     Content = Http.RequestString(schema)}

  List.iter (download >> writeFile) schemas

let private tryClean dir = 
  printf "Cleaning directory : %s\n" dir 
  try 
    Directory.Delete(dir, true)
  with ex -> ()
  Directory.CreateDirectory dir |> ignore

let prepare inputDir outputDir dbName dbUser dbPass = 
  [inputDir; outputDir] |> Seq.iter tryClean
  Stardog.deleteDb dbName dbUser dbPass
  Stardog.createDb dbName
