module JsConsoleProvider

open FsLibLog
open System
open System.Globalization

module JsLogging =
    open Fable.Core.JS

    let logTrace msg = console.trace msg
    let logDebug msg = console.debug msg
    let logInfo msg = console.info msg
    let logWarn msg = console.warn msg
    let logError msg = console.error msg

type Stack<'a>(initial) =
    let mutable stack : 'a list = initial
    member __.Push(item) = stack <- item :: stack
    member __.Push(items) = stack <- items @ stack
    member __.Pop() =
        match stack with
        | x :: xs ->
            stack <- xs
            x
        | [] -> failwith "Empty stack cannot be popped."
    member __.Items() = stack


type private ConsoleProvider () =
    let propertyStack = Stack<string * obj>([])

    let threadSafeWriter = MailboxProcessor.Start(fun inbox ->
        let rec loop () = async {
            let! (level, msg : string) = inbox.Receive()
            match level with
            | LogLevel.Trace -> JsLogging.logTrace msg
            | LogLevel.Debug -> JsLogging.logDebug msg
            | LogLevel.Info -> JsLogging.logInfo msg
            | LogLevel.Warn -> JsLogging.logWarn msg
            | LogLevel.Error -> JsLogging.logError msg
            | LogLevel.Fatal -> JsLogging.logError msg
            | _ -> printfn "Unhandled log level: %A" level
            return! loop()
        }
        loop ()
    )

    let writeMessage name logLevel (messageFunc : MessageThunk) ``exception`` formatParams =
        match messageFunc with
        | None -> true
        | Some m ->
            let formattedMsg =
                let mutable msg = m ()

                // have to do name replacements first
                for (propertyName, propertyValue) in (propertyStack.Items()) do
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

            threadSafeWriter.Post(logLevel, formattedMsg)
            true

    let addProp key value =
        propertyStack.Push( (key, value) )
        { new IDisposable with
            member __.Dispose () = propertyStack.Pop () |> ignore }

    interface ILogProvider with

        member this.GetLogger(name: string): Logger = writeMessage name
        member this.OpenMappedContext(key: string) (value: obj) (destructure: bool): System.IDisposable =
            addProp key value
        member this.OpenNestedContext(message: string): System.IDisposable =
            addProp "NDC" message

let create () =
    ConsoleProvider () :> ILogProvider

