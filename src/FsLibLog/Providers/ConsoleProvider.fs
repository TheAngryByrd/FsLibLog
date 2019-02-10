namespace FsLibLog

module ConsoleProvider =
    open System
    open System.Globalization
    open Types

    let isAvailable () = true

    type private ConsoleProvider () =
        let threadSafeWriter =  MailboxProcessor.Start(fun inbox ->
            let rec loop () = async {
                let! (consoleColor, message : string) = inbox.Receive()
                let originalForground = Console.ForegroundColor
                try
                    Console.ForegroundColor <- consoleColor
                    do! Console.Out.WriteLineAsync(message) |> Async.AwaitTask
                finally
                    Console.ForegroundColor <- originalForground
                return! loop ()
            }
            loop ()
        )
        let levelToColor =
            Map([
                (LogLevel.Fatal, ConsoleColor.DarkRed)
                (LogLevel.Error, ConsoleColor.Red)
                (LogLevel.Warn, ConsoleColor.Yellow)
                (LogLevel.Info, ConsoleColor.White)
                (LogLevel.Debug, ConsoleColor.Gray)
                (LogLevel.Trace, ConsoleColor.DarkGray)
            ])
        let writeMessage name logLevel (messageFunc : MessageThunk) ``exception`` formatParams =
            match messageFunc with
            | None -> true
            | Some m ->
                let color =
                    match levelToColor |> Map.tryFind(logLevel) with
                    | Some color -> color
                    | None -> Console.ForegroundColor
                let formattedMsg =
                    let msg = String.Format(CultureInfo.InvariantCulture, m (), formatParams)
                    let msg =
                        match ``exception`` with
                        | Some (e : exn) ->
                            String.Format("{0} | {1}", msg, e.ToString())
                        | None ->
                            msg
                    String.Format("{0} | {1} | {2} | {3}", DateTime.UtcNow, logLevel, name, msg)

                threadSafeWriter.Post(color, formattedMsg)
                true

        interface ILogProvider with

            member this.GetLogger(name: string): Logger =
                writeMessage name
            member this.OpenMappedContext(arg1: string) (arg2: obj) (arg3: bool): System.IDisposable =
                failwith "Not Implemented"
            member this.OpenNestedContext(arg1: string): System.IDisposable =
                failwith "Not Implemented"

    let create () =
        ConsoleProvider () :> ILogProvider
