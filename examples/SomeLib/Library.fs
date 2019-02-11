namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    let logger = LogProvider.getCurrentLogger()

    let hello name  =
        logger.warn(
            Log.setMessage "{name} Was said hello to"
            >> Log.addParameter name
        )
        sprintf "hello %s." name

    let fail name =
        try
            failwithf "Sorry %s isnt valid" name
        with e ->
            logger.error(
                Log.setMessage "{name} was rejected."
                >> Log.addParameter name
                >> Log.addException  e
            )
