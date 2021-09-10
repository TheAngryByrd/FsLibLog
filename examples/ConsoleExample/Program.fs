open System
open FsLibLog

/// WARN: This does not provide support for [MessageTemplates](https://messagetemplates.org/) so this will fail for message formats intended for structured logging.  This is only used for simple display implementations purposes only.
module ConsoleProvider =
    open System
    open System.Globalization

    let isAvailable () = true

    type private ConsoleProvider () =
        let propertyStack = System.Collections.Generic.Stack<string * obj>()

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
                    let mutable msg = m ()

                    // have to do name replacements first
                    for (propertyName, propertyValue) in (Seq.rev propertyStack) do
                      let name = sprintf "{%s}" propertyName
                      let value = sprintf "%A" propertyValue
                      msg <- msg.Replace(name, value)

                    // it's possible for msg at this point to have what looks like format
                    // specifiers, which will cause String.Format to puke
                    let msg = msg.Replace("{", "{{").Replace("}", "}}")

                    // then c# numeric replacements
                    let msg = String.Format(CultureInfo.InvariantCulture, msg , formatParams)

                    // then exception
                    let msg =
                        match ``exception`` with
                        | Some (e : exn) ->
                            String.Format("{0} | {1}", msg, e.ToString())
                        | None ->
                            msg

                    // stitch it all together
                    String.Format("{0} | {1} | {2} | {3}", DateTime.UtcNow, logLevel, name, msg)

                threadSafeWriter.Post(color, formattedMsg)
                true

        let writeMessageFunc logger =
            Func<LogLevel, MessageThunk, Exception option, obj array, bool>(
                fun logLevel (messageFunc : MessageThunk) ``exception`` formatParams ->
                    writeMessage logger logLevel messageFunc ``exception`` formatParams
            )

        let addProp key value =
          propertyStack.Push(key, value)
          { new IDisposable with
              member __.Dispose () = propertyStack.Pop () |> ignore }

        interface ILogProvider with

            member this.GetLogger(name: string): Logger =
                writeMessageFunc name
            member this.OpenMappedContext(key: string) (value: obj) (destructure: bool): System.IDisposable =
                addProp key value
            member this.OpenNestedContext(message: string): System.IDisposable =
                addProp "NDC" message

    let create () =
        ConsoleProvider () :> ILogProvider

[<EntryPoint>]
let main argv =
    FsLibLog.LogProvider.setLoggerProvider <| ConsoleProvider.create()
    SomeLib.Say.hello "Whatup" |> printfn "%s"
    Console.ReadLine() |> ignore
    0
