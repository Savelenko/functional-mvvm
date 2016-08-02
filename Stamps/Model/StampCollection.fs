namespace Stamps.Model

open System

(* The model. Its (simplified) domain is stamp collecting. *)

(* A stamp may be rare. This must be verified. *)

type Rareness =
    | Unknown
    | VerifiedRare
    | VerifiedNotRare

(* In this model, a stamp has a description, an integer value and rareness. Description uniquely identifies a stamp. *)

type Description = string

type Stamp =
    {
    Description : Description
    Value : uint32
    Rareness : Rareness
    }

(* The following types represents the fact that a stamp is owned. The benefit of this, among others, is that we can
"share" rareness value among all exemplars of one stamp which is in the collection. *)

type OwnedStamp = OwnedStamp of stamp : Stamp * numberOwned : uint32

(* Verification of stamp rareness can be performed by the imaginary "world stamp knowledgebase service". By applying
the dependency inversion principle, the model defines how it wishes to consume the service. *)

type IStampRarenessService =
    
    (* In order to be pure, the model is not allowed to verify stamp rareness directly, using this service.
    Instead, we create a definition of this action, which requires an effectful computation and will be performed
    elsewhere, outside the model. The result of this computation must then somehow find its way to the model, which
    will be shown in another component of the system. We use Async to define such computations. The nice thing
    (probably by design, exactly to support such a scenario as this) about F# Async is that definition of an Async
    value is strictly separated from its execution; due to this, it is easy to write pure functions involving Async.
    In contrast, .NET Task is supposed to be already running when returned by a function. *)

    abstract VerifyRareness : Description * uint32 -> Async<Description * Rareness>

(* "The" main model is the collection of owned stamps together with the rareness service. *)

type StampCollection =
    {
    Stamps : Set<OwnedStamp>
    RarenessService : IStampRarenessService
    }

module Model =

    /// The initial model which the application may start with.
    let emptyCollection rarenessService =
        {
        Stamps = Set.empty
        RarenessService = rarenessService
        }

    /// A small pre-filled model which the application starts with.
    let smallCollection rarenessService =

        let stamp1 = { Description = "Aereo Nicaragua 1987"; Value = 30u; Rareness = Unknown }
        let stamp2 = { Description = "Comunicaciones cosmicas"; Value = 6u; Rareness = VerifiedRare }
        let stamp3 = { Description = "Shinkansen N700"; Value = 100u; Rareness = Unknown }

        let stamps =
            Set.empty
            |> Set.add (OwnedStamp (stamp1, 1u))
            |> Set.add (OwnedStamp (stamp2, 1u))
            |> Set.add (OwnedStamp (stamp3, 2u))
        
        {
        Stamps = stamps
        RarenessService = rarenessService
        }


    (* In the next function we don't use type OwnedStamp for the convenience of the model consumer, i.e. the view
    model. We kind of consider type OwnedStamp to be an implementation detail of the model. *)

    /// Returns all stamp exemplars in the stamp collection.
    let allStamps collection =
        
        let allExemplars (OwnedStamp (stamp, number)) = List.replicate (int number) stamp

        collection.Stamps
        |> Seq.collect allExemplars
        |> List.ofSeq

    /// Adds a stamp to the stamp collection.
    let addStampToCollection description value collection =
        
        (* Given the definition that descriptions are unique, this function, together with function emptyCollection,
        maintains the invariant that there is at most one OwnedStamp in the collection for a given description. *)

        let desc (OwnedStamp ({ Description = d }, _)) = d

        let increaseOwnedQuantity = function
        | OwnedStamp (s, owned) as os when desc os = description -> OwnedStamp (s, owned + 1u)
        | otherStamp -> otherStamp

        let stamps = collection.Stamps
        if Set.exists (desc >> (=) description) stamps then
            { collection with Stamps = Set.map increaseOwnedQuantity stamps }
        else
            let withNewStamp =
                { Stamp.Description = description; Value = value; Rareness = Unknown }
                |> injectL 1u // one exemplar of the stamp in the collection
                |> OwnedStamp
                |> flip Set.add stamps
            { collection with Stamps = withNewStamp }

    (* The following two functions implement stamp rareness verification. Note how the result type of the effectful
    computation returned by the first function occurs in the type of the second function. *)
    
    /// Computes rareness of the stamp with the given description.
    let verifyStampRareness description value collection =
        collection.RarenessService.VerifyRareness (description,value)

    /// Registers rareness of the stamp with the given description. The collection does not change if it contains no
    /// stamp with this description.
    let setStampRareness description rareness collection =
        
        (* Use the invariant that there is at most one OwnedStamp in the collection for a given description. *)

        let applyRareness' s = { s with Rareness = rareness }

        let desc (OwnedStamp ({ Description = d }, _)) = d

        let applyRareness = function
        | OwnedStamp (stamp, n) as os when desc os = description -> OwnedStamp (applyRareness' stamp, n)
        | otherStamp -> otherStamp
        
        { collection with Stamps = Set.map applyRareness collection.Stamps }