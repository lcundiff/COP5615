open System
//open System.IO
open System.Security.Cryptography
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


let findSuccessor(id)
    if (id ∈(n, successor])
        successor;
    else
        n= closest preceding node(id);
        n′.find successor(id);
        // search the local table for the highest predecessor of id
        n.closest preceding node(id)
        for i = m downto 1
            if (finger[i] ∈(n, id))
                return finger[i];
        n;

let convertToSHA1 (arg: string) =
    System.Text.Encoding.ASCII.GetBytes arg |> (new SHA1Managed()).ComputeHash

let convertBackToString (sha1: byte[]) =
    BitConverter.ToString(sha1).Replace("-", "")




let mutable listOfActors = []
let mutable keys = []
let mutable listOfNodeIds = [] 

let m = 6.0
let chordActor (id:string) (topologyPosition:int list) = spawn system id <| fun mailbox ->
    
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender() 
        

        // handle an incoming message
        return! loop() // store the new s,w into the next state of the actor
    }
    loop()  

let getNodeId = 
    let max = 2.0**m - 1.0
    let random = Random()
    let nodeId = random.Next(max |> int)
    nodeId

let findClosestNode keyInitializer = 
    for n in 0..numOfNodes 

let generateKeys =
    for k in 0..numOfNodes/2 do
        let keyInitializer = getNodeId
        findClosestNode(keyInitializer)
        let hash = convertBackToString(convertToSHA1(keyInitializer |> string))
        
        keys <- List.append keys [hash]
    keys


let addNodesInArray = 
    for n in 0..numOfNodes do 
        let id = getNodeId |> string
        let actor = [chordActor id ]
        listOfNodeIds <- List.append listOfNodeIds id
        listOfActors <- List.append listOfActors actor 

addNodesInArray
keys = generateKeys


let input2 = System.Console.ReadLine() |> ignore