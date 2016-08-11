module compiler.YamlParser

open System.Text.RegularExpressions

type Section = {
  Name : string    
  Fields : string list
}

type Field = {
  ParentSection: string
  Field: string
}

type AccumulatedFields = {
  CurrentSection : string
  Fields : Field list
}

let private extractFieldFrom (line:string) =
  let m = Regex.Match(line,"\"(.*)\"") 
  m.Groups.[1].Value

let private newField (sectionName:string) line =
  {ParentSection = sectionName.Replace(":",""); Field = extractFieldFrom line}

let private isEmptyField {ParentSection = _; Field = field} = field <> ""

let private section {ParentSection = s; Field = _} = s 

let private createSection (name, fields) =
 {Name = name
  Fields = fields
          |> Seq.toList
          |> List.map (fun x -> x.Field)
          |> List.rev}

let private accumulateFields acc (line:string) =
  let (|IsSection|_|) (s:string) = if s.Contains ":" then Some s else None
  let (|IsField|_|) (s:string) = if s.Contains "-" then Some s else None

  match line with
  | IsSection line -> {acc with CurrentSection = line}
  | IsField line -> {acc with Fields = newField acc.CurrentSection line :: acc.Fields} 
  | _ -> acc

let parseYaml (yaml:string) =
  let lines = yaml.Replace(System.Environment.NewLine,"\n").Split '\n'
  
  let accumulatedFields = 
    lines
    |> Seq.fold accumulateFields {CurrentSection = ""; Fields = []}

  accumulatedFields.Fields
  |> Seq.filter isEmptyField
  |> Seq.groupBy section
  |> Seq.map createSection
  |> Seq.toList
  |> List.rev

