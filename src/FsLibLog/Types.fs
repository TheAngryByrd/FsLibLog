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

    type MessageThunk = (unit -> string) option

    type Logger = LogLevel -> MessageThunk -> exn option -> obj array -> bool

    type ILog =
        abstract member Log :  Logger

    type ILogProvider =
        abstract member GetLogger : string -> Logger
        abstract member OpenNestedContext : string -> IDisposable
        abstract member OpenMappedContext : string -> obj -> bool -> IDisposable


