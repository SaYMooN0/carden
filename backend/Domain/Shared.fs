module Domain.Shared

open System

module Nullable =
    let toOption (value: Nullable<'T>) =
        if value.HasValue then Some value.Value else None
