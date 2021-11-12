module Tests


#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif
open FsLibLog
open FsLibLog.Types
open FsLibLog.Operators
open System

#if FABLE_COMPILER
module Expect =
    let contains sequence element message =
        match sequence |> Seq.tryFind ((=) element) with
        | Some _ -> ()
        | None ->
            failwithf "%s. Sequence did not contain %A." message element
#endif

let private noopDisposable = {
    new IDisposable with
        member __.Dispose() = ()
}
type TestProvider () as this =
    let writeMessage name loglevel messageThunk exn parameters =
        this.LogLevel <- Some loglevel
        this.MessageThunk <- messageThunk
        this.Exception <- exn
        this.Parameters <- parameters
        true
    let addProp key value destructure =
        this.Contexts.Add(key,destructure,value)
        noopDisposable

    member val LoggerName = "" with get, set
    member val LogLevel = None with get,set
    member val MessageThunk = None with get,set
    member val Exception = None with get,set
    member val Parameters : array<obj> = Array.empty with get,set
    member val Contexts : ResizeArray<string*bool*obj> = ResizeArray<string*bool*obj>() with get

    interface ILogProvider with

        member this.GetLogger(name: string): Logger =
            this.LoggerName <- name
            writeMessage name
        member this.OpenMappedContext(key: string) (value: obj) (destructure: bool): System.IDisposable =
            addProp key value destructure
        member this.OpenNestedContext(message: string): System.IDisposable =
            addProp "NDC" message false


module SomeOtherModule =
    // should only be used in getLoggerByQuotation
    let provider = TestProvider()
    LogProvider.setLoggerProvider provider
#if !FABLE_COMPILER
    let rec loggerQuotation = LogProvider.getLoggerByQuotation <@ loggerQuotation @>
#endif

let private getNewProvider () =
    let provider = TestProvider()
    LogProvider.setLoggerProvider provider
    provider

#if !FABLE_COMPILER
let someFunction () =
    let logger = LogProvider.getLoggerByFunc()
    ()
#endif

type Dog = {
    Name : string
    Age : int
}



let tests =
    testSequenced <|
    testList "FsLibLog" [
        testList "getting logger" [
#if !FABLE_COMPILER
            testCase "LogProvider.getLoggerByFunc" <| fun _ ->
                let provider = getNewProvider()
                someFunction()
                Expect.equal provider.LoggerName "Tests.someFunction" ""

            testCase "LogProvider.getLoggerByQuotation" <| fun _ ->
                Expect.equal SomeOtherModule.provider.LoggerName "Tests+SomeOtherModule" ""
#endif
            testCase "LogProvider.getLoggerByName" <| fun _ ->
                let provider = getNewProvider()
                let loggerName = "Hello.World"
                let logger = LogProvider.getLoggerByName loggerName
                Expect.equal provider.LoggerName loggerName ""

            testCase "LogProvider.getLoggerByType" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByType typeof<string>
                Expect.equal provider.LoggerName "System.String" ""

            testCase "LogProvider.getLoggerFor" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerFor<string>()
                Expect.equal provider.LoggerName "System.String" ""
        ]

        testList "setting verbosity" [
            testCase "trace" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "trace"
                logger.trace id
                Expect.equal provider.LogLevel (Some LogLevel.Trace) ""
            testCase "trace'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "trace'"
                logger.trace' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Trace) ""
            testCase "debug" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "debug"
                logger.debug id
                Expect.equal provider.LogLevel (Some LogLevel.Debug) ""
            testCase "debug'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "debug'"
                logger.debug' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Debug) ""
            testCase "info" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "info"
                logger.info id
                Expect.equal provider.LogLevel (Some LogLevel.Info) ""
            testCase "info'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "info'"
                logger.info' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Info) ""
            testCase "warn" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "warn"
                logger.warn id
                Expect.equal provider.LogLevel (Some LogLevel.Warn) ""
            testCase "warn'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "warn'"
                logger.warn' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Warn) ""
            testCase "error" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "error"
                logger.error id
                Expect.equal provider.LogLevel (Some LogLevel.Error) ""
            testCase "error'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "error'"
                logger.error' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Error) ""
            testCase "fatal" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "fatal"
                logger.fatal id
                Expect.equal provider.LogLevel (Some LogLevel.Fatal) ""
            testCase "fatal'" <| fun _ ->
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "fatal'"
                logger.fatal' id |> ignore
                Expect.equal provider.LogLevel (Some LogLevel.Fatal) ""
        ]

        testList "setting log" [
            testCase "setMessage" <| fun _ ->
                let provider = getNewProvider()
                let message = "test message"
                let logger = LogProvider.getLoggerByName "setMessage"
                logger.fatal(Log.setMessage message)
                let actual = provider.MessageThunk.Value()
                Expect.equal actual "test message" ""

            testCase "setMessage operator (!!! )" <| fun _ ->
                let provider = getNewProvider()
                let message = "test message"
                let logger = LogProvider.getLoggerByName "!!!"
                logger.fatal(!!!  message)
                let actual = provider.MessageThunk.Value()
                Expect.equal actual "test message" ""

            testCase "setMessageThunk" <| fun _ ->
                let provider = getNewProvider()
                let message = "test message"
                let logger = LogProvider.getLoggerByName "setMessageThunk"
                logger.fatal(Log.setMessageThunk (fun () -> message))
                let actual = provider.MessageThunk.Value()
                Expect.equal actual "test message" ""

            testCase "addParameter" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName "addParameter"
                logger.fatal(Log.addParameter parameter)
                let actual = provider.Parameters
                Expect.equal actual [|parameter|] ""

            testCase "addParameter operator (>>!)" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName ">>!"
                logger.fatal(!!!  "" >>! parameter )
                let actual = provider.Parameters
                Expect.equal actual [|parameter|] ""

            testCase "addParameters" <| fun _ ->
                let provider = getNewProvider()
                let parameters = [ box "someParmeter"] // |> List.map box
                let logger = LogProvider.getLoggerByName "addParameters"
                logger.fatal(Log.addParameters parameters)
                let actual = provider.Parameters
                Expect.equal actual (Array.ofList parameters) ""

            testCase "addContext" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName "addContext"
                logger.fatal(Log.addContext "name" parameter)
                Expect.contains provider.Contexts ("name",false, box parameter) ""

            testCase "addContext operator (>>!-)" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName ">>!-"
                logger.fatal(!!! "" >>!- ("name",parameter))
                Expect.contains provider.Contexts ("name",false, box parameter) ""

            testCase "addContextDestructured" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName "addContextDestructured"
                logger.fatal(Log.addContextDestructured "name" parameter)
                Expect.contains provider.Contexts ("name",true, box parameter) ""

            testCase "addContextDestructured operator (>>!+)" <| fun _ ->
                let provider = getNewProvider()
                let parameter = "someParmeter"
                let logger = LogProvider.getLoggerByName ">>!+"
                logger.fatal(!!! "" >>!+ ("name",parameter))
                Expect.contains provider.Contexts ("name",true, box parameter) ""

            testCase "addException" <| fun _ ->
                let provider = getNewProvider()
                let ex = Exception()
                let logger = LogProvider.getLoggerByName "addException"
                logger.fatal(Log.addException ex)
                let actual = provider.Exception
                Expect.equal actual (Some ex) ""

            testCase "addExn" <| fun _ ->
                let provider = getNewProvider()
                let ex = Exception()
                let logger = LogProvider.getLoggerByName "addExn"
                logger.fatal(Log.addExn ex)
                let actual = provider.Exception
                Expect.equal actual (Some ex) ""

            testCase "addExn operator (>>!!)" <| fun _ ->
                let provider = getNewProvider()
                let ex = Exception()
                let logger = LogProvider.getLoggerByName ">>!!"
                logger.fatal(!!!  "" >>!! ex)
                let actual = provider.Exception
                Expect.equal actual (Some ex) ""

            testCase "openMappedContextDestucturable true" <| fun _ ->
                let parameter = "some parameter"
                let provider = getNewProvider()
                use __ = LogProvider.openMappedContextDestucturable "name" parameter true
                Expect.contains provider.Contexts ("name",true, box parameter) ""

            testCase "openMappedContextDestucturable false" <| fun _ ->
                let parameter = "some parameter"
                let provider = getNewProvider()
                use __ = LogProvider.openMappedContextDestucturable "name" parameter false
                Expect.contains provider.Contexts ("name",false, box parameter) ""

            testCase "openMappedContext" <| fun _ ->
                let parameter = "some parameter"
                let provider = getNewProvider()
                use __ = LogProvider.openMappedContext "name" parameter
                Expect.contains provider.Contexts ("name",false, box parameter) ""


            testCase "openNestedContext false" <| fun _ ->
                let parameter = "some parameter"
                let provider = getNewProvider()
                use __ = LogProvider.openNestedContext parameter
                Expect.contains provider.Contexts ("NDC",false, box parameter) ""
#if !FABLE_COMPILER
            testCase "setMessageInterpolated simple" <| fun _ ->
                let parameter = "problemChild123"
                let provider = getNewProvider()

                let logger = LogProvider.getLoggerByName "setMessageInterpolated"
                logger.fatal(Log.setMessageInterpolated $"This {parameter:user} did something with this")
                let actualMessage = provider.MessageThunk.Value()
                Expect.equal actualMessage "This {user} did something with this" ""
                Expect.contains provider.Contexts ("user",false, box parameter) ""

            testCase "setMessageInterpolated object" <| fun _ ->
                let parameter = {Name = "Mustard" ; Age = 42}
                let provider = getNewProvider()

                let logger = LogProvider.getLoggerByName "setMessageInterpolated"
                logger.fatal(Log.setMessageInterpolated $"This {parameter:user} did something with this")
                let actualMessage = provider.MessageThunk.Value()
                Expect.equal actualMessage "This {user} did something with this" ""
                Expect.contains provider.Contexts ("user",true, box parameter) ""

            testCase "setMessageInterpolated anonymous record" <| fun _ ->
                let parameter = {|Foo = 123|}
                let provider = getNewProvider()
                let logger = LogProvider.getLoggerByName "setMessageInterpolated"
                logger.fatal(Log.setMessageInterpolated $"This {parameter:user} did something with this")
                let actualMessage = provider.MessageThunk.Value()
                Expect.equal actualMessage "This {user} did something with this" ""
                provider.Contexts |> Seq.iter(printfn "%A")
                Expect.contains provider.Contexts ("user",true, box parameter) ""
#endif
        ]

    ]


