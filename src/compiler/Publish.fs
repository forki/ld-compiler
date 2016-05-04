module compiler.Publish

open compiler.JsonLd
open compiler.Elastic
open compiler.Stardog

let publish propertyPaths contexts indexName typeName =
  printf "Publishing resources\n"

  Stardog.extractResources propertyPaths
  |> transformToJsonLD contexts
  |> bulkUpload indexName typeName
