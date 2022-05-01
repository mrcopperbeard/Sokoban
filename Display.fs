module Display
open Domain

let display (cell : Cell) =
    match cell with
    | Floor (Some Player) | Target (Some Player) -> "~☺~"
    | Floor (Some Box) -> "[ ]"
    | Target (Some Box) -> "[X]"
    | Floor None -> "   "
    | Target None -> " X "
    | Wall -> "███"

let parseLevel (level : string list) =
    let parseRow (row : string) =
        let parseCell char =
            match char with
            | 'W' | 'w' -> Wall
            | 'X' | 'x' -> Target None
            | 'P' | 'p' -> Floor (Some Player)
            | 'B' | 'b' -> Floor (Some Box)
            | _ -> Floor None
        row
        |> Seq.map parseCell
        |> Seq.toList

    level
    |> List.map parseRow
    |> array2D