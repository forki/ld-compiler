module compiler.api.App

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Files
open System.Threading
open System.IO

let file = "compiler.running"

let isRunning () =
  let running =
    try
      let content = File.ReadAllText(file)
      printf "content: %s\n" content
      content.Contains("True")
    with _ -> false
  printf "isRunning %b\n" running
  running

let setRunning running =
  File.WriteAllText(file,running.ToString())

let compile milliseconds message: WebPart =
 fun (x : HttpContext) ->
   async {
     setRunning true
     printf "starting sleeping with running %b\n" ( isRunning() )
     do! Async.Sleep milliseconds
     setRunning false
     printf "finished sleeping with running %b\n" ( isRunning() )
     return! Successful.OK message x
   }

let checkStatus () : WebPart =
  fun (x : HttpContext) ->
    async {
      let running = 
        match isRunning() with
        | true -> "Running"
        | false -> "Finished"
      printf "checkStatus: %s\n" running
      return! Successful.OK running x
    }

let createApp compileFn =
  choose
    [GET >=> path "/compile" >=> compile 10000 "Done"
     GET >=> path "/status" >=> checkStatus ()
     RequestErrors.NOT_FOUND "Found no handlers"]
