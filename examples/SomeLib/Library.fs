namespace SomeLib
open FsLibLog
open FsLibLog.Types

module Say =
    let logger = LogProvider.getCurrentLogger()

    let hello name =
        logger.warn(
            Log.setMessage ("{name} Was said hello to" )
            >> Log.addParameters [name]
        )
        sprintf "hello %s" name
