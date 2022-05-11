open System

open Domain
open Entity
open Grid

type GameObject = Player of PlayerEntity | Box of BoxEntity | Target of TargetEntity
let grid = Grid(5)

PlayerEntity(0, 0) |> grid.Add
BoxEntity(0, 1) |> grid.Add
TargetEntity(0, 2) |> grid.Add

(0, 0)
|> Coordinates
|> grid.Display
|> printfn "%s"