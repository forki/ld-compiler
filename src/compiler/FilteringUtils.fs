module compiler.FilteringUtils

open compiler.Domain
open compiler.ConfigTypes

let private isAnnotationUndiscoverable (label, value) theseUndiscoverables =
  theseUndiscoverables
  |> List.contains { UndiscoverableLabel = label;  AnnotationValue = value}

let private searchTermsForUndiscoverables theseUndiscoverables thisAnnotation =
  thisAnnotation.Terms
  |> List.map (fun t -> isAnnotationUndiscoverable (thisAnnotation.Vocab, t) theseUndiscoverables)
  |> List.contains true

let private searchAnnotationsForUndiscoverables theseUndiscoverables theseAnnotations =
  theseAnnotations
  |> List.map (fun a -> searchTermsForUndiscoverables theseUndiscoverables a)
  |> List.contains true

let isStatementUndiscoverable (theseUndiscoverables:UndiscoverableItem List) (thisStatement:Statement) =
  let isUndiscoverable = thisStatement.Annotations
                       |> searchAnnotationsForUndiscoverables theseUndiscoverables

  (thisStatement, isUndiscoverable)
