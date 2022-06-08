namespace Sokoban

module Systems =
    open System
    open Components
    open Sokoban.Engine

    type Move = Up | Right | Down | Left

    type WorldMessage =
    | Move of Move

    type private TransformUpdated = { Entity: IEntity; Coordinates: Coordinates }

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

            let playerCoordinates = Utils.getPlayerCoordinates world
            let updateWorld command = command.Entity.SetComponent command.Coordinates

            moveInternal playerCoordinates []
            |> List.iter updateWorld

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