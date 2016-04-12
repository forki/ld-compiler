module publish.Stardog

open System.Diagnostics
open System.IO

let write ttl =
  File.WriteAllText("$ARTIFACTS_DIR/output.ttl", ttl) 
  let proc = Process.Start("addgraph --named-graph http://ld.nice.org.uk/ $ARTIFACTS_DIR/output.ttl")
  let timeout = 10000

  proc.WaitForExit(timeout) |> ignore
  proc.ExitCode = 0
