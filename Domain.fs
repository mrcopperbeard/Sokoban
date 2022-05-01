module Domain

type GameObject = Player | Box
type Cell = Floor of GameObject option | Target of GameObject option | Wall
with
    static member setObj cell obj =
        match cell with
        | Floor _ -> Floor obj
        | Target _ -> Target obj
        | Wall -> cell