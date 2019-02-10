namespace FsLibLog

module LogProvider =
    open System
    open Types
    open System.Diagnostics
    let mutable private logProvider = ConsoleProvider.create()

    let getLogger (``type`` : Type) =
        let logFunc = logProvider.GetLogger(``type``.ToString())
        { new ILog with member x.Log = logFunc}
    let inline getCurrentLogger ()   =
        let stackFrame = StackFrame(0, false)
        getLogger(stackFrame.GetMethod().DeclaringType)
