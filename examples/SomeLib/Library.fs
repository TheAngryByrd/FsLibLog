namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    let logger = LogProvider.getCurrentLogger()

    type AdditionalData = {
        Name : string
    }

    let hello name  =
        logger.warn(
            Log.setMessage "{name} Was said hello to"
            >> Log.addParameter name
        )
        sprintf "hello %s." name

    let fail name =
        use x = LogProvider.openMappedContext "Foo" "bar"
        use x = LogProvider.openMappedContext "Bar" {Name = "Additional"}
        use x = LogProvider.openMappedContextDestucturable "Baz" {Name = "Additional"} true
        try
            failwithf "Sorry %s isnt valid" name
        with e ->
            logger.error(
                Log.setMessage "{name} was rejected."
                >> Log.addParameter name
                >> Log.addException  e
            )
