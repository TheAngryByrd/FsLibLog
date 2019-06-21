namespace FsLibLog.Adapters.Marten

open FsLibLog
open FsLibLog.Types
open Marten
open Marten.Services
open Npgsql
open System.Linq
open System.Collections.Generic

type FsLibLogLogger (logger :  FsLibLog.Types.ILog) =
    new () =  FsLibLogLogger (LogProvider.getLoggerByName "Marten")
    interface IMartenLogger with
        member this.StartSession(_session : IQuerySession) : IMartenSessionLogger = this :> IMartenSessionLogger
        member __.SchemaChange(sql : string) =
            logger.debug(
                Log.setMessage "Schema Change: {sql}"
                >> Log.addParameter sql
            )

    interface IMartenSessionLogger with
        member __.LogSuccess(command:NpgsqlCommand) =
            let parameters = Dictionary<string,obj>()
            command.Parameters
            |> Seq.iter(fun p ->
                parameters.Add(p.ParameterName, p.Value)
            )
            logger.debug(
                Log.setMessage "Success: {CommandText} - {parameters}"
                >> Log.addParameter command.CommandText
                >> Log.addParameter parameters
            )

        member __.LogFailure(command:NpgsqlCommand, ex : exn) =
            let parameters = Dictionary<string,obj>()
            command.Parameters
            |> Seq.iter(fun p ->
                parameters.Add(p.ParameterName, p.Value)
            )
            logger.error(
                Log.setMessage "Failure: {CommandText} - {parameters}"
                >> Log.addParameter command.CommandText
                >> Log.addParameter parameters
                >> Log.addException ex
            )

        member __.RecordSavedChanges(_session: IDocumentSession, commit : IChangeSet) =
            logger.debug(
                Log.setMessage "Persisted {Updated} updates, {Inserted} inserts, {Deleted} deletions, {Patches} patched"
                >> Log.addParameter (commit.Updated.Count())
                >> Log.addParameter (commit.Inserted.Count())
                >> Log.addParameter (commit.Deleted.Count())
                >> Log.addParameter (commit.Patches.Count())
            )
