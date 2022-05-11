module Grid

open Domain
open Component
open System.Collections.Generic

type Grid(displayRadius: int) =
    let grid = Dictionary<Coordinates, Entity>()
    let get coordinates =
        match grid.TryGetValue(coordinates) with
        | false, _ -> None
        | true, entity -> Some entity

    member _.Add<'T when 'T :> Entity and 'T :> ITransform> (entity : 'T) =
        let coordinates = (entity :> ITransform).Coordinates
        grid[coordinates] <- entity

    member _.Display (center: Coordinates) =
        let display entity =
            let entity : obj = entity
            match entity with
            | :? IPrintable as printable -> Some <| printable.Display()
            | _ -> None

        let centerX, centerY = center
        let result = System.Text.StringBuilder()
        for x in [centerX - displayRadius .. centerX + displayRadius] do
            for y in [centerY - displayRadius .. centerY + displayRadius] do
                (x, y)
                |> Coordinates
                |> get
                |> Option.bind display
                |> Option.defaultValue "   "
                |> result.Append
                |> ignore

            result.AppendLine "" |> ignore

        result.ToString()