module compiler.api.core.App

open Suave
open Suave.Filters
open Suave.Operators
open Suave.Files
open System.Threading

let r = ref false

let isRunning () = !r

let setRunning running = r := running

let runCompile compileFn: WebPart =
 fun (x : HttpContext) ->
   async {
       setRunning true
       printf "starting compiling...\n" 
       do! compileFn ()
       setRunning false
       printf "finished\n"
       return! Successful.ACCEPTED "OK" x
   }

let checkStatus () : WebPart =
  fun (x : HttpContext) ->
    async {
      let running = 
        match isRunning() with
        | true -> "Running"
        | false -> "Not running"
      return! Successful.OK running x
    }

let createApp compileFn =
  choose
    [POST >=> path "/compile" >=> runCompile compileFn
     GET >=> path "/status" >=> checkStatus ()
     RequestErrors.NOT_FOUND "Found no handlers"]
