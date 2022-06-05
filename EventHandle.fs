namespace Sokoban

open System

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