open System
open System.Collections.Generic
open Sokoban
open Sokoban.Engine
open Sokoban.Components
open Sokoban.Systems

let consoleRender = Render.console 5 >> fun _ -> 17

let world = World<Systems.WorldMessage>(consoleRender)
let entityStore = world :> IWorld<Systems.WorldMessage>
let player = entityStore.CreateEntity { X = 0; Y = 0}

player.AddTag "Player"
player.SetComponent { Movable = true }
"~☺~" |> Schematic |> player.SetComponent

let box1 = entityStore.CreateEntity { X = 2; Y = 0}

box1.SetComponent { Movable = true }
seq {
    while true do
        for _ in 0..3 do
            yield "[o]"

        yield "[-]"
}
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

world.Update () |> Async.RunSynchronously