namespace SomeLib
open FsLibLog.Types

module Say =
    let logger = FsLibLog.LogProvider.getCurrentLogger()
    let hello name =
        logger.Log LogLevel.Warn (Some (fun () -> name)) None [||]
        |> ignore
