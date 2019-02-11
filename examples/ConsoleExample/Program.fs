open System

[<EntryPoint>]
let main argv =
    FsLibLog.LogProvider.setLoggerProvider <| FsLibLog.Providers.ConsoleProvider.create()
    SomeLib.Say.hello "Whatup" |> printfn "%s"
    Console.ReadLine() |> ignore
    0
