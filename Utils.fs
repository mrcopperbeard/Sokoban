namespace Sokoban

// TODO: Remove it
module Utils =
    open Sokoban.Engine

    let getPlayerCoordinates (world : IWorld) =
        world.FindByTag "Player"
        |> Seq.tryHead
        |> Option.defaultWith (fun () -> failwith "Player not found")
        |> fun e -> e.GetComponent<Coordinates> ()
        |> Option.defaultWith (fun () -> failwith "Player coordinates not defined")