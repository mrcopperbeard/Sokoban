open System
open Components

let world = World()

module Factory =
    let player coordinates = {
        Transform = Some coordinates
        Rigidbody = Some { Movable = true }
        Display = Some "~☺~"
    }

    let box coordinates = {
        Transform = Some coordinates
        Rigidbody = Some { Movable = true }
        Display = Some "[ ]"
    }

    let wall coordinates = {
        Transform = Some coordinates
        Rigidbody = Some { Movable = true }
        Display = Some "███"
    }

    let spot coordinates = {
        Transform = Some coordinates
        Rigidbody = None
        Display = Some " X "
    }

let entityStore = world :> IWorld

[
    Factory.player (0, 0)
    Factory.box (0, 1)
    Factory.box (1, 1)
    Factory.spot (2, 2)
    Factory.wall (-1, 1)
]
|> List.iter entityStore.AddEntity

let toConsole (str : string) =
    Console.Clear()
    Console.WriteLine str

let display = Systems.display (0, 0) 5 >> toConsole

world.AddSystem display

world.Update()