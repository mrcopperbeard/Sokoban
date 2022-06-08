namespace Sokoban.Engine

open System

type IComponent = interface end
type Coordinates = { X: int; Y: int } interface IComponent

type IEntity =
    abstract member Id: int64
    abstract member SetComponent:  #IComponent -> unit
    abstract member GetComponent: unit -> #IComponent option
    abstract member AddTag: string -> unit
    abstract member HasTag: string -> bool

type IWorld =
    abstract member CreateEntity: Coordinates -> IEntity
    abstract member UpdateEntity: IEntity -> unit
    abstract member GetEntity: Coordinates -> IEntity option
    abstract member FindByTag: string -> IEntity seq

type IWorld<'T> =
    inherit IWorld
    abstract member SendMessage: 'T -> unit

module private DictionaryHelper =
    open System.Collections.Generic

    let get<'TKey, 'TValue> (dict : Dictionary<'TKey, 'TValue>) key =
        match dict.TryGetValue(key) with
        | false, _ -> None
        | true, entity -> Some entity

type private EventHandler<'T> () =
    let mutable observers : IObserver<'T> list = []

    member _.Send message = observers |> List.iter (fun o -> o.OnNext message)

    interface IObservable<'T> with
        member _.Subscribe obs =
            observers <- obs :: observers
            {
                new IDisposable with
                    member _.Dispose () = observers <- observers |> List.except [obs]
            }