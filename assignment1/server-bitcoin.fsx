open System
open System.IO
open System.Text;
open System.Security.Cryptography
open System.Diagnostics
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
open Akka.Configuration
open Akka.FSharp
open Akka.Actor

let config =
    Configuration.parse
        @"akka {
            actor.provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            remote.helios.tcp {
                hostname = localhost
                port = 9001
            }
        }"

let system = System.create "my-system" config

type Message =
    | Stop
    | Hash
    | Start of string
    | Coin of string
    | Mine of string
    | GetCoin of string * string

[<EntryPoint>]
let main args =
    use system = System.create "remote-system" config
    
    0






