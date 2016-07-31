namespace Stamps.DataAccess

open System
open Stamps.Model

type WorldStampKnowledgeBaseService() =

    interface IStampRarenessService with
        
        member svc.VerifyRareness(description, value) =
            
            (* A stamp is considered rare, if its value is not divisible by 5. However, it takes the service quite
            some time to calculate the answer. Not enough cores or something... *)
            async {
                // simulate not enough cores
                Async.Sleep 10000 |> Async.RunSynchronously |> ignore
            
                return (description, if int value % 5 = 0 then Rareness.VerifiedNotRare else VerifiedRare)
            }
            |> Effect
                        