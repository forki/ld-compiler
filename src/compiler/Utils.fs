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
  {Guid = getGuidFromFilepath(handle.Guid); Content = File.ReadAllText(handle.Guid, Encoding.UTF8 )}

let writeFile file =
  try 
    File.WriteAllText(file.Guid, file.Content, Encoding.UTF8)
    printf "Written %s\n" file.Guid
  with ex -> printf "Couldnt write %s to disk!\n" file.Guid

let prepareAsFile baseUrl outputDir ext (id:string, content) =
  let id = id.Replace(baseUrl+"/", "").Replace("/","_")
  {Guid = sprintf "%s/%s%s" outputDir id ext; Content = content}