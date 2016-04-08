module publish.Tests.Tests

open publish
open NUnit.Framework
open Swensen.Unquote
open FSharp.RDF
open Assertion
open resource
open rdf

//[<Test>]
//let ``Should convert a markdown file to RDF`` () =
//  let file = { 
//     Id = "qs1/st1"
//     Content = "This is a markdown file"
//  }
//
//  let expected =
//    resource !!"http://ld.nice.org.uk/qualitystatements/qs1/st1"
//      [dataProperty !!"https://ld.nice.org.uk/qualitystatements/content" ("This is a markdown file"^^xsd.string)]
//
//  let actual = markdownToRDF file
//  test <@ actual = expected @>

