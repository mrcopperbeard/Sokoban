open System
open System.Collections.Generic
open Sokoban
open Sokoban.Engine
open Sokoban.Components
open Sokoban.Systems

let world = World<Systems.WorldMessage>()
let entityStore = world :> IWorld<Systems.WorldMessage>
let player = entityStore.CreateEntity { X = 0; Y = 0}

player.AddTag "Player"
player.SetComponent { Movable = true }
"~☺~" |> Schematic |> player.SetComponent

let box1 = entityStore.CreateEntity { X = 2; Y = 0}

box1.SetComponent { Movable = true }

let rec animatedBox () = seq {
    yield "[o]"
    yield "[o]"
    yield "[o]"
    yield "[-]"
    yield! animatedBox ()
}

animatedBox ()
:?> IEnumerator<string>
|> Animated
|> box1.SetComponent

let box2 = entityStore.CreateEntity { X = 0; Y = 2}

box2.SetComponent { Movable = false }
"[ ]" |> Schematic |> box2.SetComponent

let movement = MoveSystem world
let input = InputSystem world

let observableWorld = world :> IObservable<Systems.WorldMessage>

observableWorld
|> Observable.choose (fun msg ->
    match msg with Move move -> Some move
)
|> Observable.add movement.OnMove

world
:> IWorld
|> Render.console 5
|> Async.RunSynchronously