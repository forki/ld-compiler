module compiler.BindDataToHtml

open compiler.Domain

let bindDataToHtml statement =

  let newHtml = "test"
  { statement with Html = newHtml }
