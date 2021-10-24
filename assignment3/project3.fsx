open System
//open System.IO
open System.Text;
open System.Diagnostics
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
open Akka.Configuration
open Akka.FSharp
//open type System.Math; 
open System.Collections.Generic 
open Akka.Actor

let system = System.create "my-system" <| ConfigurationFactory.Default()

type Message =
    | Message of string

printfn "input ready"
let inputLine = Console.ReadLine() 
let splitLine = (fun (line : string) -> Seq.toList (line.Split ' '))
let inputParams = splitLine inputLine
let numOfNodes = inputParams.[0] |> int
let topology = inputParams.[1]
let alg = inputParams.[2]

let proc = Process.GetCurrentProcess()
let cpuTime = proc.TotalProcessorTime
let sw = Stopwatch.StartNew()


let input2 = System.Console.ReadLine() |> ignore