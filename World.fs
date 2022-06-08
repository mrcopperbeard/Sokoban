namespace Sokoban.Engine

open System
open System.Collections.Generic

type World(render: IWorld -> int) =
    let mutable id = 0L
    let entities = Dictionary<int64, IEntity>()
    let grid = Dictionary<(int * int), int64>()
    let mutable systems = []
    member _.AddSystem (sys : IWorld -> unit) = systems <- sys :: systems

    member this.Update() = async {
        while true do
            let world = this :> IWorld
            systems |> List.iter (fun sys -> sys world)
            let sleepDuration = render world
            do! Async.Sleep sleepDuration
    }

    member private _.OnEntityUpdated (updateEvent: EntityUpdated) =
        let updateGrid () =
            match updateEvent with
            | ComponentAdded added ->
                match added.Component with
                | :? Coordinates as coords -> grid[(coords.X, coords.Y)] <- added.EntityId
                | _ -> ()
            | ComponentRemoved removed ->
                match removed.Component with
                | :? Coordinates as coords -> grid.Remove ((coords.X, coords.Y)) |> ignore
                | _ -> ()

        updateGrid ()

    interface IWorld with
        member _.UpdateEntity entity = entities[entity.Id] <- entity

        member this.CreateEntity coordinates =
            id <- id + 1L
            let world = this :> IWorld
            let entity = Entity(id)
            entity :> IObservable<EntityUpdated> |> Observable.add this.OnEntityUpdated

            (entity :> IEntity).SetComponent coordinates
            world.UpdateEntity entity
            entity

        member _.GetEntity coordinates =
            (coordinates.X, coordinates.Y)
            |> DictionaryHelper.get grid
            |> Option.bind (DictionaryHelper.get entities)

        member _.FindByTag tag =
            entities.Values
            |> Seq.filter (fun entity -> entity.HasTag tag)

type World<'T>(render: IWorld -> int) =
    inherit World(render)

    let eventHandler = EventHandler<'T> ()

    interface IWorld<'T> with member _.SendMessage message = eventHandler.Send message

    interface IObservable<'T> 
        with
            member _.Subscribe (obs : IObserver<'T>) =
                let observable = eventHandler :> IObservable<'T>
                observable.Subscribe obs