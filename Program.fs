open System

open Domain

type Move = Up | Right | Down | Left

type Grid (grid: Cell[,]) =
    let decrement x = x - 1
    let xLength = grid |> Array2D.length1 |> decrement
    let yLength = grid |> Array2D.length2 |> decrement
    let rec getPlayerCoordinates i j =
        match i, j, grid[i,j] with
        | _, _, Floor (Some Player) | _, _, Target (Some Player) -> i, j
        | x, y, _ when x = xLength && y = yLength -> failwith "No player on field"
        | x, _, _ when x = xLength -> getPlayerCoordinates 0 (j + 1)
        | _ -> getPlayerCoordinates (i + 1) j

    let winCheck () =
        let rec winCheckInternal i j =
            match i, j, grid[i,j] with
            | _, _, Floor (Some Box) -> false
            | x, y, _ when x = xLength && y = yLength -> true
            | x, _, _ when x = xLength -> winCheckInternal 0 (j + 1)
            | _ -> winCheckInternal (i + 1) j

        winCheckInternal 0 0

    let mutable playerCoordinates = getPlayerCoordinates 0 0

    member _.DisplayCoordinates() =
        let i, j = playerCoordinates
        sprintf "%d %d" i j

    member _.Display () =
        let builder = Text.StringBuilder()
        for i in [0..xLength] do
            for j in [0..yLength] do
                grid[i,j]
                |> Display.display
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

        let rec move (cellToPlace : GameObject option) coordinates commands =
            let i, j = coordinates
            let currentCell = grid[i,j]
            let placeCommand = cellToPlace, coordinates
            let moveFurther obj =
                let nextCoordinates = getNextCoordinates coordinates
                move obj nextCoordinates (placeCommand :: commands)

            match currentCell with
            | Wall -> []
            | Floor None | Target None -> (placeCommand :: commands)
            | Floor someObj-> moveFurther someObj
            | Target someObj-> moveFurther someObj

        let updateGrid (obj: GameObject option, coordinates) =
            let i, j = coordinates
            grid[i,j] <- Cell.setObj grid[i,j] obj
            if obj = Some Player then playerCoordinates <- coordinates

        move None playerCoordinates []
        |> List.iter updateGrid

        winCheck ()

let parseKey (key: ConsoleKeyInfo) =
    match key.Key with
    | ConsoleKey.W -> Some Up
    | ConsoleKey.D -> Some Right
    | ConsoleKey.S -> Some Down
    | ConsoleKey.A -> Some Left
    | _ -> None

let runLevel levelNumber cells =
    let grid = Grid(cells)
    let display () =
        Console.Clear()
        printfn "Level %d" <| levelNumber + 1
        printfn "%s" <| grid.Display()

    display()
    let mutable complete = false
    while not complete do
        Console.ReadKey()
        |> parseKey
        |> function
        | Some command -> complete <- grid.MovePlayer command; display()
        | None -> ()

let cells = seq {
    yield [
        "WWWWW"
        "WpbxW"
        "WWWWW"
    ]
    yield [
        "WWWWWWW"
        "W     W"
        "W     W"
        "W bwx W"
        "Wxp b W"
        "WWWWWWW"
    ]
}

cells
|> Seq.map Display.parseLevel
|> Seq.iteri runLevel

printfn "You win!"