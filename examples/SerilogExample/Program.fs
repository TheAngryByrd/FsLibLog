open System
open Serilog
open SomeLib
open Serilog.Context

[<EntryPoint>]
let main argv =
    let log =
        LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.ColoredConsole(outputTemplate= "{Timestamp:HH:mm} [{Level}] <{SourceContext}> ({Name:l}) {Message:j} - {Properties:j}{NewLine}{Exception}{NewLine}")
            .Enrich.FromLogContext()
            .CreateLogger();
    Log.Logger <- log

    Say.hello "Captain" |> printfn "%A"


    Say.fail "Captain"

    Console.ReadLine() |> ignore
    0
