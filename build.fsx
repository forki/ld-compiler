// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r @"packages/build/FAKE/tools/FakeLib.dll"
#r @"packages/FSharpLint/FSharpLint.Application.dll"
#r @"packages/FSharpLint/FSharpLint.FAKE.dll"
open FSharpLint.FAKE
open Fake
open System
open System.IO

// --------------------------------------------------------------------------------------
// START TODO: Provide project-specific details below
// --------------------------------------------------------------------------------------

// Information about the project are used
//  - for version and project name in generated AssemblyInfo file
//  - by the generated NuGet package
//  - to run tests and to publish documentation on GitHub gh-pages
//  - for documentation, you also need to edit info in "docs/tools/generate.fsx"

// The name of the project
// (used by attributes in AssemblyInfo, name of a NuGet package and directory in 'src')
let project = "Compiler"

// File system information
let solutionFile  = "compiler.sln"

// Pattern specifying assemblies to be tested using NUnit
let unitTestAssemblies = "tests/**/bin/Release/*.Tests*.dll"

Target "CopyBinaries" (fun _ ->
    CopyDir "bin" "src/compiler.api/bin/Release" (fun _ -> true)
)

// --------------------------------------------------------------------------------------
// Clean build results

Target "Clean" (fun _ ->
    CleanDirs ["bin"; "temp"]
)

Target "CleanDocs" (fun _ ->
    CleanDirs ["docs/output"]
)

// --------------------------------------------------------------------------------------
// Build library & test project

Target "BuildDebug" (fun _ ->
    printf "Running build task...."
    let s = !! solutionFile
#if MONO
            |> MSBuild "" "Build" [ ("DefineConstants","MONO") ] 
#else
            |> MSBuildDebug "" "Build"
#endif
    printf "%A" s
)
Target "BuildRelease" (fun _ ->
    printf "Running build task...."
    let s = !! solutionFile
#if MONO
            |> MSBuildReleaseExt "" [ ("DefineConstants","MONO") ] "Build"
#else
            |> MSBuildRelease "" "Build"
#endif
    printf "%A" s
)

Target "RebuildDebug" (fun _ ->
    printf "Running rebuild task...."
    let s = !! solutionFile
#if MONO
            |> MSBuild "" "Rebuild" [ ("DefineConstants","MONO") ] 
#else
            |> MSBuildDebug "" "Rebuild"
#endif
    printf "%A" s
)

Target "RebuildRelease" (fun _ ->
    printf "Running rebuild task...."
    let s = !! solutionFile
#if MONO
            |> MSBuildReleaseExt "" [ ("DefineConstants","MONO") ] "Rebuild"
#else
            |> MSBuildRelease "" "Rebuild"
#endif
    printf "%A" s
)

// --------------------------------------------------------------------------------------
// Run the unit tests using test runner

Target "RunTests" (fun _ ->

  let assemblies = !! unitTestAssemblies |> Seq.fold (fun acc a -> acc + " " + a) ""
  let result = 
    ExecProcess (fun info -> info.FileName <- "mono"
                             info.Arguments <- "packages/NUnit.ConsoleRunner/tools/nunit3-console.exe --labels=All --workers=1" + assemblies) (TimeSpan.FromMinutes 2.0)
  if result <> 0 then failwithf "NUnit failed"
    
)

Target "Lint" (fun _ ->
    let projects = !! "src/**/*.fsproj" ++ "tests/**/*.fsproj"
    printf "linting projects: %A" projects
    projects |> Seq.iter (FSharpLint (fun options -> {options with FailBuildIfAnyWarnings=true})))

// --------------------------------------------------------------------------------------
// Run all targets by default. Invoke 'build <Target>' to override

Target "All" DoNothing

"Clean"
  ==> "BuildRelease"
  ==> "Lint"
  ==> "RunTests"
  ==> "CopyBinaries"
  ==> "All"

RunTargetOrDefault "All"
