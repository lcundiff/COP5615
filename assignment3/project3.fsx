open System
//open System.IO
open System.Security.Cryptography
open System.Text;
open System.Diagnostics
#r "nuget: Akka.FSharp" 
open Akka.Configuration
open Akka.FSharp
//open type System.Math; 
open Akka.Actor

let system = ActorSystem.Create("FSharp")

type Message =
    | Message of string

let mutable numOfNodes = 0
let convertToSHA1 (arg: string) =
    System.Text.Encoding.ASCII.GetBytes arg |> (new SHA1Managed()).ComputeHash

let convertBackToString (sha1: byte[]) =
    BitConverter.ToString(sha1).Replace("-", "")




// let mutable listOfActors = []
let mutable keys = []
let mutable nodes = [] 
let mutable (nodeMappings: int array array) = [||]
let mutable actorList = []
let m = 6.0


let getNodeId() = 
    let max = 2.0**m - 1.0
    let random = Random()
   
    let nodeId = random.Next((int max))
    nodeId 

let rec identify (key:int) (sortedNodes:int list) (index:int) = 
    if (index >= sortedNodes.Length)
    then sortedNodes.[0]
    elif sortedNodes.[index] >= key
    then sortedNodes.[index]
    else identify key sortedNodes (index+1)

let rec map (key:int) (nodeForKey:int) (index: int)=
    if (index >= nodeMappings.Length)
    then nodeMappings <- Array.append nodeMappings [|[|nodeForKey; key|]|]
    elif (nodeForKey = nodeMappings.[index].[0])
    then nodeMappings.[index] <- Array.append nodeMappings.[index] [|key|]
    else map key nodeForKey (index+1)

// key = 63
// node = 65
let findClosestNode ()= 
    let sortedNodes = List.sort nodes
    for k in keys do
        let nodeForKey = identify k sortedNodes 0 // 65
        map k nodeForKey 0
        // [[0;120;125;130][1;1][8;2;4;6][65;62;63]]
    nodeMappings
    
let generateKeys ()=
    for k in 0..numOfNodes/2 do
        let keyInitializer = getNodeId()
        // let hash = convertBackToString(convertToSHA1(keyInitializer |> string))
        // printfn "%i" keyInitializer
        keys <- List.append keys [keyInitializer]
    keys


let addNodesInArray ()= 
    for n in 0..numOfNodes-1 do
        let id = getNodeId()
        // let id_string = id |> string
        // let actor = [chordActor id_string ]
        // Preventing duplicates
        if (not (List.contains id nodes))
        then nodes <- List.append nodes [id]
    // listOfActors <- List.append listOfActors actor 

let chordActor (id:string) (keyList: (int*string) list) successor = spawn system id <| fun mailbox ->
    printfn "Created actor with id: %s." id 
    printfn "My keys are: %A" keyList
    printfn "My successor is located at %i, and is %i" (fst(successor)) (snd(successor))
    printfn "==================="

    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender() 
        

        // handle an incoming message
        return! loop() // store the new s,w into the next state of the actor
    }
    loop()  

let createActors() = 
    let sortedNodes = List.sort nodes
    printfn "Sorted Nodes: %A" sortedNodes
    printfn "==============================="
    let mutable index = 0
    for node in sortedNodes do
        let mutable pairList = []
        for keyList in nodeMappings do
            if (keyList.[0] = node) then 
                for i in 1 .. keyList.Length-1 do 
                    pairList <- List.append pairList [(keyList.[i], convertBackToString(convertToSHA1(string keyList.[i])))]
        if (index = sortedNodes.Length-1)
        then actorList <- List.append actorList [chordActor (string node) pairList (0, sortedNodes.[0])]
        else actorList <- List.append actorList [chordActor (string node) pairList (index + 1, sortedNodes.[index+1])]
        index <- index + 1
        Threading.Thread.Sleep(500)
    0    

// [55; 54; 28]
// 

[<EntryPoint>]
let main argv = 
    numOfNodes <- (int argv.[0])
    addNodesInArray()
    printfn "Nodes: %A" nodes
    generateKeys()
    printfn "Keys: %A" keys
    // printfn"Printing list"
    findClosestNode()
    printfn "Mappings: %A" nodeMappings
    createActors()
    // The KeyValue Mappings are in nodeMappings [|55; 24; 28|] [|9;5|].. to access 55 you would do nodeMappings.[0].[0]
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code


// 55, 27, 9, 62, 17
// Actor55 [SHA1KEYS-54, SHA1KEY-28] 
// Actor 27 []
// Actor9 [5]
// Actor62 ..

// ACtor 55 isn't going to look for key 5
// SHA1(5) -> AA490yhdf0as9h0312h4