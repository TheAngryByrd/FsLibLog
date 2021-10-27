// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System
open Microsoft.Extensions.Logging


[<EntryPoint>]
let main argv =
    let microsoftLoggerFactory = LoggerFactory.Create(fun builder ->
        builder
            .SetMinimumLevel(LogLevel.Debug)
            .AddSimpleConsole(fun opts -> opts.IncludeScopes <-true)
            // .AddJsonConsole(fun opts -> opts.IncludeScopes <- true)
        |> ignore

    )

    // This line is important to make Microsoft.Extensions.Logging work
    FsLibLog.Providers.MicrosoftExtensionsLoggingProvider.setMicrosoftLoggerFactory microsoftLoggerFactory
    SomeLib.Say.nestedHello "Howdy" |> printfn "%s"
    SomeLib.Say.hello "Whatup" |> printfn "%s"
    try
        SomeLib.Say.fail "failed"
    with e -> ()
    Console.ReadLine() |> ignore
    0 // return an integer exit code
