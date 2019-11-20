namespace FsLibLog.Adapters.Npgsql

open FsLibLog
open FsLibLog.Types
open Npgsql.Logging

type FsLibLogLogger (logger :  FsLibLog.Types.ILog) =
    inherit NpgsqlLogger() with

        let mapLogLevels (level : NpgsqlLogLevel) =
            match level with
            | NpgsqlLogLevel.Trace -> LogLevel.Trace
            | NpgsqlLogLevel.Debug -> LogLevel.Debug
            | NpgsqlLogLevel.Info ->  LogLevel.Info
            | NpgsqlLogLevel.Warn ->  LogLevel.Warn
            | NpgsqlLogLevel.Error -> LogLevel.Error
            | NpgsqlLogLevel.Fatal -> LogLevel.Fatal
            | _ -> LogLevel.Trace

        override __.IsEnabled(level : NpgsqlLogLevel) =
            level
            |> mapLogLevels
            |> Log.StartLogLevel
            |> logger.fromLog
        override __.Log
            (   level : NpgsqlLogLevel,
                connectorId : int,
                message : string,
                ex : exn) =
            let log =
                level
                |> mapLogLevels
                |> Log.StartLogLevel
            let format = "{connectorId} : {message}"
            let logConfig =
                Log.setMessage format
                >> Log.addContext "connectorId" connectorId
                >> Log.addContext "message" message
                >> Log.addException ex
            log
            |> logConfig
            |> logger.fromLog
            |> ignore

type FsLibLogLoggerProvider () =
    interface INpgsqlLoggingProvider with
        member __.CreateLogger (name : string) =
            LogProvider.getLoggerByName name
            |> FsLibLogLogger
            :> NpgsqlLogger
