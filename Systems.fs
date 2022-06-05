namespace Sokoban

module Systems =
    open System
    open Components
    open Sokoban

    type DisplayCommand = { Center: Coordinates; Distance: int }
    type Move = Up | Right | Down | Left

    type WorldMessage =
    | Display of DisplayCommand
    | Move of Move

    type ConsoleDisplaySystem(world: IWorld<WorldMessage>) =
        let display command =
            let center = command.Center
            let distance = command.Distance 
            let result = Text.StringBuilder()
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

        let toConsole (str : string) =
            Console.Clear()
            Console.WriteLine str

        let mailbox = MailboxProcessor.Start(fun inbox ->
            let rec loop () = async {
                let! message = inbox.Receive()
                message
                |> (display >> toConsole)

                return! loop()
            }
            loop()
        )

        member _.OnDisplay (command : DisplayCommand) = mailbox.Post command

    type private TransformUpdated = { Entity: Entity; Coordinates: Coordinates }

    type MoveSystem(world: IWorld<WorldMessage>) =
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

            let commands = moveInternal playerCoordinates []
            commands |> List.iter updateWorld

            if commands |> List.isEmpty |> not then
                world.SendMessage <| Display { Center = playerCoordinates; Distance = 5 }

        let mailbox = MailboxProcessor.Start (fun inbox -> 
            let rec loop () = async {
                let! message = inbox.Receive()
                move message
                return! loop()
            }

            loop ()
        )

        member _.OnMove (move: Move) = mailbox.Post move

    type InputSystem(world : IWorld<WorldMessage>) =
        let parseKey (key: ConsoleKeyInfo) =
            match key.Key with
            | ConsoleKey.W -> Some Up
            | ConsoleKey.D -> Some Right
            | ConsoleKey.S -> Some Down
            | ConsoleKey.A -> Some Left
            | _ -> None

        let input _ =
            let rec loop () = async {
                System.Console.ReadKey true
                |> parseKey
                |> Option.map (fun move -> Move move)
                |> Option.map world.SendMessage
                |> ignore

                return! loop ()
            }

            loop ()

        let _ = MailboxProcessor.Start input