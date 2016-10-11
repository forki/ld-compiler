module compiler.Utils

open Serilog
open NICE.Logging
open System.IO
open System.Text
open compiler.ContentHandle

let findFiles inputDir filePattern =
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

let getGuidFromFilepath (filePath:string) =
  let filename = filePath.Split[|'/'|]
                   |> Array.rev
                   |> Array.head
  filename.Split[|'.'|]
    |> Array.head

let readHandle handle =
  {Thing = getGuidFromFilepath(handle.Thing); Content = File.ReadAllText(handle.Thing, Encoding.UTF8 )}

let writeFile file =
  try 
    File.WriteAllText(file.Thing, file.Content, Encoding.UTF8)
    Log.Information (sprintf "Written %s" file.Thing)
  with ex -> Log.Error (sprintf "Couldnt write %s to disk!" file.Thing)

let prepareAsFile baseUrl outputDir ext (id:string, content) =
  let id = id.Replace(baseUrl+"/", "").Replace("/","_")
  {Thing = sprintf "%s/%s%s" outputDir id ext; Content = content}

let tryClean dir = 
  Log.Information (sprintf "Cleaning directory : %s" dir)
  try 
    Directory.Delete(dir, true)
  with ex -> ()
  Directory.CreateDirectory dir |> ignore

let getProperty (x : string) = x.Replace(" ", "").ToLowerInvariant()

let isNullOrWhitespace (x:string) =
  match obj.ReferenceEquals(x, null) with
  | true -> true
  | _ -> match x.Replace(" ","").Length with
         | 0 -> true
         | _ -> false