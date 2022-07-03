namespace Sokoban

module Render =
    open System
    open Sokoban.Engine
    open Sokoban.Components

    let [<Literal>] private placeholder = "   "
    let private tileWidth = placeholder |> String.length

    type private State = {
        Tiles: (int * int * string) list
    }

    let private draw display =
        match display with
        | Schematic str -> Some str
        | Animated iterator ->
            if iterator.MoveNext() then Some iterator.Current
            else None

    let private getCellString (world : IWorld) coords =
        coords
        |> world.GetEntity
        |> Option.bind (fun entity -> entity.GetComponent<Display> ())
        |> Option.bind draw

    let console distance (world : IWorld) = async {
        Console.CursorVisible <- true
        Console.Clear()

        let maxLen = distance * 2 - 1

        let writeToConsole x y (s : string) =
            try
                Console.SetCursorPosition (x * tileWidth, y)
                Console.Write s
            with | :? ArgumentOutOfRangeException -> failwithf "X: %d, Y: %d" x y

        let rec getDifference (oldTiles : (int * int * string) list ) (newTiles : (int * int * string) list ) = seq {
            match oldTiles, newTiles with
            | [], [] -> ()
            | [], newer -> yield! seq { for (x, y, n) in newer -> x, y, n }
            | older, [] -> yield! seq { for (x, y, _) in older -> x, y, placeholder }
            | (oX, oY, oldValue) :: oldRemained, (nX, nY, newValue) :: newRemained ->
                let o = oY * maxLen + oX
                let n = nY * maxLen + nX
                if o < n then
                    yield oX, oY, placeholder
                    yield! getDifference oldRemained newTiles
                elif o > n then
                    yield nX, nY, newValue
                    yield! getDifference oldTiles newRemained
                elif oldValue = newValue then
                    yield! getDifference oldRemained newRemained
                else
                    yield nX, nY, newValue
                    yield! getDifference oldRemained newRemained
        }

        let getTiles () =
            let center = Utils.getPlayerCoordinates world
            [0..maxLen]
            |> List.allPairs [0..maxLen]
            |> List.choose (fun (x, y) ->
                { X = x + center.X; Y = y + center.Y }
                |> getCellString world
                |> Option.map (fun value -> x, y, value)
            )

        let rec loop oldState = async {
            let newState = { Tiles = getTiles () }

            getDifference oldState.Tiles newState.Tiles
            |> Seq.iter (fun (x, y, value) -> writeToConsole x y value)

            do! Async.Sleep 160
            return! loop newState
        }

        do! loop { Tiles = getTiles () }
    }
