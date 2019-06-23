namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    let logger = LogProvider.getCurrentLogger()

    type AdditionalData = {
        Name : string
    }

    let hello name  =
        logger.info(
            Log.setMessage "{name} Was said hello to"
            >> Log.addParameter name
            >> Log.addContext "FunctionName" "hello"
            >> Log.addContextDestructured "UserContext"  {Name = "User123"}
        )
        sprintf "hello %s." name

    let fail name =
        use x = LogProvider.openMappedContext "Value" "bar"
        use x = LogProvider.openMappedContext "DestructureFalse" {Name = "Additional"}
        use x = LogProvider.openMappedContextDestucturable "DestructureTrueValue" "lol" true
        use x = LogProvider.openMappedContextDestucturable "DestructureTrue" {Name = "Additional"} true
        try
            failwithf "Sorry %s isnt valid" name
        with e ->
            logger.error(
                Log.setMessage "{name} was rejected."
                >> Log.addParameter name
                >> Log.addException  e
            )
