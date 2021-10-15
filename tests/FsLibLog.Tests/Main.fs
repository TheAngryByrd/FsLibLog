module ExpectoTemplate

#if FABLE_COMPILER
open Fable.Mocha
#else
open Expecto
#endif

let allTests = testList "All Tests" [
    Tests.tests
]

// [<EntryPoint>]
let main argv =
    #if FABLE_COMPILER
    printfn "Running tests"
    Mocha.runTests allTests
    #else
    Tests.runTestsWithArgs defaultConfig argv allTests
    #endif
