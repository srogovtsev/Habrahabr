module Generators

open System
open System.IO

let rand = Random()

let generateAnts bound writer =
    let antsByType =
        seq {0..255}
        |> Seq.mapFold (fun s i -> 
            let q =
                //randomly chosen
                match rand.Next(0, bound/256*2) with
                | v when v >= 0 -> v
                | _ -> 0
            
            if i = 255 then
                (s, 0)
            elif (s < q) then
                (s, 0)
            else
                (q, s-q)
        ) bound
        |> fst
        |> Seq.map (fun i -> Array.init i (fun _ -> Guid.NewGuid()))
        |> Seq.toArray

    writer (fun w ->
        antsByType
        |> Array.iteri (fun t ids ->
            ids |> Array.iter (fun id -> w id (t |> byte))
        )
    )

    antsByType |> Seq.concat

let generateCells bound writer = 
    let cells = Array.init bound (fun _ -> Guid.NewGuid())
    
    writer (fun w ->
        cells
        |> Seq.sort
        |> Seq.iter (fun id -> w id (rand.Next(0, 256) |> byte))
    )

    cells

