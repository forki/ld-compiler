module compiler.api.Tests.AppTests

open NUnit.Framework
open FsUnit
open Suave
open Suave.Web
open Suave.Http
open Suave.Testing
open System.Threading
open compiler.api.core.App

let startServerWith compileFn =
  runWith defaultConfig (createApp compileFn)
let req meth path testCtx = reqQuery meth path "" testCtx 

let simulateCompilationStarting () =
  setRunning true

[<SetUp>]
let setup () =
  setRunning false

[<Test>]
let ``When calling compile with missing repo url parameter should get bad request`` () =

  let compileFn _ () = ()
  let response = startServerWith compileFn |> req HttpMethod.POST "/compile"

  response |> should equal "Please provide git repo url as a querystring parameter called 'repoUrl'" 

[<Test>]
let ``When compilation has not started then check status should return not running`` () =
  let compileFn _ () = ()
  
  let response = startServerWith compileFn |> req HttpMethod.GET "/status"

  response |> should equal "Not running" 

[<Test>]
let ``When compilation is started then should immediately return ok`` () =
  let compileFn _ () = ()
  let qs = "repoUrl=http%3A%2F%2Fgithub.com%2Fnhsevidence%2Fld-dummy-content"
  let response = startServerWith compileFn |> reqQuery HttpMethod.POST "/compile" qs

  response |> should equal "Started" 

[<Test>]
let ``When compilation is started if we trigger it again then should say already running`` () =
  let compileFn _ () = ()
  let qs = "repoUrl=http%3A%2F%2Fgithub.com%2Fnhsevidence%2Fld-dummy-content"

  simulateCompilationStarting ()
  let response = startServerWith compileFn |> reqQuery HttpMethod.POST "/compile" qs

  response |> should equal "Already running" 

[<Test>]
let ``When compilation has started then check status should return running`` () =
  let compileFn _ () = ()

  simulateCompilationStarting ()
  let response = startServerWith compileFn |> req HttpMethod.GET "/status"

  response |> should equal "Running" 

[<Test>]
let ``When accesing an unknown route should return found no handlers`` () =
  let compileFn _ () = ()
  let response = startServerWith compileFn |> req HttpMethod.GET "/unknownroute"
  response |> should equal "Found no handlers"

