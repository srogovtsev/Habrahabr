open System

let generate bound outDir =
    ignore 0

[<EntryPoint>]
let main argv = 
    try
        let defBound = 1000
        let defOut = System.Environment.CurrentDirectory
        match argv with
        | [||] -> generate defBound defOut
        | [|v|] ->
            match Int32.TryParse v with
            | (true, bound) -> generate bound defOut
            | (false, dir) -> generate defBound dir
        | [|v;dir|] -> generate (Int32.Parse v) dir
        | _ -> invalidArg "argv" "Too many arguments"
    with
    | e -> printfn "%A" e
    0