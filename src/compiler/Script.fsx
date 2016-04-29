open System
open System.Threading

let response =
  async {
    printf "starting compiling...\n" 
    Thread.Sleep(2000)
    printf "finished\n"
  }
  
Async.Start response
printf "OK" 
