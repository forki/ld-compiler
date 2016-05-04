module compiler.Tests.PublishTests

open compiler.Publish
open compiler.ContentHandle
open NUnit.Framework
open Swensen.Unquote

[<Test>]
let ``convertPathToResourceUri should convert path to correct resource uri`` () =
  let outputDir = "/somedir/somedir"
  let handle = {Path = outputDir+"/qs1_st1.html";Content = ""}
  let handleUri = convertPathToResourceUri outputDir handle
  test <@ handleUri.Path = "qs1/st1" @>

