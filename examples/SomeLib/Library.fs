namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    let logger = LogProvider.getCurrentLogger()

    type AdditionalData = {
        Name : string
    }

    let hello name  =
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
        // The log in `hello`  should also have these additional contexts added
        hello name


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
