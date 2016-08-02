namespace Stamps.Messaging

open Stamps.Model
open Stamps.ViewModel

(* The view model and view are triggered using messages. A message originates from the view based on a GUI event.
A message can also originate from the "effectful world" and carry a result of an effectful computation performed in
that world, for example (EM1) below. The following type defines all possible messages. *)

type Message =
    | AddStamp of description : string * value : uint32
    | ValidateRareness of description : string * value : uint32
    | RarenessValidated of description : string * rareness : Stamps.Model.Rareness // (EM1)
    | SortStamps of StampSortedDisplayOrder
    | RemoveStampSorting

(* The model returns effectful computations as F# Async values. A bit further, we will need to distinguish situations
when there is a computation to perform and when no action should be taken, beside application of model and view model
functions. The following type contains corresponding values, where None designates no need to perform any effect. *)

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

module Dispatching =

    (* The dispatch function translates messages to invocations of functions provided by the model and the view model.
    For this purpose, this function knows the meaning of each message and each function. The dispatcher function is
    pure and does not maintain reference to the model and the view model. Instead, it receives current model and view
    model and returns new model and view model as in the following (abstract) signature:
    
    (Model, ViewModel) -> Message -> (Model, ViewModel, Effect)

    Depending on the exact message, the model may request that an effectful computation is performed. The dispatcher
    function does not execute effects. It returns them for execution elsewhere. *)
    
    let dispatch (m,vm) = function

        // User initiated action "add stamp to collection" in the view.
        | AddStamp (description, value) ->
            let m' = Model.addStampToCollection description value m
            let vm' = ViewModel.refresh m' vm
            (m',vm',None)
        
        // User initiated action "validate stamp rareness" in the view.
        | ValidateRareness (description, value) -> 
            let validation = Effect <| Model.verifyStampRareness description value m
            // Transform the outcome of validation to a message which will be fed into this same dispatcher function.
            // Note that this means that Effect T becomes Effect Message with T carried by the corresponding message.
            let validation' = validation |> Effect.map RarenessValidated

            let vm' = ViewModel.rarenessVerificationInProgress vm

            (m,vm',validation')            
        
        // The "effectful world" computed rareness of a stamp.
        | RarenessValidated (description, rareness) ->
            let m' = Model.setStampRareness description rareness m
            let vm' = vm |> ViewModel.refresh m' |> ViewModel.rarenessVerificationNotInProgress
            (m',vm',None)

        // ... and so on
        | SortStamps order ->
            // Note, that the model does not change, as this message is processed solely by the view model.
            let vm' = ViewModel.changeDisplayOrder m (Sorted order) vm
            (m,vm',None)

        | RemoveStampSorting ->
            // Note, that the model does not change, as this message is processed solely by the view model.
            let vm' = ViewModel.changeDisplayOrder m Unsorted vm
            (m,vm',None)