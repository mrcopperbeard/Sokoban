namespace Sokoban

module Render =
    open System
    open Sokoban.Engine
    open Sokoban.Components

    let private draw display =
        match display with
        | Schematic str -> Some str
        | Animated iterator ->
            if iterator.MoveNext() then Some iterator.Current
            else None

    let private worldToString distance (world : IWorld) =
        let center = Utils.getPlayerCoordinates world
        let result = Text.StringBuilder()
        for y in [center.Y - distance .. center.Y + distance] do
            for x in [center.X - distance .. center.X + distance] do
                { X = x; Y = y }
                |> world.GetEntity
                |> Option.bind (fun entity -> entity.GetComponent<Display> ())
                |> Option.bind draw
                |> Option.defaultValue "   "
                |> result.Append
                |> ignore

            result.AppendLine "" |> ignore

        result.ToString()

    let private toConsole (str : string) =
        Console.Clear()
        Console.WriteLine str

    let console distance = worldToString distance >> toConsole
    