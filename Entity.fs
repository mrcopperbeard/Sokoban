module Entity

open Domain
open Component

type PlayerEntity(coordinates) =
    inherit Entity()
    interface IRigidbody
    interface ITransform with member val Coordinates = coordinates with get, set
    interface IPrintable with member _.Display() = "~☺~"

type BoxEntity(coordinates) =
    inherit Entity()
    interface IRigidbody
    interface ITransform with member val Coordinates = coordinates with get, set
    interface IPrintable with member _.Display() = "[ ]"

type TargetEntity(coordinates) =
    inherit Entity()
    interface IRigidbody
    interface ITransform with member val Coordinates = coordinates with get, set
    interface IPrintable with member _.Display() = " X "

type WallEntity(coordinates) =
    interface IRigidbody
    interface ITransform with member val Coordinates = coordinates with get, set
    interface IPrintable with member _.Display() = "███"