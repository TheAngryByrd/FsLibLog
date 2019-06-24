open System
open Serilog
open SomeLib
open Serilog.Context

[<EntryPoint>]
let main argv =
    let log =
        LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.ColoredConsole(outputTemplate= "{Timestamp:o} [{Level}] <{SourceContext}> ({Name:l}) {Message:j} - {Properties:j}{NewLine}{Exception}")
            .Enrich.FromLogContext() //Necessary if you want to use MappedContext
            .CreateLogger();
    Log.Logger <- log

    Say.hello "Captain" |> printfn "%A"

    Say.nestedHello "Commander" |> printfn "%A"

    Say.fail "DaiMon"

    Console.ReadLine() |> ignore
    0
