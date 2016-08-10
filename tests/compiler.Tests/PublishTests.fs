module compiler.Tests.PublishTests

open compiler.Publish
open compiler.ContentHandle
open NUnit.Framework
open FsUnit

[<Test>]
let ``convertPathToResourceUri should convert path to correct resource uri`` () =
  let outputDir = "/somedir/somedir"
  let handle = {Path = outputDir+"/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.html";Guid="b17964c7-50d8-4f9d-b7b2-1ec0c039de77";Content = ""}
  let handleUri = convertPathToResourceUri outputDir handle
  handleUri.Path |> should equal "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"

