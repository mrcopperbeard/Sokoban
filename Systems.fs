module Systems

open Components

let display (center: Coordinates) (radius: int32) (world: IWorld) =
    let centerX, centerY = center
    let result = System.Text.StringBuilder()
    for x in [centerX - radius .. centerX + radius] do
        for y in [centerY - radius .. centerY + radius] do
            (x, y)
            |> Coordinates
            |> world.GetEntity
            |> Option.bind (fun entity -> entity.Component.Display)
            |> Option.defaultValue "   "
            |> result.Append
            |> ignore

        result.AppendLine "" |> ignore

    result.ToString()