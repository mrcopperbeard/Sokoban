module Systems

open Components

let display (center: Coordinates) (distance: int32) (world: IWorld) =
    let result = System.Text.StringBuilder()
    for y in [center.Y - distance .. center.Y + distance] do
        for x in [center.X - distance .. center.X + distance] do
            { X = x; Y = y }
            |> world.GetEntity
            |> Option.bind (fun entity -> entity.GetComponent<Display> ())
            |> Option.map (fun display -> display.Schematic)
            |> Option.defaultValue "   "
            |> result.Append
            |> ignore

        result.AppendLine "" |> ignore

    result.ToString()

open System

type private Move = Up | Right | Down | Left
type private TransformUpdated = { Entity: Entity; Coordinates: Coordinates }

let input (world: IWorld) =
    let parseKey (key: ConsoleKeyInfo) =
        match key.Key with
        | ConsoleKey.W -> Some Up
        | ConsoleKey.D -> Some Right
        | ConsoleKey.S -> Some Down
        | ConsoleKey.A -> Some Left
        | _ -> None

    let move (command: Move) =
        let getNextCoordinates = 
            match command with
            | Left -> fun c -> { c with X = c.X - 1}
            | Down -> fun c -> { c with Y = c.Y + 1}
            | Right -> fun c -> { c with X = c.X + 1}
            | Up -> fun c -> { c with Y = c.Y - 1}

        let rec moveInternal coordinates commands =
            world.GetEntity coordinates
            |> function
            | Some entity ->
                let rigidbody = entity.GetComponent<Rigidbody> ()
                let coordinates = entity.GetComponent<Coordinates> ()

                match rigidbody, coordinates with
                | Some { Movable = true }, Some coordinates ->
                    let nextCoordinates = getNextCoordinates coordinates
                    let command = { 
                        Entity = entity
                        Coordinates = nextCoordinates
                    }
                    moveInternal nextCoordinates (command :: commands)
                | Some { Movable = false }, _ -> []
                | _ -> commands
            | _ -> commands

        let playerCoordinates =
            world.FindByTag "Player"
            |> Seq.tryHead
            |> Option.defaultWith (fun () -> failwith "Player not found")
            |> fun e -> e.GetComponent<Coordinates> ()
            |> Option.defaultWith (fun () -> failwith "Player coordinates not defined")

        let updateWorld command = command.Entity.SetComponent command.Coordinates

        moveInternal playerCoordinates []
        |> List.iter updateWorld

    System.Console.ReadKey true
    |> parseKey
    |> Option.map move
    |> ignore