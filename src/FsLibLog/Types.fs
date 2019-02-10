namespace FsLibLog

module Types =
    open System
    type LogLevel =
    | Trace = 0
    | Debug = 1
    | Info = 2
    | Warn = 3
    | Error = 4
    | Fatal = 5

    /// An optional message thunk.
    ///
    /// - If `None` is provided, this typically signals to the logger to do a isEnabled check.
    /// - If `Some` is provided, this signals the logger to log.
    type MessageThunk = (unit -> string) option

    /// The signature of a log message function
    type LogMessage = LogLevel -> MessageThunk -> exn option -> obj array -> bool

    /// An interface wrapper for `LogMessage`. Useful when using depedency injection frameworks.
    type ILog =
        abstract member Log :  LogMessage

    /// An interface for retrieving a concrete logger such as Serilog, Nlog, etc.
    type ILogProvider =
        abstract member GetLogger : string -> LogMessage
        abstract member OpenNestedContext : string -> IDisposable
        abstract member OpenMappedContext : string -> obj -> bool -> IDisposable


