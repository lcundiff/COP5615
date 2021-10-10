
open System
// Define a new function to print a name.
// It must be defined before it is called in the main function.
let printGreeting name =
    printfn "Hello %s from F#!" name

[<EntryPoint>]
let main argv =
    // Call your new function!
    printGreeting "Logan"
    0 // return an integer exit code
