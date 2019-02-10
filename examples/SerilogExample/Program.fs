// Learn more about F# at http://fsharp.org

open System
open Serilog
open SomeLib

[<EntryPoint>]
let main argv =
    // FsLibLog.LogProvider.setLoggerProvider <| FsLibLog.SerilogProvider.create()
    let log =
        LoggerConfiguration()
            .WriteTo.ColoredConsole(outputTemplate= "{Timestamp:HH:mm} [{Level}] ({Name:l}) {Message}{NewLine}{Exception}")
            .CreateLogger();
    Log.Logger <- log

    Say.hello "Captain"

    Console.ReadLine() |> ignore
    // printfn "Hello World from F#!"
    0 // return an integer exit code
