open System
open System.IO

open Generators

let generate bound outDir =
    if not(Directory.Exists(outDir)) then invalidArg "outDir" "Directory does not exist"
    printfn "Generating data with upper bound %i to %s" bound outDir

    let generateFile file (writer: TextWriter -> unit) =
        using(new FileStream(Path.Combine(outDir, file), FileMode.Create, FileAccess.Write)) (fun fs ->
            using(new StreamWriter(fs)) writer
        )

    let generateFileByLine file writer =
        generateFile file (fun sw -> writer (fun (s1: string, s2: string) -> sw.Write(s1); sw.Write('\t'); sw.WriteLine(s2)))

    let generateFileGuidToByte file writer =
        generateFileByLine file (fun w -> writer (fun (id: Guid) (t: byte) -> w(id.ToString("N"), t.ToString("D3"))))

    let sw = System.Diagnostics.Stopwatch();
    let st =
        sw.Start
    let ts s =
        sw.Stop()
        printfn "%s in %s" s (sw.Elapsed.ToString "mm\:ss\.fff")
        sw.Reset()

    st()
    let ants = generateAnts bound ("ants" |> generateFileGuidToByte)
    ts "Ants generated"
    st()
    let cells = generateCells bound ("cells" |> generateFileGuidToByte)
    ts "Cells generated"
    st()
    generateFileByLine "antsToCells" (fun w -> 
        ants
        |> Seq.collect (fun ant -> Seq.init (rand.Next(1, 3)) (fun _ -> (ant, cells.[rand.Next(bound)])))
        |> Seq.take bound
        |> Seq.sortBy snd
        |> Seq.iter (fun (ant, cell) -> w (ant.ToString("N"), cell.ToString("N")))
    )
    ts "Links generated"

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
            | (false, _) -> generate defBound v
        | [|v;dir|] -> generate (Int32.Parse v) dir
        | _ -> invalidArg "argv" "Too many arguments"
    with
    | e -> printfn "%A" e
    0