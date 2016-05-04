module compiler.ContentExtractor

open compiler.ContentHandle
open System

type ContentExtractor = {
  readAllContentItems : unit -> ContentHandle seq
  readContentForItem : ContentHandle -> ContentHandle
}
