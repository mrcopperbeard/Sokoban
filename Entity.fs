namespace Sokoban

open System
open Components

type Entity(id: int64) =
    let mutable components : IComponent list = []
    let mutable tags = Set.empty<string>

    let eventHandler = EventHandler<EntityUpdated> ()

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

        commands
        |> List.iter eventHandler.Send

    member _.GetComponent<'T when 'T :> IComponent> () =
        components
        |> Seq.choose Entity.GetComponentByType<'T>
        |> Seq.tryHead

    member _.AddTag tag = tags <- tags.Add tag

    member _.HasTag tag = tags.Contains tag

    interface IObservable<EntityUpdated> with
        member _.Subscribe observer =
            let observable = eventHandler :> IObservable<EntityUpdated>
            observable.Subscribe observer