open System

type Cell = Player | Empty | Box | Wall
type Move = Up | Right | Down | Left
type State = { Grid: Cell[,]; PlayerCoordinates: int * int }

type Grid (grid: Cell[,]) =
    let decrement x = x - 1
    let xLength = grid |> Array2D.length1 |> decrement
    let yLength = grid |> Array2D.length2 |> decrement
    let rec getPlayerCoordinates i j =
        match i, j, grid[i,j] with
        | _, _, Player -> i, j
        | x, y, _ when x = xLength && y = yLength -> failwith "No player on field"
        | x, _, _ when x = xLength -> getPlayerCoordinates 0 (j + 1)
        | _ -> getPlayerCoordinates (i + 1) j

    let mutable playerCoordinates = getPlayerCoordinates 0 0

    member _.DisplayCoordinates() =
        let i, j = playerCoordinates
        sprintf "%d %d" i j

    member _.Display () =
        let display cell =
            match cell with
            | Player -> "~☺~"
            | Empty -> "   "
            | Box -> "[X]"
            | Wall -> "███"
        let builder = Text.StringBuilder()
        for i in [0..xLength] do
            for j in [0..yLength] do
                grid[i,j]
                |> display
                |> if j = yLength then builder.AppendLine else builder.Append
                |> ignore
        builder.ToString()

    member _.MovePlayer command =
        let getNextCoordinates = 
            match command with
            | Left -> fun (i, j) -> (i, j - 1)
            | Down -> fun (i, j) -> (i + 1, j)
            | Right -> fun (i, j) -> (i, j + 1)
            | Up -> fun (i, j) -> (i - 1, j)

        let rec move cellToPlace coordinates commands =
            let i, j = coordinates
            let currentCell = grid[i,j]
            let placeCommand = cellToPlace, coordinates
            match currentCell with
            | Wall -> []
            | Empty -> (placeCommand :: commands)
            | Box | Player ->
                let nextCoordinates = getNextCoordinates coordinates
                move currentCell nextCoordinates (placeCommand :: commands)

        let updateGrid (cell, coordinates) =
            let i, j = coordinates
            grid[i,j] <- cell
            if cell = Player then playerCoordinates <- coordinates

        move Empty playerCoordinates []
        |> List.iter updateGrid

let parseKey (key: ConsoleKeyInfo) =
    match key.Key with
    | ConsoleKey.W -> Some Up
    | ConsoleKey.D -> Some Right
    | ConsoleKey.S -> Some Down
    | ConsoleKey.A -> Some Left
    | _ -> None

let cells = array2D [
    [ Wall; Wall; Wall; Wall; Wall; Wall; Wall ]
    [ Wall; Empty; Empty; Empty; Empty; Empty; Wall ]
    [ Wall; Empty; Empty; Empty; Wall; Empty; Wall ]
    [ Wall; Empty; Box; Player; Wall; Box; Wall ]
    [ Wall; Empty; Empty; Empty; Empty; Empty; Wall ]
    [ Wall; Empty; Empty; Empty; Empty; Empty; Wall ]
    [ Wall; Wall; Wall; Wall; Wall; Wall; Wall ]
]

let grid = Grid(cells)
let display () =
    Console.Clear()
    printfn "%s" <| grid.Display()

display ()
while true do
    Console.ReadKey()
    |> parseKey
    |> function
    | Some command -> grid.MovePlayer command; display ()
    | None -> ()
