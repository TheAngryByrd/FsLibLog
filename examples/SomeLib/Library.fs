namespace SomeLib
open FsLibLog
open FsLibLog.Types
open System
open FsLibLog.Operators

module Say =

    type AdditionalData = {
        Name : string
    }

#if !FABLE_COMPILER
    // let logger = LogProvider.getLoggerByName "SomeLibrary.Say"
    let rec logger = LogProvider.getLoggerByQuotation <@ logger @>
#else
    let logger = LogProvider.getLoggerByName "SomeLib.Say"
#endif
    // Example Log Output:
    // 16:23 [Information] <SomeLib.Say> () "Captain" Was said hello to - {"UserContext": {"Name": "User123", "$type": "AdditionalData"}, "FunctionName": "hello"}
    let hello name  =
#if !FABLE_COMPILER
        // if you're not using fable, you can create a more specific logger for a given function
        // Fable would have to request by a more specific name, and can't create via function
        let logger = LogProvider.getLoggerByFunc ()
#endif
        // Starts the log out as an Informational log
        logger.info(
            Log.setMessage "{name} Was said hello to"
            // MessageTemplates require the order of parameters to be consistent with the tokens to replace
            >> Log.addParameter name
            // This adds additional context to the log, it is not part of the message template
            // This is useful for things like MachineName, ProcessId, ThreadId, or anything that doesn't easily fit within a MessageTemplate
            // This is the same as calling `LogProvider.openMappedContext` right before logging.
            >> Log.addContext "FunctionName" "hello"
            // This is the same as calling `LogProvider.openMappedContextDestucturable`  right before logging.
            >> Log.addContextDestructured "UserContext"  {Name = "User123"}
        )
        sprintf "hello %s." name


    // Example Log Output:
    // 16:23 [Debug] <SomeLib.Say> () In nested - {"DestructureTrue": {"Name": "Additional", "$type": "AdditionalData"}, "DestructureFalse": "{Name = \"Additional\";}", "Value": "bar"}
    // [Information] <SomeLib.Say> () "Commander" Was said hello to - {"UserContext": {"Name": "User123", "$type": "AdditionalData"}, "FunctionName": "hello", "DestructureTrue": {"Name": "Additional", "$type": "AdditionalData"}, "DestructureFalse": "{Name = \"Additional\";}", "Value": "bar"}
    let nestedHello name =
        // This sets additional context to any log within scope
        // This is useful if you want to add this to all logs within this given scope
        use x = LogProvider.openMappedContext "Value" "bar"
        // This doesn't destructure the record and calls ToString on it
        use x = LogProvider.openMappedContext "DestructureFalse" {Name = "Additional"}
        // This does destructure the record,  Destructuring can be expensive depending on how big the object is.
        use x = LogProvider.openMappedContextDestucturable "DestructureTrue" {Name = "Additional"} true

        logger.debug(
            Log.setMessage "In nested"
        )
        // The log in `hello` should also have these additional contexts added
        hello name


    // Example Log Output:
    // 16:23 [Error] <SomeLib.Say> () "DaiMon" was rejected. - {}
    // System.Exception: Sorry DaiMon isnt valid
    //    at Microsoft.FSharp.Core.PrintfModule.PrintFormatToStringThenFail@1647.Invoke(String message)
    //    at SomeLib.Say.fail(String name) in /Users/jimmybyrd/Documents/GitHub/FsLibLog/examples/SomeLib/Library.fs:line 57
    let fail name =
        try
            failwithf "Sorry %s isnt valid" name
        with e ->
            // Starts the log out as an Error log
            logger.error(
                Log.setMessage "{name} was rejected."
                // MessageTemplates require the order of parameters to be consistent with the tokens to replace
                >> Log.addParameter name
                // Adds an exception to the log
                >> Log.addException  e
            )

    // Example Log Output:
    // 2021-09-15T20:34:14.9060810-04:00 [Information] <SomeLib.Say> () The user {"Name": "Ensign Kim", "$type": "AdditionalData"} has requested a reservation date of "2021-09-16T00:34:14.8853360+00:00"
    let interpolated (person : AdditionalData) (reservationDate : DateTimeOffset) =
        // Starts the log out as an Info log
        logger.info(
            // Generates a message template via a specific string intepolation syntax.
            // Add the name of the property after the expression
            // for example: "person" will be logged as "user" and "reservationDate" as "reservationDate"
            Log.setMessageI $"The user {person:User} has requested a reservation date of {reservationDate:ReservationDate} "
        )

    // Creates a log similar to the other logs, but using operators
    let useOperators name =
        !!! "{name} said hello!"
        >>!- ("name", name)
        |> logger.info
        sprintf "%s says \"Hello\"" name
