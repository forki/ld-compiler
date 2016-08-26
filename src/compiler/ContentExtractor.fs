module compiler.ContentExtractor

open compiler.ContentHandle
open compiler.ConfigTypes
open System

type ContentExtractor = {
  readAllContentItems : unit -> ContentHandle seq
  readContentForItem : ContentHandle -> ContentHandle
  readConfig : unit -> Config
  prepare : unit -> unit
}
