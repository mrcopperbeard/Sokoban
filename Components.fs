module Components

type Coordinates = int * int
type Display = string
type Rigidbody = {
    Movable: bool
}

type Component = {
    Transform: Coordinates option
    Rigidbody: Rigidbody option
    Display: Display option
}
with
    static member create () = {
        Transform = None
        Rigidbody = None
        Display = None
    }

type Entity = {
    Id: int64
    Component: Component
}

open System.Collections.Generic

let private get<'TKey, 'TValue> (dict : Dictionary<'TKey, 'TValue>) key =
    match dict.TryGetValue(key) with
    | false, _ -> None
    | true, entity -> Some entity

type IWorld =
    abstract member AddEntity: Component -> unit
    abstract member UpdateEntity: Entity -> unit
    abstract member GetEntity: Coordinates -> Entity option

type World() =
    let mutable id = 0L
    let entities = Dictionary<int64, Entity>()
    let grid = Dictionary<Coordinates, int64>()
    let mutable systems = []

    member _.AddSystem (sys : IWorld -> unit) = systems <- sys :: systems
    member this.Update() =
        let world = this :> IWorld
        systems
        |> List.iter (fun sys -> sys world)

    interface IWorld with
        member _.UpdateEntity entity =
            entities[entity.Id] <- entity
            match entity.Component.Transform with
            | Some coordinates -> grid[coordinates] <- entity.Id
            | None -> ()

        member this.AddEntity cmpt =
            id <- id + 1L
            let world = this :> IWorld
            world.UpdateEntity { Id = id; Component = cmpt }

        member _.GetEntity coordinates =
            coordinates
            |> get grid
            |> Option.bind (get entities)

