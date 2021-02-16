namespace FsLibLog.Providers.Expecto

open System
open FsLibLog

module EMsg = Expecto.Logging.Message
type ELL = Expecto.Logging.LogLevel

module Helpers =
    let addExnOpt exOpt msg =
        match exOpt with
        | None -> msg
        | Some ex -> msg |> EMsg.addExn ex

    let addValues (items:obj[]) msg =
        (msg,items |> Seq.mapi(fun i item -> i,item))
        ||> Seq.fold(fun msg (i,item) ->
        msg
        |> EMsg.setField (string i) item
        )

    let getLogLevel : LogLevel -> Expecto.Logging.LogLevel =
        function
        | LogLevel.Debug -> ELL.Debug
        | LogLevel.Error -> ELL.Error
        | LogLevel.Fatal -> ELL.Fatal
        | LogLevel.Info -> ELL.Info
        | LogLevel.Trace -> ELL.Verbose
        | LogLevel.Warn -> ELL.Warn
        | _ -> ELL.Warn

open Helpers

// Naive implementation, not that important, just need logging to actually work
type ExpectoLogProvider () =
    let mutable contexts = ResizeArray<_>() // not thread-safe
    let createDisp (name:string) =
        contexts.Add name
        let fullContext =
            contexts
            |> String.concat "."
        let logger = Expecto.Logging.Log.create fullContext
        let logContextChange title =
            sprintf "%s %s" fullContext title
            |> EMsg.eventX
            |> logger.log ELL.Info
            |> Async.RunSynchronously
        logContextChange "Starting"
        let deq () =
            logContextChange "Finished"

            if not <| contexts.Remove name then
                let msg = sprintf "Could not find context %s in collection" name
                x.log ELL.Error (EMsg.eventX msg)
                |> Async.RunSynchronously
        { new IDisposable with
            member __.Dispose() = deq()
        }

    interface ILogProvider with
        override __.GetLogger(name:string): Logger =
            let logger = Expecto.Logging.Log.create name
            fun ll mt exnOpt values ->
                match mt with
                | Some f ->
                    let ll = getLogLevel ll
                    logger.log ll (fun ll ->
                        let message = f()
                        let msg = Expecto.Logging.Message.eventX message ll
                        match exnOpt with
                        | None -> msg
                        | Some ex -> msg |> Expecto.Logging.Message.addExn ex
                        |> addValues values
                    ) |> Async.RunSynchronously
                    true
                | None -> false
        override __.OpenMappedContext (name:string) (o:obj) (b:bool) =
            createDisp name
        override __.OpenNestedContext name = createDisp name
