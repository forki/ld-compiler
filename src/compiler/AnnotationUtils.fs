module compiler.AnnotationUtils

open compiler.Domain
open compiler.ConfigTypes

let private getAnnotationDisplayDetails thisDisplayItem =
  match obj.ReferenceEquals(thisDisplayItem.Display, null) with
  | true -> false,null,null
  | _ -> thisDisplayItem.Display.Always,thisDisplayItem.Display.Label,thisDisplayItem.Display.Template

let private constructAnnotationWithConfig thisAnnotation thisAnnotationConfig =
  let isDisplayed, label, template = getAnnotationDisplayDetails thisAnnotationConfig

  { thisAnnotation with
      Format = thisAnnotationConfig.Format
      Uri = thisAnnotationConfig.Uri
      IsDataAnnotation = thisAnnotationConfig.DataAnnotation
      IsValidated = thisAnnotationConfig.Validate
      UndiscoverableWhen = thisAnnotationConfig.UndiscoverableWhen
      IsDisplayed = isDisplayed
      DisplayLabel = label
      DisplayTemplate = template
  }

let addConfigToAnnotation annotationConfig thisAnnotation =
  (* printf "this annotation config %A %A" annotationConfig thisAnnotation.Vocab*)
  let thisAnnotationConfig = annotationConfig
                             |> List.filter (fun c -> c.Uri = thisAnnotation.Vocab)
                             |> List.tryHead
  match thisAnnotationConfig.IsSome with
  | false -> thisAnnotation
  | _ -> constructAnnotationWithConfig thisAnnotation thisAnnotationConfig.Value

let addUriToAnnotation propertyBaseUrl thisAnnotation =
  match thisAnnotation.IsValidated with
  | true -> { thisAnnotation with Uri = sprintf "%s%s" propertyBaseUrl thisAnnotation.Uri }
  | _ -> thisAnnotation
