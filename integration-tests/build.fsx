// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------
#r @"packages/build/FAKE/tools/FakeLib.dll"
open Fake
open Fake.Git
open Fake.AssemblyInfoFile
open Fake.ReleaseNotesHelper
open Fake.UserInputHelper
open System
open System.IO

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "Build" (fun _ ->
    MSBuild "" "Build" [ ("DefineConstants","MONO") ] ["compiler.IntegrationTests/compiler.IntegrationTests.fsproj"] |> ignore
)

Target "RunTests" (fun _ ->

  let result = 
    ExecProcess (fun info -> info.FileName <- "mono"
                             info.Arguments <- "packages/NUnit.ConsoleRunner/tools/nunit3-console.exe --labels=All --workers=1 compiler.IntegrationTests/bin/Debug/compiler.IntegrationTests.dll") (TimeSpan.FromMinutes 2.0)
  if result <> 0 then failwithf "NUnit failed"
    
)


// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "All" DoNothing

"Clean"
  ==> "Build"
  ==> "RunTests"
  ==> "All"

RunTargetOrDefault "All"
