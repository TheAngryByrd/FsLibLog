module App

open Browser.Dom
open System
open FsLibLog

LogProvider.setLoggerProvider
<| JsConsoleProvider.create ()

// Mutable variable to count the number of times we clicked the button
let mutable count = 0

let logger = LogProvider.getLoggerByName "TestLogger"

let myButton =
    document.querySelector (".my-button") :?> Browser.Types.HTMLButtonElement

// Register our listener
myButton.onclick <-
    fun _ ->
        logger.info (Log.setMessage "Button clicked.")
        count <- count + 1

        let wasSent =
            logger.info' (
                Log.setMessage "Button clicked, count is now {count}"
                >> Log.addContext "count" count
            )

        myButton.innerText <- sprintf "You clicked: %i time(s)" count

let libOutput = SomeLib.Say.useOperators "username"
printfn "Library output: %s" libOutput
