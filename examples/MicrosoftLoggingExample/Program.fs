// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Microsoft.Extensions.Logging


[<EntryPoint>]
let main argv =
    let factory = LoggerFactory.Create(fun builder ->
        builder
            .SetMinimumLevel(LogLevel.Debug)
            .AddSimpleConsole(fun opts -> opts.IncludeScopes <-true)
            // .AddJsonConsole(fun opts -> opts.IncludeScopes <- true)
        |> ignore
            // .AddSimpleConsole(fun bu -> bu.IncludeScopes <- true) |> ignore
    )

    FsLibLog.LogProvider.setLoggerProvider <| FsLibLog.Providers.Microsoft.Extensions.Logging.create factory
    SomeLib.Say.nestedHello "Howdy" |> printfn "%s"
    SomeLib.Say.hello "Whatup" |> printfn "%s"
    Console.ReadLine() |> ignore
    0 // return an integer exit code
