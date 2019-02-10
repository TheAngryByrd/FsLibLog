namespace FsLibLog

module SerilogProvider =
    open System
    open Types
    open System.Linq.Expressions

    let getLogManagerType () =
        Type.GetType("Serilog.Log, Serilog")
    let isAvailable () =
        getLogManagerType () |> isNull |> not

    let getForContextMethodCall () =
        let logManagerType = getLogManagerType ()
        let method = logManagerType.GetMethod("ForContext", [|typedefof<string>; typedefof<obj>; typedefof<bool>|])
        let propertyNameParam = Expression.Parameter(typedefof<string>, "propertyName")
        let valueParam = Expression.Parameter(typedefof<obj>, "value")
        let destructureObjectsParam = Expression.Parameter(typedefof<bool>, "destructureObjects")
        let exrs : Expression []=
            [|
                propertyNameParam
                valueParam
                destructureObjectsParam
            |]
        let methodCall =
            Expression.Call(null, method, exrs)
        let func =
            Expression.Lambda<Func<string, obj, bool, obj>>(
                methodCall,
                propertyNameParam,
                valueParam,
                destructureObjectsParam).Compile()
        fun name -> func.Invoke("SourceContext", name, false)

    type SerilogGateway = {
        Write : obj -> obj -> string -> obj [] -> unit
        WriteException : obj -> obj -> exn -> string -> obj [] -> unit
        IsEnabled : obj -> obj -> bool
        TranslateLevel : LogLevel -> obj
    } with
        static member Create () =
            let logEventLevelType = Type.GetType("Serilog.Events.LogEventLevel, Serilog")
            if (logEventLevelType |> isNull) then
                failwith ("Type Serilog.Events.LogEventLevel was not found.")

            let debugLevel = Enum.Parse(logEventLevelType, "Debug", false)
            let errorLevel = Enum.Parse(logEventLevelType, "Error", false)
            let fatalLevel = Enum.Parse(logEventLevelType, "Fatal", false)
            let informationLevel = Enum.Parse(logEventLevelType, "Information", false)
            let verboseLevel = Enum.Parse(logEventLevelType, "Verbose", false)
            let warningLevel = Enum.Parse(logEventLevelType, "Warning", false)
            let translateLevel (level : LogLevel) =
                match level with
                | LogLevel.Fatal -> fatalLevel
                | LogLevel.Error -> errorLevel
                | LogLevel.Warn -> warningLevel
                | LogLevel.Info -> informationLevel
                | LogLevel.Debug -> debugLevel
                | LogLevel.Trace -> verboseLevel
                | _ -> debugLevel

            let loggerType = Type.GetType("Serilog.ILogger, Serilog")
            if (loggerType |> isNull) then failwith ("Type Serilog.ILogger was not found.")
            let isEnabledMethodInfo = loggerType.GetMethod("IsEnabled", [|logEventLevelType|])
            let instanceParam = Expression.Parameter(typedefof<obj>)
            let instanceCast = Expression.Convert(instanceParam, loggerType)
            let levelParam = Expression.Parameter(typedefof<obj>)
            let levelCast = Expression.Convert(levelParam, logEventLevelType)
            let isEnabledMethodCall = Expression.Call(instanceCast, isEnabledMethodInfo, levelCast)
            let isEnabled =
                Expression
                    .Lambda<Func<obj, obj, bool>>(isEnabledMethodCall, instanceParam, levelParam).Compile()

            let writeMethodInfo =
                loggerType.GetMethod("Write", [|logEventLevelType; typedefof<string>; typedefof<obj []>|])
            let messageParam = Expression.Parameter(typedefof<string>)
            let propertyValuesParam = Expression.Parameter(typedefof<obj []>)
            let writeMethodExp =
                Expression.Call(
                    instanceCast,
                    writeMethodInfo,
                    levelCast,
                    messageParam,
                    propertyValuesParam)
            let expression =
                Expression.Lambda<Action<obj, obj, string, obj []>>(
                    writeMethodExp,
                    instanceParam,
                    levelParam,
                    messageParam,
                    propertyValuesParam)
            let write = expression.Compile()

            // // Action<object, object, string, Exception> WriteException =
            // // (logger, level, exception, message) => { ((ILogger)logger).Write(level, exception, message, new object[]); }
            let writeExceptionMethodInfo =
                loggerType.GetMethod(
                    "Write",
                    [| logEventLevelType; typedefof<exn>; typedefof<string>; typedefof<obj []>|])
            let exceptionParam = Expression.Parameter(typedefof<exn>)
            let writeMethodExp =
                Expression.Call(
                    instanceCast,
                    writeExceptionMethodInfo,
                    levelCast,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam)
            let writeException =
                Expression.Lambda<Action<obj, obj, exn, string, obj []>>(
                    writeMethodExp,
                    instanceParam,
                    levelParam,
                    exceptionParam,
                    messageParam,
                    propertyValuesParam).Compile()
            {
                Write = (fun logger level message formattedParmeters -> write.Invoke(logger,level,message,formattedParmeters))
                WriteException = fun logger level ex message formattedParmeters -> writeException.Invoke(logger,level,ex,message,formattedParmeters)
                IsEnabled = fun logger level -> isEnabled.Invoke(logger,level)
                TranslateLevel = translateLevel
            }

    type private SerigLogProvider () =
        let getLoggerByName = getForContextMethodCall ()
        let serilogGatewayInit = lazy(SerilogGateway.Create())

        let writeMessage logger logLevel (messageFunc : MessageThunk) ``exception`` formatParams =
            let serilogGateway = serilogGatewayInit.Value
            let translatedValue = serilogGateway.TranslateLevel logLevel
            match messageFunc with
            | None -> serilogGateway.IsEnabled logger translatedValue
            | Some _ when  serilogGateway.IsEnabled logger translatedValue |> not -> false
            | Some m ->
                match ``exception`` with
                | Some ex ->
                    serilogGateway.WriteException logger translatedValue ex (m()) formatParams
                | None ->
                    serilogGateway.Write logger translatedValue (m()) formatParams
                true

        interface ILogProvider with
            member this.GetLogger(name: string): Logger =
                let logger =  getLoggerByName (name)
                printfn "%A" logger
                writeMessage logger
            member this.OpenMappedContext(arg1: string) (arg2: obj) (arg3: bool): IDisposable =
                failwith "Not Implemented"
            member this.OpenNestedContext(arg1: string): IDisposable =
                failwith "Not Implemented"

    let create () =
        SerigLogProvider () :> ILogProvider
