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
    | Update of int list
    | Successor of int * int * int // key * hopsCount

let mutable numOfNodes = 0
// list of keys in SHA1 form
// Nodes = [NodeIDs in int form] [42;84..]
// Keys = [KeyID in int form] 
// NodeMappings = [Node with keys mapped to it.]
let mutable (keys: int list)= []
let mutable (nodes: int list) = []  // node42 "DSHGFIOAHDIFOH2138Y9"
let mutable (mappings: int array array) = [||]
let mutable actorList = []
let m = 13.0
let mutable numOfRequests = 0 // init inputted value
let mutable sendRequests = false
let mutable (hopsList:int list) = []
let _lock = Object()
let random = Random() 
let maxEntries = 2.0**m - 1.0

let convertToSHA1 (arg: string) =
    System.Text.Encoding.ASCII.GetBytes arg |> (new SHA1Managed()).ComputeHash

let convertBackToString (sha1: byte[]) =
    BitConverter.ToString(sha1).Replace("-", "")

let SHA1 (arg: string) = 
    let x = convertBackToString(convertToSHA1(arg))
    x

let generateNodes() = 
    for n in 0..numOfNodes-1 do
        let mutable check = true 
        while check do 
            let random = Random()
            let nodeId =  random.Next(int maxEntries)
            // Don't add duplicate nodes.
            if (not (List.contains nodeId nodes)) then 
                nodes <- List.append nodes [nodeId] 
                check <- false
    nodes

let generateKeys() =
    for k in 0..numOfNodes/2 do
        let mutable check = true 
        while check do
            let random = Random()
            let keyId = random.Next(int maxEntries)
            if (not (List.contains keyId keys)) then
                keys <- List.append keys [keyId]
                check <- false
    keys 

let rec identify (key:int) (sortedNodes: int list) (index: int) = 
    if (index >= sortedNodes.Length)
    then sortedNodes.[0] 
    elif sortedNodes.[index] >= key
    then sortedNodes.[index]
    else identify key sortedNodes (index + 1)

let rec map (key: int) (nodeForKey: int) (index: int)=
    if (index >= mappings.Length)
    then mappings <- Array.append mappings [|[|nodeForKey; key|]|]
    elif nodeForKey = mappings.[index].[0]
    then mappings.[index] <- Array.append mappings.[index] [|key|]
    else map key nodeForKey (index+1)

let findClosestNode ()= 
    // Takes all keys and all nodes
    // For each key, it finds the node that it maps to with IDENTIFY
    // Map will create an array of the mappings.
    let sortedNodes = List.sort nodes
    let sortedKeys = List.sort keys
    for key in sortedKeys do 
        let nodeForKey = identify key sortedNodes 0 // 65
        map key nodeForKey 0
    
    
// keyList is used to make sure we aren't checking for a key that we already have
let getRandomKey(keyList) = 
    let mutable key = -1
    let mutable check = true 
    while check do 
        key <- keys.[random.Next((keys.Length))]
        let mutable keyFound = false
        // keyList has a id and a key hash
        for hash in keyList do // check if current node contains key we are looking for
            if hash = key
            then 
                keyFound <- true
            // printfn "Added new one %d" nodeId
        if not keyFound
        then check <- false
    key

let checkIfFinished() =
    //printf "\ntotal requests: %i" hopsList.Length
    if (hopsList.Length >= (numOfNodes * numOfRequests))
    then 
        let mutable sum = 0
        for num in hopsList do
            sum <- sum + num 
        
        let average = float sum / (float hopsList.Length)
        printfn "Total Hops: %d\nAverage hops per requests: %f" hopsList.Length average
        Environment.Exit 0
    
let fingerTableInit (nodeId:int) = 
    let mutable fingerTable = []
    let sortedNodes = List.sort nodes // [a list of hashes]
    let mutable closestNodeIndex = 0 
    for i in 1..int(m) do
        if closestNodeIndex < sortedNodes.Length
        then
            let finger = int(( float(nodeId) + (2.0**float(i-1)) ) % (2.0**m)) 
            let hashedRowId = SHA1(finger |> string)
            if sortedNodes.[closestNodeIndex] < finger
            then // keep checking nodes until theres a node bigger than current finger, or we reach final node
                while closestNodeIndex < sortedNodes.Length && sortedNodes.[closestNodeIndex] < finger do
                    closestNodeIndex <- closestNodeIndex + 1
                if closestNodeIndex = sortedNodes.Length // if there are no nodes bigger than finger, than map finger to 1st index
                then closestNodeIndex <- 0
            fingerTable <- List.append fingerTable [closestNodeIndex] 
            //if closestNodeIndex > 0 && closestNodeIndex < sortedNodes.Length
            //then fingerTable <- List.append fingerTable [[fingerEntry;closestNodeIndex]] // it would be easier if we used number ids insead of hashes
            //then fingerTable <- List.append fingerTable [closestNodeIndex] 
    fingerTable

let getNodeFromFingerTable (currentNodeId:int) (keyId:int) = 
    //printfn "Mappings: %A" mappings
    let nodeFingerTable = fingerTableInit(currentNodeId)
    printf "\nfinger table %A nodeid: %i" nodeFingerTable currentNodeId

    let sortedNodes = List.sort nodes
    let mutable nextNodeIndex = -1
    // psuedocode from paper: "if (id E (n, id)] return successor;"
    let successorIndex = nodeFingerTable.[0] // 1st row of finger table is the successor
    let successorId = sortedNodes.[successorIndex]
    // IF key is between node and successor 
    // 1st edge case: IF key is above the highest nodeid
    // 2nd edge case: IF key is below the lowest nodeid
    if (keyId <= successorId  && keyId > currentNodeId) || (successorIndex = 0 && (currentNodeId < keyId || successorId >= keyId))
    then
        //printf "successor picked" 
        nextNodeIndex <- nodeFingerTable.[0] // successor has key
    (* psuedocode from paper: 
        for i = m downto 1
            if (finger[i] E (n, id)) // finger is "in between" node and id
            return finger[i];
            return n; 
    *)
    for i = nodeFingerTable.Length-1 downto 0 do
        let nodeIndex = nodeFingerTable.[i]
        let nodeId = sortedNodes.[nodeIndex]
        // need to calculate if finger is "in between" node and id: 
        let keyIsInFront = keyId > currentNodeId 
        let fingerIsInFront = nodeId > currentNodeId
        // if the finger table node is less than key AND it is greater than current node (UNLESS the key is also less than current node)
        //printf "\n next node: %i key id %i current node: %i bool: %b" nodeId keyId currentNodeId (nodeId < keyId && (not keyIsInFront || fingerIsInFront) && nextNodeIndex = -1 )
        if nodeId < keyId && (fingerIsInFront || not keyIsInFront) && nextNodeIndex = -1 // -1 indicates node hasnt been found
        then 
            //printf "\n node picked: %i key id %i current node: %i" nodeId keyId currentNodeId
            nextNodeIndex <- nodeIndex // this node is largest nodeid in finger table that is less than the key            

    if nextNodeIndex = -1 // if looped through finger table but no node was less than key
    then 
        //printf "\n No node matched. next node: %i keyId: %i current node: %i" sortedNodes.[nodeFingerTable.[nodeFingerTable.Length-1]] keyId currentNodeId
        nextNodeIndex <- nodeFingerTable.[nodeFingerTable.Length-1]
    nextNodeIndex

let delete id (keyList : int list) =
    let sortedNodes = List.sort nodes 
    let mutable index = 0
    for node in sortedNodes do 
        if id = node then 
            actorList.[0] <! Update keyList
        else 
            index <- index + 1
    // kill mailbox after.

let chordActor (id: int) (keyList: int list) = spawn system (string id) <| fun mailbox ->
    // printfn "Created actor with id: %s." id 
    // printfn "My keys are: %A" keyList
    // printfn "My successor is located at %i, and is %i" (fst(successor)) (snd(successor))
    // printfn "==================="
    //let mutable sentRequests = 0
    let mutable integratedKeyList = keyList
    let rec loop(sentRequests) = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        //let mutable req = sentRequests
        //printf "message recieved"
        match msg with
            | Update(newKeysToAdd) ->
                integratedKeyList <- List.append integratedKeyList newKeysToAdd
            | Successor(originalID, keyId,hops) -> 
                let keyHash = SHA1(keyId |> string) // should we be comparing key hash or the key id? adding this for now
                let newHops = hops+1 // keep track of how many hops it takes to find key by itterating by 1
                let originalHash = SHA1(string originalID)
                let idHash = SHA1(id |> string)
                // If we made it all the way back to the current user
                // Then we can assume the id doesn't exist 
                if (originalHash = idHash) // 
                then
                    printf "at original node"
                    lock _lock (fun () -> 
                        hopsList <- List.append hopsList [newHops]
                        // printfn "Key not found after %d hops" newHops
                        checkIfFinished()
                    )
                // Else, we need to check if the currentNode contains our key.
                else 
                    let mutable keyFound = false
                    //printfn " Key List: %A" keyList
                    //printf " keyBeingSearchedFor %i" keyId
                    //printfn "Mappings: %A" mappings

                    // REMINDER: KeyList only has keys that the node has so we can just compare our keyHash to every hash in keyList
                    for key in keyList do // check if current node contains key we are looking for
                        let keyBeingSearchedFor = SHA1(key |> string)
                        if keyBeingSearchedFor = keyHash
                        then 
                            keyFound <- true
                    if keyFound
                    then 
                        lock _lock (fun () -> 
                            hopsList <- List.append hopsList [newHops]
                            printfn "key found after %d hops" newHops 
                            checkIfFinished()
                        )
                        // send request every second
                    else 
                        let nextNodeIndex = getNodeFromFingerTable id keyId 
                        //printfn "Next Node Index: %d" nextNodeIndex
                        actorList.[nextNodeIndex] <! Successor(originalID, keyId, newHops)
                if (sendRequests) 
                then
                    system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds(0.0), TimeSpan.FromMilliseconds(1000.0), fun () -> 
                        if !sentRequests < numOfRequests
                        then
                            // find it in the finger table
                            incr sentRequests
                            let randomKeyIndex = random.Next(keys.Length-1)
                            let nextNodeIndex = getNodeFromFingerTable id keys.[randomKeyIndex] 
                            //printf "\n req: %d" !sentRequests
                            actorList.[nextNodeIndex] <! Successor(id, keys.[randomKeyIndex], 0)
                            // else mailbox.Context.System.Terminate() |> ignore // stop the actor after it makes a certain amount of requests
                    ) 
        // handle an incoming message
        return! loop(sentRequests) // store the new s,w into the next state of the actor
    }
    loop(ref 0)  

let createActors() = 
    // We are going to create an actor using the mappings we just made
    // If an actor has no mappings, we need to make that evident
    let sortedNodes = List.sort nodes
    let mutable index = 0
    for node in sortedNodes do // For each Node, we will check if it has a mapping in node mapping.
        let mutable keyList = [] // keyList will be a list of keys that are associated with each actor.
        for nodeMappings in mappings do
            let mappedNode = nodeMappings.[0] //nodeMappings.[0] should be the nodeID that the keys are mapped to.
            if (mappedNode = node) then 
                for i in 1 .. nodeMappings.Length-1 do 
                    keyList <- List.append keyList [nodeMappings.[i]]
        if (index = sortedNodes.Length-1)
        then actorList <- List.append actorList [chordActor node keyList] // node (because we do this for every node). keyList could be empty.
        else actorList <- List.append actorList [chordActor node keyList] 
        index <- index + 1
        // Threading.Thread.Sleep(500)
    0    



// [55; 54; 28]
// will start process of searching for keys
let findKey () = 
    let sortedNodes = List.sort nodes
    let randomKeyIndex = random.Next(keys.Length-1)
    let randomNode = random.Next(sortedNodes.Length-1)
    printf "finding key %i starting from %i \n" keys.[randomKeyIndex] sortedNodes.[randomNode]
    let nextNodeIndex = getNodeFromFingerTable sortedNodes.[randomNode] keys.[randomKeyIndex]
    actorList.[nextNodeIndex] <! Successor(sortedNodes.[randomNode], keys.[randomKeyIndex], 0) 



[<EntryPoint>]
let main argv = 
    numOfNodes <- (int argv.[0])
    numOfRequests <- (int argv.[1])
    generateNodes() |> ignore
    let sortedNodes = List.sort nodes
    printfn "Nodes: %A" sortedNodes
    generateKeys() |> ignore
    printfn "Keys: %A" keys

    findClosestNode() |> ignore
    printfn "Mappings: %A" mappings
    createActors() |> ignore
    printfn "Finished making actors."
    findKey() |> ignore
    sendRequests <- true

    // test cases
    //printf "node: %i key: %i" sortedNodes.[sortedNodes.Length-1] mappings.[0].[1]
    let testNodeIndex = getNodeFromFingerTable sortedNodes.[sortedNodes.Length-1] mappings.[0].[1]
    if sortedNodes.[testNodeIndex] <= mappings.[0].[0]
    then printf "\nedge case passed"
    else printf "\nedge case test failed"
    
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