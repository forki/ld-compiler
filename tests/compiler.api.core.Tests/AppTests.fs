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
    runWith defaultConfig (compiler.api.core.App.createApp compileFn)
let get path testCtx = reqQuery HttpMethod.GET path "" testCtx 
let post path testCtx = reqQuery HttpMethod.POST path "" testCtx 
let getQuery path qs testCtx = reqQuery HttpMethod.GET path qs testCtx 

[<Test>]
let ``When compilation has not started then check status should return not running`` () =
  let compileFn () = Async.Sleep(1)
  
  let response = startServerWith compileFn |> get "/status"

  test <@ response = "Not running" @> 

[<Test>]
let ``When compilation is started then should immediately return ok`` () =
  let compileFn () = Async.Sleep(2000)

  let response = startServerWith compileFn |> post "/compile"
  Thread.Sleep(2000)

  test <@ response = "OK" @> 

[<Test>]
let ``When compilation has started then check status should return running`` () =
  let ms = 1000
  let compileFn () = Async.Sleep(ms)

  startServerWith compileFn |> post "/compile" |> ignore
  let response = startServerWith compileFn |> get "/status"
  Thread.Sleep(ms * 2)

  test <@ response = "Running" @> 

[<Test>]
let ``When accesing an unknown route should return found no handlers`` () =
  test <@ startServerWith (fun () -> Async.Sleep(1)) |> get "/unknownroute" = "Found no handlers" @>
