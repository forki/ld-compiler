module compiler.Utils

open System.IO
open compiler.ContentHandle

let findFiles inputDir filePattern =
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

let readHandle handle =
  {Path = handle.Path; Content = File.ReadAllText handle.Path}

let writeFile file =
  try 
    File.WriteAllText(file.Path, file.Content)
    printf "Written %s\n" file.Path
  with ex -> printf "Couldnt write %s to disk!\n" file.Path

let prepareAsFile baseUrl outputDir ext (id:string, content) =
  let id = id.Replace(baseUrl+"/", "").Replace("/","_")
  {Path = sprintf "%s/%s%s" outputDir id ext; Content = content}
