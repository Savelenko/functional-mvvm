namespace Stamps.ViewModel

(* The view model of a stamp. It differs from the model of a stamp exactly because this is a model of the _view_. The
first type is about rareness. Observe, that its terminology is a bit different: the view model does not care about the
fact that rareness must be verified. We need similar values of rareness here for display purposes in the view. *)

type Rareness =
    | Unknown
    | Rare
    | NotRare    

(* The stamp view model proper contains basic stamp "properties" which should be displayed by the view. It is the
intention, that the view provides possibility to determine rareness. This view model contains enabled/disabled state
for the respective user interface action(s), e.g. buttons and menu commands. *)

type StampViewModel =
    {
    Description : string
    Value : uint32
    Rareness : Rareness
    }

    member svm.IsVerificationEnabled = (svm.Rareness = Unknown)

(* Here is an example of a display concern which should not be present in the model but only in the view model. Stamps
can be displayed in certain order and the following type defines the possibilities. *)

type StampDisplayOrder =
    | Unsorted
    | ByValueAscending
    | ByValueDescending

(* "The" view model is the list of stamp view models together with current display order. An invariant here is that
the order of view models in the list corresponds to current display order value. *)

type StampCollectionViewModel =
    {
    Stamps : List<StampViewModel>
    DisplayOrder : StampDisplayOrder
    }

module ViewModel =

    /// Some helpers.
    module private Aux =
        
        /// Sorts the stamps depending on stamp display order.
        let sortStamps (displayOrder : StampDisplayOrder) =
            
            let rel { StampViewModel.Value = v } =
                match displayOrder with
                | ByValueAscending -> int v
                | ByValueDescending -> -1 * (int v)

            List.sortBy rel