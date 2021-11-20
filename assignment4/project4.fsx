open System
open System.Security.Cryptography
open System.Text;
open System.Diagnostics
#r "nuget: Akka.FSharp" 
open Akka.Configuration
open Akka.FSharp
open Akka.Actor

let system = ActorSystem.Create("FSharp")

type Message =
    | Tweet of string
    | Subscribe of string
    | Unsubscribe of string



[<EntryPoint>]
let main argv = 
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
