module compiler.Test.YamlParserTests

open NUnit.Framework
open Swensen.Unquote
open compiler.YamlParser

[<Test>]
let ``Should handle no empty string`` () =
  test <@ parseYaml "" = [] @>

[<Test>]
let ``Should extract single section with single field`` () =
  let yaml = """
Section:
  - "Field"
"""

  let expected = [{Name = "Section"; Fields = ["Field"]}]
  let actual = parseYaml yaml
  test <@ expected = actual @>

[<Test>]
let ``Should extract single section with multiple fields`` () =
  let yaml = """
Section:
    - "Field1"
    - "Field2"
"""

  let expected = [{Name = "Section"; Fields = ["Field1"; "Field2"]}]
  let actual = parseYaml yaml
  test <@ expected = actual @>

[<Test>]
let ``Should extract multiple sections`` () =
  let yaml = """
Section1:
    - "Field"
Section2:
    - "Field"
"""

  let expected = [{Name = "Section1"; Fields = ["Field"]}
                  {Name = "Section2"; Fields = ["Field"]}]
  let actual = parseYaml yaml
  test <@ expected = actual @>
