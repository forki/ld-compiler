module compiler.Test.YamlParserTests

open NUnit.Framework
open FsUnit
open compiler.YamlParser

[<Test>]
let ``YamlParserTests: Should handle no empty string`` () =
  parseYaml "" |> should equal []

[<Test>]
let ``YamlParserTests: Should extract single section with single field`` () =
  let yaml = """
This Section:
  - "Field"
"""

  let expected = [{Name = "This Section"; Fields = ["Field"]}]
  let actual = parseYaml yaml
  expected |> should equal actual

[<Test>]
let ``YamlParserTests: Should extract single section with multiple fields`` () =
  let yaml = """
Section:
    - "Field1"
    - "Field2"
"""

  let expected = [{Name = "Section"; Fields = ["Field1"; "Field2"]}]
  let actual = parseYaml yaml
  expected |> should equal actual

[<Test>]
let ``YamlParserTests: Should extract multiple sections`` () =
  let yaml = """
Section1:
    - "Field"
Section2:
    - "Field"
"""

  let expected = [{Name = "Section1"; Fields = ["Field"]}
                  {Name = "Section2"; Fields = ["Field"]}]
  let actual = parseYaml yaml
  expected |> should equal actual
