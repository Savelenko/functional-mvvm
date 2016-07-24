[<AutoOpen>]
module Prelude

/// Exchanges parameters of a binary function.
let flip f b a = f a b

/// Injects the second argument into a tuple in left position.
let injectL r l = (l,r)

/// Injects the second argument into a tuple in right position.
let injectR l r = (l,r)