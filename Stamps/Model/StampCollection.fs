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
the dependency inversion principle, the model defines how it wishes to consume the service. We use F# Async as a base
for the type of effectful computations. Below, value None designates no need to perform any effect. *)

type Effect<'T> =
    | None
    | Effect of Async<'T>

type IStampRarenessService =
    
    abstract VerifyRareness : Description -> Effect<Description * Rareness>

(* "The" main model is the collection of owned stamps together with the rareness service. *)

type StampCollection =
    {
    Stamps : Set<OwnedStamp>
    RarenessService : IStampRarenessService
    }

[<CompilationRepresentation(CompilationRepresentationFlags.ModuleSuffix)>]
module StampCollection =

    /// The initial model which the application starts with.
    let emptyCollection rarenessService =
        {
        Stamps = Set.empty
        RarenessService = rarenessService
        }

    (* In the next function we don't use type OwnedStamp for the convenience of the model consumer, i.e. the view
    model. We kind of consider type OwnedStamp to be implementation details of the model. *)

    /// Returns all stamp exemplars in the stamp collection.
    let allStamps collection =
        
        let allExemplars (OwnedStamp (stamp, number)) = List.replicate (int number) stamp

        let { Stamps = stamps } = collection        

        stamps
        |> Seq.collect allExemplars
        |> List.ofSeq

    /// Adds a stamp to the stamp collection.
    let addStampToCollection description value collection =
        
        (* Given the definition that descriptions are unique, this function, together with function emptyCollection,
        maintain the invariant that there is at least one OwnedStamp in the collection for a given description. *)

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

    (* The following two functions implement stamp rareness verification. Note how the type of the effect returned by
    the first function occurs in the type of the second function. *)
    
    /// Computes rareness of the stamp with the given description.
    let verifyStampRareness description collection =
        collection.RarenessService.VerifyRareness description

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