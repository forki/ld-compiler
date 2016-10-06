module compiler.ContentExtractor

open compiler.ConfigTypes
open compiler.ContentHandle

open System

type ContentExtractor = {
  readAllContentItems : unit -> ContentHandle seq
  readContentForItem : ContentHandle -> ContentHandle
  readConfig : unit -> ConfigDetails
  prepare : unit -> unit
}
