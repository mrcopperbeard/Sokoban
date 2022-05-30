open System
open Components

printfn "%s: Sokoban started" <| DateTime.Now.ToShortTimeString()

let world = World()

let entityStore = world :> IWorld

let player = entityStore.CreateEntity { X = 0; Y = 0}

player.AddTag "Player"
player.SetComponent { Movable = true }
player.SetComponent { Schematic = "~☺~" }

let box = entityStore.CreateEntity { X = 2; Y = 0}

box.SetComponent { Movable = true }
box.SetComponent { Schematic = "[ ]" }

let toConsole (str : string) =
    Console.Clear()
    Console.WriteLine str

let display = Systems.display { X = 0; Y = 0 } 5 >> toConsole

world.AddSystem display
world.AddSystem Systems.input

while true do
    world.Update()