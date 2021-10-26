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
    | Successor of string * string * int // key * hopsCount

let mutable numOfNodes = 0
// list of keys in SHA1 form
let mutable (keys: string list)= []
// list of nodes in SHA1 form
let mutable (nodes: string list) = [] 
let mutable (nodesInt : int list) = []
let mutable (nodeMappings: string array array) = [||]
let mutable actorList = []
let m = 13.0
let mutable numOfRequests = 0 // init inputted value
let mutable sendRequests = false
let mutable (hopsList:int list) = []
let _lock = Object()
let random = Random() 
let maxKey = 2.0**m - 1.0

let convertToSHA1 (arg: string) =
    System.Text.Encoding.ASCII.GetBytes arg |> (new SHA1Managed()).ComputeHash

let convertBackToString (sha1: byte[]) =
    BitConverter.ToString(sha1).Replace("-", "")

let SHA1 (arg: string) = 
    let x = convertBackToString(convertToSHA1(arg))
    x

let getNodeId() = 
    let mutable check = true 
    let mutable nodeId = "" 
    while check do
        let random = Random()
        let x =  random.Next(int maxKey)
        nodeId <- SHA1(string x)
        if (not (List.contains nodeId nodes))
        then
            if (not (List.contains x nodesInt))
            then 
                nodesInt <- List.append nodesInt [x]
                check <- false

            // printfn "Added new one %d" nodeId
    nodeId 

let rec identify (key:string) (sortedNodes:string list) (index:int) = 
    if (index >= sortedNodes.Length)
    then sortedNodes.[0]
    elif sortedNodes.[index] >= key
    then sortedNodes.[index]
    else identify key sortedNodes (index+1)

let rec map (key:string) (nodeForKey:string) (index: int)=
    if (index >= nodeMappings.Length)
    then nodeMappings <- Array.append nodeMappings [|[|nodeForKey; key|]|]
    elif (nodeForKey = nodeMappings.[index].[0])
    then nodeMappings.[index] <- Array.append nodeMappings.[index] [|key|]
    else map key nodeForKey (index+1)


// key = 63
// node = 65
// sortedNodes is in SHA1 form already
let findClosestNode ()= 
    let sortedNodes = List.sort nodes
    for key in keys do
        let nodeForKey = identify key sortedNodes 0 // 65
        map key nodeForKey 0
        // [[0;120;125;130][1;1][8;2;4;6][65;62;63]]
    nodeMappings
    
let getRandomKey(keyList) = 
    let mutable key = ""
    let mutable check = true 
    while check do 
        key <- keys.[random.Next((keys.Length))]
        let mutable keyFound = false
        // keyList has a id and a key hash
        for (k,hash) in keyList do // check if current node contains key we are looking for
            if k = key
            then 
                keyFound <- true
            // printfn "Added new one %d" nodeId
        if not keyFound
        then check <- false
    key

let generateKeys ()=
    for k in 0..numOfNodes/2 do
        let keyInitializer = getNodeId()
        // let hash = convertBackToString(convertToSHA1(keyInitializer |> string))
        // printfn "%i" keyInitializer
        let x = SHA1(keyInitializer |> string)
        keys <- List.append keys [x]
    keys

let checkIfFinished() =
    if (hopsList.Length >= (numOfNodes * numOfRequests))
    then 
        let mutable sum = 0
        for num in hopsList do
            sum <- sum + num 
        
        let average = float sum / (float hopsList.Length)
        printfn "Total Hops: %d\nAverage hops per requests: %f" hopsList.Length average
        Environment.Exit 0

let addNodesInArray ()= 
    for n in 0..numOfNodes-1 do
        let id = getNodeId()
        // let id_string = id |> string
        // let actor = [chordActor id_string ]
        // Preventing duplicates
        // if (not (List.contains id nodes))
       //  then 
        let x = SHA1(id |> string)
        nodes <- List.append nodes [x]
    // listOfActors <- List.append listOfActors actor 

let chordActor (id:string) (keyList: (string*string) list) (successor: int * string) = spawn system id <| fun mailbox ->
    // printfn "Created actor with id: %s." id 
    // printfn "My keys are: %A" keyList
    // printfn "My successor is located at %i, and is %i" (fst(successor)) (snd(successor))
    // printfn "==================="
    let mutable sentRequests = 0
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()

        match msg with
            | Successor(originalID, keyId,hops) -> 
                let keyHash = convertBackToString(convertToSHA1(keyId)) // should we be comparing key hash or the key id? adding this for now
                // printfn "Actor: %s recieved key query for %d" id keyId
                let newHops = hops+1 // keep track of how many hops it takes to find key by itterating by 1
                let originalHash = convertBackToString(convertToSHA1(originalID))
                let idHash = convertBackToString(convertToSHA1(id))
                if (originalHash = idHash)
                then
                    lock _lock (fun () -> 
                        hopsList <- List.append hopsList [newHops]
                        // printfn "Key not found after %d hops" newHops
                        checkIfFinished()
                    )
                    
                    
                else 
                    let mutable keyFound = false
                    // keyList has a id and a key hash
                    for (k,hash) in keyList do // check if current node contains key we are looking for
                        if keyHash = hash
                        then 
                            keyFound <- true
                    if keyFound
                    then 
                        lock _lock (fun () -> 
                            hopsList <- List.append hopsList [newHops]
                            // printfn "key found after %d hops" newHops // not sure what to do here?
                            checkIfFinished()
                        )
                        // send request every second
                    else actorList.[(fst(successor))] <! Successor(originalID, keyId, newHops)
                if (sendRequests) 
                then
                    system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds(0.0), TimeSpan.FromMilliseconds(1000.0), fun () -> 
                        if sentRequests < numOfRequests
                        then
                            let randomKey = getRandomKey(keyList)
                            sentRequests <- sentRequests + 1
                            actorList.[fst(successor)] <! Successor(id, randomKey,0) 
                            // else mailbox.Context.System.Terminate() |> ignore // stop the actor after it makes a certain amount of requests
                    ) 


        // handle an incoming message
        return! loop() // store the new s,w into the next state of the actor
    }
    loop()  

let createActors() = 
    let sortedNodes = List.sort nodes
    // printfn "Sorted Nodes: %A" sortedNodes
    // printfn "==============================="
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
        // Threading.Thread.Sleep(500)
    0    

// [55; 54; 28]
// will start process of searching for keys
let findKey () = 
    let sortedNodes = List.sort nodes
    let randomKey = random.Next(keys.Length-1)
    let randomNode = random.Next(sortedNodes.Length-1)
    actorList.[randomNode] <! Successor(sortedNodes.[randomNode], keys.[randomKey],0) 



[<EntryPoint>]
let main argv = 
    numOfNodes <- (int argv.[0])
    numOfRequests <- (int argv.[1])
    addNodesInArray()
    printfn "Finished making nodes in array"
    printfn "Nodes: %A" nodes
    generateKeys() |> ignore
    printfn "Finished generating keys"

    printfn "Keys: %A" keys
    printfn"Printing list"
    findClosestNode() |> ignore
    printfn "Finished mappings"
    printfn "Mappings: %A" nodeMappings
    createActors() |> ignore
    printfn "Finished making actors."
    findKey() |> ignore
    sendRequests <- true
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