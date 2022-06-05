namespace Sokoban

open System
open System.Collections.Generic
open Components

type IWorld =
    abstract member CreateEntity: Coordinates -> Entity
    abstract member UpdateEntity: Entity -> unit
    abstract member GetEntity: Coordinates -> Entity option
    abstract member FindByTag: string -> Entity seq

module private DictionaryHelper =
    let get<'TKey, 'TValue> (dict : Dictionary<'TKey, 'TValue>) key =
        match dict.TryGetValue(key) with
        | false, _ -> None
        | true, entity -> Some entity

type World() =
    let mutable id = 0L
    let entities = Dictionary<int64, Entity>()
    let grid = Dictionary<(int * int), int64>()
    let mutable systems = []
    member _.AddSystem (sys : IWorld -> unit) = systems <- sys :: systems

    member this.Update() =
        let world = this :> IWorld
        systems |> List.iter (fun sys -> sys world)

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

            entity.SetComponent coordinates
            world.UpdateEntity entity
            entity

        member _.GetEntity coordinates =
            (coordinates.X, coordinates.Y)
            |> DictionaryHelper.get grid
            |> Option.bind (DictionaryHelper.get entities)

        member _.FindByTag tag =
            entities.Values
            |> Seq.filter (fun entity -> entity.HasTag tag)

type IWorld<'T> =
    inherit IWorld
    abstract member SendMessage: 'T -> unit

type World<'T>() =
    inherit World()

    let eventHandler = EventHandler<'T> ()

    interface IWorld<'T> with member _.SendMessage message = eventHandler.Send message

    interface IObservable<'T> 
        with
            member _.Subscribe (obs : IObserver<'T>) =
                let observable = eventHandler :> IObservable<'T>
                observable.Subscribe obs