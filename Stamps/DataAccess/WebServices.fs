namespace Stamps.DataAccess

open System
open Stamps.Model

type WorldStampKnowledgeBaseService() =

    interface IStampRarenessService with
        
        member svc.VerifyRareness description = raise (NotImplementedException "Service not yet implemented.")