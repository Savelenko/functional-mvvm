namespace Stamps.Model

(* We use F# Async as a base for the type of effectful computations. Below, value None designates no need to perform
any effect. *)

type Effect<'T> =
    | None
    | Effect of Async<'T>

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module Effect =

    /// Applies the given function to the result of the effectful computation.
    let map f = function
        | None -> None
        | Effect comp ->            
            async {
                let! x = comp
                return f x
            }
            |> Effect