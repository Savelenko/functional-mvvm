namespace Stamps.ViewModel

open System

(* The view model of a stamp. It differs from the model of a stamp exactly because this is a model of the _view_. The
first type is about rareness. Observe, that its terminology is a bit different: the view model does not care about the
fact that rareness must be verified. We need similar values of rareness here for display purposes in the view. *)

type Rareness =
    | NotDisplayed
    | Rare
    | NotRare

    override r.ToString () =
        match r with
        | NotDisplayed -> String.Empty
        | Rare -> "Rare"
        | NotRare -> "Common"

(* The stamp view model proper contains basic stamp "properties" which should be displayed by the view. It is the
intention, that the view provides possibility to determine rareness. This view model contains enabled/disabled state
for the respective user interface action(s), e.g. buttons and menu commands. *)

type StampViewModel =
    {
    Description : string
    Value : uint32
    Rareness : Rareness
    }

    member svm.IsVerificationEnabled = (svm.Rareness = NotDisplayed)

    override svm.ToString () = sprintf "%s (%d)" svm.Description svm.Value        

(* Here is an example of a display concern which should not be present in the model but only in the view model. Stamps
can be displayed in certain order and the following type defines the possibilities. *)

type StampSortedDisplayOrder =
    | ByValueAscending
    | ByValueDescending

type StampDisplayOrder =
    | Unsorted // this will be interpreted as "in the order of the model"
    | Sorted of StampSortedDisplayOrder

(* "The" view model is the list of stamp view models together with current display order. An invariant here is that
the order of view models in the list corresponds to current display order value. *)

type StampCollectionViewModel =
    {
    Stamps : List<StampViewModel>
    DisplayOrder : StampDisplayOrder
    IsRarenessVerificationInProgress : bool
    }

module ViewModel =

    open Stamps.Model

    /// Some helpers.
    module private Aux =        
        
        /// Sorts the stamps depending on stamp display order.
        let sortStamps (displayOrder : StampSortedDisplayOrder) =
            
            // the sorting relation
            let rel { StampViewModel.Value = v } =
                match displayOrder with
                | ByValueAscending -> int v
                | ByValueDescending -> -1 * (int v)

            List.sortBy rel

        /// Constructs the view model of one stamp.
        let viewModelOf (stamp : Stamp) : StampViewModel =

            let r =
                match stamp.Rareness with                
                | VerifiedNotRare -> NotRare
                | VerifiedRare -> Rare
                | Unknown -> NotDisplayed

            {
            Description = stamp.Description
            Value = stamp.Value
            Rareness = r
            }

        /// Constructs a list of stamp view models from the complete model. 
        let takeStampsFromModel = Model.allStamps >> List.map viewModelOf

    (* Public functions of the view model *)

    /// Computes the initial view model the application starts with, based on the initial model.
    let initialViewModel model =
        
        {
        Stamps = Aux.takeStampsFromModel model
        DisplayOrder = Unsorted
        IsRarenessVerificationInProgress = false
        }

    /// Refreshes the view model when the model changes either due to addition of a new stamp or change in stamp
    /// rareness.
    let refresh model (viewModel : StampCollectionViewModel) =

        let newStamps = Aux.takeStampsFromModel model
            
        (* As we are _refreshing_, retain current display order. *)
        let newStampsWithPreservedOrder =
            match viewModel.DisplayOrder with
            | Unsorted -> newStamps
            | Sorted order -> Aux.sortStamps order newStamps

        
        { viewModel with Stamps = newStampsWithPreservedOrder }

    /// Changes the display order of the stamps.
    let changeDisplayOrder model newDisplayOrder (viewModel : StampCollectionViewModel) =

        let newStamps =
            match viewModel.Stamps, newDisplayOrder with
            | currentStamps, _ when viewModel.DisplayOrder = newDisplayOrder -> currentStamps
            | currentStamps, Sorted order -> Aux.sortStamps order currentStamps
            | currentStamps, Unsorted -> Aux.takeStampsFromModel model

        { viewModel with Stamps = newStamps; DisplayOrder = newDisplayOrder }

    /// Makes the view model represent state when verification of stamp rareness is in progress.
    let rarenessVerificationInProgress (viewModel : StampCollectionViewModel) =
        { viewModel with IsRarenessVerificationInProgress = true }

    /// Makes the view model represent state when verification of stamp rareness is not in progress.
    let rarenessVerificationNotInProgress (viewModel : StampCollectionViewModel) =
        { viewModel with IsRarenessVerificationInProgress = false }