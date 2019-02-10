namespace FsLibLog

module LogProvider =
    open System
    open Types
    open System.Diagnostics
    let mutable private currentLogProvider = None

    let knownProviders = [
        (SerilogProvider.isAvailable , SerilogProvider.create)
        (ConsoleProvider.isAvailable, ConsoleProvider.create)
    ]

    let resolvedLogger = lazy (
        knownProviders
        |> Seq.find(fun (isAvailable,_) -> isAvailable ())
        |> fun (_, create) -> create()
    )

    let setLoggerProvider (provider) =
        currentLogProvider <- Some provider

    let getLogger (``type`` : Type) =
        let loggerProvider =
            match currentLogProvider with
            | None -> resolvedLogger.Value
            | Some p -> p
        let logFunc = loggerProvider.GetLogger(``type``.ToString())
        { new ILog with member x.Log = logFunc}

    let inline getCurrentLogger ()   =
        let stackFrame = StackFrame(0, false)
        getLogger(stackFrame.GetMethod().DeclaringType)
