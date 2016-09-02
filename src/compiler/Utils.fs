module compiler.Utils

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
    printf "Written %s\n" file.Thing
  with ex -> printf "Couldnt write %s to disk!\n" file.Thing

let prepareAsFile baseUrl outputDir ext (id:string, content) =
  let id = id.Replace(baseUrl+"/", "").Replace("/","_")
  {Thing = sprintf "%s/%s%s" outputDir id ext; Content = content}

let tryClean dir = 
  printf "Cleaning directory : %s\n" dir 
  try 
    Directory.Delete(dir, true)
  with ex -> ()
  Directory.CreateDirectory dir |> ignore

let getProperty (x : string) = x.Replace(" ", "").ToLowerInvariant()
