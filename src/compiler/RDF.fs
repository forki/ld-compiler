module publish.RDF

open publish.Domain
open FSharp.RDF
open Assertion
open resource
open rdf

let transformToRDF baseUrl statement =
  let uri = sprintf "%s/%s" baseUrl statement.Id

  resource !! uri
    [a !! "owl:NamedIndividual"
     dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#title" (statement.Title^^xsd.string)
     dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#abstract" (statement.Abstract^^xsd.string)
     dataProperty !!"http://www.w3.org/2011/content#chars" (statement.Content^^xsd.string)
     dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#stidentifier" (statement.StatementId^^xsd.integer)
     dataProperty !!"http://ld.nice.org.uk/ns/qualitystandard#qsidentifier" (statement.StandardId^^xsd.integer)]
  
