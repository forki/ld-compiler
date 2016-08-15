module compiler.Tests.PublishTests

open compiler.Publish
open compiler.ContentHandle
open NUnit.Framework
open FsUnit
open compiler.Utils

//[<Test>]
//let ``convertPathToResourceUri should convert path to correct resource uri`` () =
//  let outputDir = "/somedir/somedir"
//  let handle = {Guid = outputDir+"/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.html";Content = ""}
//  let handleUri = convertPathToResourceUri outputDir handle
//  handleUri.Guid |> should equal "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"

[<Test>]
let ``getGuidFromFilepath should convert path to correct resource uri`` () =
  let outputDir = "/somedir/somedir"
  let handle = {Guid = outputDir+"/b17964c7-50d8-4f9d-b7b2-1ec0c039de77.html";Content = ""}
  let handleUri = getGuidFromFilepath(handle.Guid)
  handleUri |> should equal "b17964c7-50d8-4f9d-b7b2-1ec0c039de77"
  