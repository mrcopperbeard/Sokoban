namespace Sokoban

module Components =
    type IComponent = interface end
    type Coordinates = { X: int; Y: int } interface IComponent
    type Display = { Schematic: string } interface IComponent
    type Rigidbody = { Movable: bool } interface IComponent

    type EntityUpdateInfo = { EntityId: int64; Component: IComponent }
    type EntityUpdated = ComponentAdded of EntityUpdateInfo | ComponentRemoved of EntityUpdateInfo