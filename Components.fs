module Components

open System

type IComponent = interface end
type Coordinates = { X: int; Y: int } interface IComponent
type Display = { Schematic: string } interface IComponent
type Rigidbody = { Movable: bool } interface IComponent

type EntityUpdateInfo = { EntityId: int64; Component: IComponent }
type EntityUpdated = ComponentAdded of EntityUpdateInfo | ComponentRemoved of EntityUpdateInfo

type Entity(id: int64) =
    let mutable components : IComponent list = []
    let mutable tags = Set.empty<string>
    let mutable observers : IObserver<EntityUpdated> list = []

    static member private GetComponentByType<'T when 'T :> IComponent> (cmp : IComponent) =
        match cmp with
        | :? 'T as t  -> Some t
        | _ -> None

    member _.Id = id

    member _.SetComponent<'T when 'T :> IComponent> (adding : 'T) =
        let cmp = adding :> IComponent
        let mutable commands = [ ComponentAdded { EntityId = id; Component = cmp } ]
        components <- components
        |> List.tryFindIndex (Entity.GetComponentByType<'T> >> Option.isSome)
        |> function
        | Some index ->
            let removing = components |> List.item index
            commands <- ComponentRemoved { EntityId = id; Component = removing } :: commands
            List.updateAt index cmp components
        | None -> cmp :: components

        List.allPairs commands observers
        |> List.iter (fun (command, observer) -> observer.OnNext command)

    member _.GetComponent<'T when 'T :> IComponent> () =
        components
        |> Seq.choose Entity.GetComponentByType<'T>
        |> Seq.tryHead

    member _.AddTag tag = tags <- tags.Add tag

    member _.HasTag tag = tags.Contains tag

    interface IObservable<EntityUpdated> with
        member _.Subscribe observer =
            observers <- observer :: observers
            {
                new IDisposable with 
                    member _.Dispose () =
                        observers <- List.except [ observer ] observers
            }

open System.Collections.Generic

let private get<'TKey, 'TValue> (dict : Dictionary<'TKey, 'TValue>) key =
    match dict.TryGetValue(key) with
    | false, _ -> None
    | true, entity -> Some entity

type IWorld =
    abstract member CreateEntity: Coordinates -> Entity
    abstract member UpdateEntity: Entity -> unit
    abstract member GetEntity: Coordinates -> Entity option
    abstract member FindByTag: string -> Entity seq

type World() =
    let mutable id = 0L
    let entities = Dictionary<int64, Entity>()
    let grid = Dictionary<(int * int), int64>()
    let mutable systems = []

    member _.AddSystem (sys : IWorld -> unit) = systems <- sys :: systems
    member this.Update() =
        let world = this :> IWorld
        systems
        |> List.iter (fun sys -> sys world)

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
        member _.UpdateEntity entity =
            entities[entity.Id] <- entity

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
            |> get grid
            |> Option.bind (get entities)

        member _.FindByTag tag =
            entities.Values
            |> Seq.filter (fun entity -> entity.HasTag tag)

