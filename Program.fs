open System
open Sokoban
open Sokoban.Components
open Systems

printfn "%s: Sokoban started" <| DateTime.Now.ToShortTimeString()

let world = World<Systems.WorldMessage>()

let entityStore = world :> IWorld<Systems.WorldMessage>

let player = entityStore.CreateEntity { X = 0; Y = 0}

player.AddTag "Player"
player.SetComponent { Movable = true }
player.SetComponent { Schematic = "~☺~" }

let box1 = entityStore.CreateEntity { X = 2; Y = 0}

box1.SetComponent { Movable = true }
box1.SetComponent { Schematic = "[ ]" }

let box2 = entityStore.CreateEntity { X = 0; Y = 2}

box2.SetComponent { Movable = false }
box2.SetComponent { Schematic = "[-]" }

let display = ConsoleDisplaySystem world
let movement = MoveSystem world
let input = InputSystem world

let observableWorld = world :> IObservable<Systems.WorldMessage>

observableWorld
|> Observable.choose (fun msg ->
    match msg with Move move -> Some move | _ -> None
)
|> Observable.add movement.OnMove

observableWorld
|> Observable.choose (fun msg ->
    match msg with Display display -> Some display | _ -> None
)
|> Observable.add display.OnDisplay

while true do Async.Sleep 200 |> Async.RunSynchronously