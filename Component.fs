module Component

open Domain

type ITransform = abstract member Coordinates : Coordinates with get, set

type IRigidbody = interface end

type IPrintable = abstract member Display: unit -> string