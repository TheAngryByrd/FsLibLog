namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    // let logger = LogProvider.getLoggerByName "SomeLibrary.Say"
    let rec logger = LogProvider.getLoggerByQuotation <@ logger @>
    type AdditionalData = {
        Name : string
    }

    let rec myModule = LogProvider.getModuleType <@ myModule @>
    // Example Log Output:
    // 16:23 [Information] <SomeLib.Say> () "Captain" Was said hello to - {"UserContext": {"Name": "User123", "$type": "AdditionalData"}, "FunctionName": "hello"}
    let hello name  =
        let logger2 = LogProvider.getLoggerByFunc ()
        // Starts the log out as an Informational log
        logger2.info(
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
