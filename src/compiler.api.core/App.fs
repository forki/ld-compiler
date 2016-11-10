module compiler.api.core.App

open Serilog
open NICE.Logging
open Suave
open Suave.Filters
open Suave.Operators
open Suave.Files
open System.Threading
open compiler.Pandoc
open compiler.ContentHandle
// high level exception handler

let r = ref false

let private isRunning () = !r

// Public as used in AppTests.fs
let setRunning running = r := running

let private asyncCompile compileFn =
  Async.Start(
    async {
      setRunning true
      Log.Information "Started compiling..."
      try compileFn () with ex -> Log.Error (ex.ToString())
      setRunning false
      Log.Information "Finished compiling!"
  })

let private runCompile compileFn: WebPart =
 fun (x : HttpContext) ->
   match isRunning() with
   | true ->
     Successful.ACCEPTED "Already running" x
   | false ->
     asyncCompile compileFn
     Successful.ACCEPTED "Started" x

let private checkStatus () : WebPart =
  fun (x : HttpContext) ->
    async {
      let running = 
        match isRunning() with
        | true -> "Running"
        | false -> "Not running"
      return! Successful.OK running x
    }

let private convertMDToHTML () : WebPart =
  fun (x : HttpContext) ->
    async {
      let markdown = match x.request.formData "markdown" with
                     | Choice1Of2 t -> t
                     | _ -> ""
      let content = {Thing=""; Content = markdown}
      let converted = convertMarkdownToHtml content
      return! Successful.OK (snd converted) x
    }

let createApp compileFn =
  choose
    [POST >=> path "/compile" >=>
       request (fun req ->
                match req.queryParam "repoUrl" with
                | Choice1Of2 repoUrl when repoUrl <> "" -> runCompile (compileFn repoUrl)
                | _ -> RequestErrors.BAD_REQUEST "Please provide git repo url as a querystring parameter called 'repoUrl'")
     GET >=> path "/status" >=> checkStatus ()
     POST >=> path "/convert" >=> convertMDToHTML()
     RequestErrors.NOT_FOUND "Found no handlers"]
