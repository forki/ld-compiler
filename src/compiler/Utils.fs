module compiler.Utils

open System.IO
open compiler.ContentHandle

let findFiles inputDir filePattern =
  let dir = System.IO.DirectoryInfo(inputDir)
  let files = dir.GetFiles(filePattern, System.IO.SearchOption.AllDirectories)
  files |> Array.map(fun fs -> fs.FullName) |> Array.toList

let readHandle handle =
  {Path = handle.Path; Content = File.ReadAllText handle.Content}
