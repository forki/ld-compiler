module compiler.api.Tests.AppTests

open NUnit.Framework
open Swensen.Unquote
open Suave
open Suave.Web
open Suave.Http
open Suave.Testing
open System.Threading
open compiler.api.core.App

let startServerWith compileFn =
  runWith defaultConfig (createApp compileFn)
let get path testCtx = reqQuery HttpMethod.GET path "" testCtx 
let post path testCtx = reqQuery HttpMethod.POST path "" testCtx 
let getQuery path qs testCtx = reqQuery HttpMethod.GET path qs testCtx 

let startCompilation () =
  setRunning true

[<SetUp>]
let setup () =
  setRunning false

[<Test>]
let ``When compilation has not started then check status should return not running`` () =
  let compileFn () = ()
  
  let response = startServerWith compileFn |> get "/status"

  test <@ response = "Not running" @> 

[<Test>]
let ``When compilation is started then should immediately return ok`` () =
  let compileFn () = ()

  let response = startServerWith compileFn |> post "/compile"

  test <@ response = "Started" @> 

[<Test>]
let ``When compilation is started if we trigger it again then should say already running`` () =
  let compileFn () = ()

  startCompilation ()
  let response = startServerWith compileFn |> post "/compile"

  test <@ response = "Already running" @> 

[<Test>]
let ``When compilation has started then check status should return running`` () =
  let compileFn () = ()

  startCompilation ()
  let response = startServerWith compileFn |> get "/status"

  test <@ response = "Running" @> 

[<Test>]
let ``When accesing an unknown route should return found no handlers`` () =
  test <@ startServerWith (fun () -> ()) |> get "/unknownroute" = "Found no handlers" @>
