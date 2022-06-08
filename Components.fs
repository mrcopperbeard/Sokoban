namespace Sokoban

open System.Collections.Generic
open Sokoban.Engine

module Components =
    type Display = Schematic of string | Animated of IEnumerator<string> interface IComponent
    type Rigidbody = { Movable: bool } interface IComponent