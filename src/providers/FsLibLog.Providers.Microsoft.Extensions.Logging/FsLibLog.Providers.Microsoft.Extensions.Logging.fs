namespace FsLibLog.Providers.Microsoft.Extensions

open Microsoft.Extensions.Logging
open FsLibLog
open System
open System.Collections.Generic

module Logging =
    type private MicrosoftProvider (factory : ILoggerFactory) =
        interface ILogProvider with
            override _.GetLogger(name) =
                let microsoftLogger = factory.CreateLogger name

                fun (logLevel : LogLevel) (messageThunk : (unit -> string) option) (exn : exn option) (args : obj array) ->
                    let microsoftLogLevel =
                        match logLevel with
                        | LogLevel.Trace -> Microsoft.Extensions.Logging.LogLevel.Trace
                        | LogLevel.Debug -> Microsoft.Extensions.Logging.LogLevel.Debug
                        | LogLevel.Info -> Microsoft.Extensions.Logging.LogLevel.Information
                        | LogLevel.Warn -> Microsoft.Extensions.Logging.LogLevel.Warning
                        | LogLevel.Error -> Microsoft.Extensions.Logging.LogLevel.Error
                        | LogLevel.Fatal -> Microsoft.Extensions.Logging.LogLevel.Critical
                        | other -> Microsoft.Extensions.Logging.LogLevel.None

                    match messageThunk with
                    | Some messageThunk ->
                        match exn with
                        | Some ex-> microsoftLogger.Log(microsoftLogLevel, ex, messageThunk (), args)
                        | None -> microsoftLogger.Log(microsoftLogLevel, messageThunk (), args)
                        true
                    | None ->
                        microsoftLogger.IsEnabled microsoftLogLevel

            override _.OpenMappedContext name (o : obj) destructure =
                // Create bogus logger that will propagate to a real logger later
                let logger = factory.CreateLogger(Guid.NewGuid().ToString("n"))
                // Requires a IEnumerable<KeyValuePair> to make sense
                // https://nblumhardt.com/2016/11/ilogger-beginscope/
                [KeyValuePair(name, o)]
                |> logger.BeginScope

            override _.OpenNestedContext name =
                // Create bogus logger that will propagate to a real logger later
                let logger = factory.CreateLogger(Guid.NewGuid().ToString("n"))
                logger.BeginScope name


    let create (factory : ILoggerFactory) = MicrosoftProvider(factory) :> FsLibLog.Types.ILogProvider
