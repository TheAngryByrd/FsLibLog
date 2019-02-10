namespace FsLibLog

module LogProvider =
    open System
    open Types
    open System.Diagnostics

    let mutable private currentLogProvider = None

    let private knownProviders = [
        (SerilogProvider.isAvailable , SerilogProvider.create)
        (ConsoleProvider.isAvailable, ConsoleProvider.create)
    ]

    /// Greedy search for first available LogProvider. Order of known providers matters.
    let private resolvedLogger = lazy (
        knownProviders
        |> Seq.find(fun (isAvailable,_) -> isAvailable ())
        |> fun (_, create) -> create()
    )

    /// **Description**
    ///
    /// Allows custom override when `getLogger` searches for a LogProvider.
    ///
    /// **Parameters**
    ///   * `provider` - parameter of type `ILogProvider`
    ///
    /// **Output Type**
    ///   * `unit`
    let setLoggerProvider (logProvider : ILogProvider) =
        currentLogProvider <- Some logProvider

    /// **Description**
    ///
    /// Creates a logger given a `Type`.  This will attempt to retrieve any loggers set with `setLoggerProvider`.  It will fallback to a known list of providers.
    ///
    /// **Parameters**
    ///   * `type` - parameter of type `Type`
    ///
    /// **Output Type**
    ///   * `ILog`
    let getLogger (``type`` : Type) =
        let loggerProvider =
            match currentLogProvider with
            | None -> resolvedLogger.Value
            | Some p -> p
        let logFunc = loggerProvider.GetLogger(``type``.ToString())
        { new ILog with member x.Log = logFunc}

    /// **Description**
    ///
    /// Creates a logger. It's name is based on the current StackFrame.
    ///
    /// **Output Type**
    ///   * `ILog`
    ///
    let inline getCurrentLogger ()   =
        let stackFrame = StackFrame(0, false)
        getLogger(stackFrame.GetMethod().DeclaringType)
