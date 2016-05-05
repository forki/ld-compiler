module compiler.Tests.PublishTests

open compiler.Publish
open compiler.ContentHandle
open NUnit.Framework
open FsUnit

[<Test>]
let ``convertPathToResourceUri should convert path to correct resource uri`` () =
  let outputDir = "/somedir/somedir"
  let handle = {Path = outputDir+"/qs1_st1.html";Content = ""}
  let handleUri = convertPathToResourceUri outputDir handle
  handleUri.Path |> should equal "qs1/st1"

