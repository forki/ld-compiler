module compiler.FilteringUtils

open compiler.Domain

let filterOutUndiscoverableStatements config theseStatements =
  theseStatements