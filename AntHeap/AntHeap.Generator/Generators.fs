module Generators

open System
open System.IO

let rand = Random()

let private b = rand.Next((int)Int16.MaxValue) |> int16
let private c = rand.Next((int)Int16.MaxValue) |> int16
let private d = Array.zeroCreate 8
rand.NextBytes d

let guidFromCounter i = new Guid(i, b, c, d)

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
    writer (fun w ->
        seq{0..bound-1}
        |> Seq.iter (fun i -> w (i |> guidFromCounter) (rand.Next(0, 256) |> byte))
    )
