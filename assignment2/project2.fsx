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
    | Stop
    | StartSum of int * string
    | SumWeight of float * float
    | Estimate
    |StartGossip of int * String
    |Rumor of string

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

let mutable listOfActors = [] //[0..inputParams.[0] |> int] // list of actors as long as the nodes inputted
//let mutable gridOfActors = [listOfActors]
let mutable cubeOfActors = []
let allActors = Map.empty

let find3dNeighbor (index: int list) = 
    (*let neighbor1 = [x-1,y,z]
    let neighbor2 = [x+1,y,z]
    let neighbor3 = [x,y-1,z]
    let neighbor4 = [x,y+1,z]
    let neighbor5 = [x,y,z-1]
    let neighbor6 = [x,y,z+1]*)
    
    // which axis to find neighbor on
    let random = Random()
    let mutable properIndexNotFound = true
    let mutable firstIndex = 0
    let mutable secondIndex = 0
    let mutable thirdIndex = 0

    while properIndexNotFound do // loop until we are not index out of bounds
        let randomAxisIndex = random.Next(3)
        let mutable axisIndex = index.[randomAxisIndex] 
        let randomDirection = random.Next(2) // direction will either be 0 or 1 (forward/backward on axis)
        // which direction 
        if randomDirection = 1
        then axisIndex <- axisIndex + 1
        else axisIndex <- axisIndex - 1 
        let neighbor = index |> List.mapi (fun i v -> if i = randomAxisIndex then axisIndex else v) 
        firstIndex <- neighbor.[0]   
        secondIndex <- neighbor.[1]   
        thirdIndex <-  neighbor.[2] 
        let cubeLength = Math.Cbrt(numOfNodes |> float) |> int
        //printfn "index1: %d index2: %d index3: %d" randomDirection secondIndex thirdIndex
        if (firstIndex < cubeLength && firstIndex >= 0 && secondIndex < cubeLength && secondIndex >= 0 && thirdIndex < cubeLength && thirdIndex >= 0)
        then 
            properIndexNotFound <- false 
        else 
            properIndexNotFound <- true // i know this is pointless  

    let gridOfActors : _ list = cubeOfActors.[firstIndex] // first index as index into cube
    let rowOfActors : _ list = gridOfActors.[secondIndex]
    let actor = rowOfActors.[thirdIndex]
    actor
    //cubeOfActors.[neighbor.[0]][neighbor.[1]][neighbor.[2]] // return actor neighbor

let findLineNeighbor (index: int) = 
    
    // which axis to find neighbor on
    let random = Random()
    let mutable neighbor = index 
    let randomDirection = random.Next(1)

    let mutable properIndexNotFound = true
    while properIndexNotFound do // loop until we are not index out of bounds
        // which direction 
        if randomDirection = 1
        then neighbor <- neighbor + 1
        else neighbor <- neighbor - 1 

        if (neighbor < numOfNodes && neighbor >= 0)
        then 
            properIndexNotFound <- false 
        else 
            properIndexNotFound <- true
            
    let actor = listOfActors.[neighbor]
    actor
    //cubeOfActors.[neighbor.[0]][neighbor.[1]][neighbor.[2]] // return actor neighbor

let gossipActor (name: string) (topologyPosition:int list) = spawn system name <| fun mailbox ->

    let rec loop (count,(position: int list)) = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            let mutable counter = count
            let mutable position = position
            let random = Random()

            match msg with
            |Rumor (rumor) ->
                let randomNum = random.Next(numOfNodes)
                
                if count < 10 then
                    match topology with
                    | "line" -> 
                        let neighborActor = findLineNeighbor(position.[0])
                        (*system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds 0., TimeSpan.FromMilliseconds(50.), fun () -> 
                            findLineNeighbor(position.[0]) <! rumor
                        )*)
                        neighborActor <! Rumor(rumor) 
                    | "3D" -> 
                        let neighborActor = find3dNeighbor(position)                          
                        neighborActor <!  Rumor(rumor) 
                    | "imp3D" -> 
                        let neighborActor = find3dNeighbor(position) 
                        neighborActor <!  Rumor(rumor) 
                    | _ -> 
                        let neighborActor = listOfActors.[randomNum]
                        neighborActor <!  Rumor(rumor)
            |_ -> printf ""

            counter <- counter + 1

            if counter > 10 then
                printfn "actor: %s terminating" name

                let cpuTimeDiff = (proc.TotalProcessorTime-cpuTime).TotalMilliseconds
                sw.Stop()
                printfn "CPU time = %dms" (int64 cpuTimeDiff)
                printfn "REAL time = %fms" sw.Elapsed.TotalMilliseconds
                mailbox.Context.System.Terminate() |> ignore

            return! loop(counter,position)
        }
    let initialS = name |> float
    loop(0,topologyPosition)


let pushSum (name:string) (topologyPosition:int list) = spawn system name <| fun mailbox ->
        //let mutable keepMessaging = true
        let rec loop(s,w, count,(position: int list)) = actor {
            let! msg = mailbox.Receive() 
            let sender = mailbox.Sender() 
            let mutable localS = s
            let mutable localW = w
            let mutable counter = count
            let mutable position = position
            let random = Random()
            //printfn " pos: %A" position 
            match msg with
            | SumWeight (recievedS,recievedW) -> 
                //printfn "local s: %f" localS
                localS <- localS + recievedS
                localW <- localW + recievedW
                
                localS <- localS/2.0; // keep half
                localW <- localW/2.0;
                //printfn "len: %d" listOfActors.Length
                //printfn "local s again: %f " localS
                let randomNum = random.Next(numOfNodes) // randomly choose actor to send to

                if count < 3
                then
                    match topology with
                        | "line" -> 
                            let neighborActor = findLineNeighbor(position.[0])
                            //printfn "calling actor: %d" randomNum
                            system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds 0., TimeSpan.FromMilliseconds(50.), fun () -> 
                                findLineNeighbor(position.[0]) <! (SumWeight(localS,localW))
                            )
                            neighborActor <! (SumWeight(localS,localW)) // send half of s and w to next actor 
                        | "3D" -> 
                            let neighborActor = find3dNeighbor(position)
                            //printfn "calling actor @ %A" position 
                            (*system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds 0., TimeSpan.FromMilliseconds 100., fun () -> 
                                find3dNeighbor(position) <! (SumWeight(localS,localW))
                            )*)                           
                            neighborActor <! (SumWeight(localS,localW)) // send half of s and w to next actor 
                        | "imp3D" -> 
                            let neighborActor = find3dNeighbor(position) 
                            //system.Scheduler.ScheduleTellRepeatedly (TimeSpan.Zero, TimeSpan.FromMilliseconds(5.), neighborActor, (SumWeight(localS,localW)))
                            neighborActor <! (SumWeight(localS,localW)) // send half of s and w to next actor 
                        | _ -> 
                            let neighborActor = listOfActors.[randomNum]
                            (*system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds 0., TimeSpan.FromMilliseconds 100., fun () -> 
                                listOfActors.[randomNum] <! (SumWeight(localS,localW))
                            )*)     
                            neighborActor <! (SumWeight(localS,localW)) // send half of s and w to next actor     
            | _ -> printfn "this shouldn't happen"    
            
            let oldEstimate = s/w
            let newEstimate = localS/localW
            let diff = Math.Abs(newEstimate - oldEstimate)
            let minimumDiff = Math.Pow(10.0,-10.0)
            //printfn "actor: %s estimates: " name
            //printfn "old : %d" oldEstimate
            //printfn "new : %d" newEstimate
            if ( (diff) < minimumDiff )
            then counter <- counter + 1
            else
                //printfn "diff: %.10f" diff
                counter <- 0

            if counter > 2
            then 
                printfn "actor: %s terminating" name
                //printfn "diff: %.10f" diff
                //keepMessaging <- false
                let cpuTimeDiff = (proc.TotalProcessorTime-cpuTime).TotalMilliseconds
                sw.Stop()
                printfn "CPU time = %dms" (int64 cpuTimeDiff)
                printfn "REAL time = %fms" sw.Elapsed.TotalMilliseconds
                mailbox.Context.System.Terminate() |> ignore
           
            // handle an incoming message
            return! loop(localS,localW,counter,position) // store the new s,w into the next state of the actor
        }
        let initialS = name |> float
        //printfn "actor: %s is at initial position: %A" name topologyPosition
        loop(initialS,1.0,0,topologyPosition) // all actors start out with an s and w value that is maintained 

let addNodesInArray nodes = 
    for i in 0..nodes do 
        let name = i |> string
        if alg = "gossip"
        then 
            let actor = [gossipActor name [i]]
            listOfActors <- List.append listOfActors actor  // append 
        else
            let actor = [pushSum name [i]]
            listOfActors <- List.append listOfActors actor  // append 

let addNodesInCube nodes = 
    let cubeLength = Math.Cbrt(nodes |> float) |> int
    printfn "cube length %d" cubeLength
    let mutable nodeCount = 0
    for grid in 0..cubeLength-1 do 
        let mutable gridOfActors = [] // make a new grid
        let gridNum = grid |> string 
        for row in 0..cubeLength-1 do 
            let mutable rowOfActors = []
            let rowNum = row |> string
            for cell in 0..cubeLength-1 do
                let cellNum = cell |> string
                //let actorName = gridNum + rowNum + cellNum
                nodeCount <- nodeCount + 1
                let actorName = nodeCount |> string
                //printfn "position: %s" actorName
                if alg = "gossip"
                then 
                    let actor = [gossipActor actorName [grid;row;cell]]
                    rowOfActors <- List.append rowOfActors actor  // append gossip actor                     
                else
                    let actor = [pushSum actorName [grid;row;cell]]
                    rowOfActors <- List.append rowOfActors actor  // append pushsum actor 
                
            let rowOfActors2 = [rowOfActors]
            gridOfActors <- List.append gridOfActors rowOfActors2 // append row of actors 
        
        let gridOfActors2 = [gridOfActors] 
        cubeOfActors <- List.append cubeOfActors gridOfActors2 // append grid for each layer of depth
    
(*
let boss2 (nodes: int) (mailbox: Actor<_>) rumor =
    
    let refArr =
        [|
        for i in 0 .. nodes-1 -> 
            (spawn mailbox ("actor"+i.ToString()) (gossipActor (neighbors.Item(i))))
        |]

    let mutable actorRef : IActorRef list = []
    
    let rnd = Random().Next(0, neighbors.Count)
    
    for i in 0 .. refArr.Length do
        actorRef <- actorRef @ [refArr.[i]]
    
    actorRef.[rnd] <! rumor

    let mutable rumorCount = 0
    let mutable actorIs = mailbox.Context.Parent


    let rec loop () = 
        actor {
            let! message = mailbox.Receive()
            let sender = mailbox.Sender()
            if actorIs = mailbox.Context.Parent then
                actorIs <- sender
                return! loop()
            else
                rumorCount <- rumorCount + 1
                if rumorCount < 20 then
                    return! loop()
                else
                    Console.WriteLine ("All nodes have received the rumor!")
                    actorIs <! "Finished"
        }
    loop ()
*)

let boss = 
    spawn system "boss" 
        (actorOf2 (fun mailbox msg ->
            match msg with  
            | StartSum (nodes, topology) -> 
                let random = Random()
                let randomNum = random.Next(nodes) // randomly choose actor to start with
                match topology with
                    | "line" -> 
                        addNodesInArray(numOfNodes) 
                        listOfActors.[randomNum] <! SumWeight(0.0,0.0) // ill not add anything to s,w since its first iteration 
                    | "3D" -> 
                        printfn "3D topology"
                        addNodesInCube(numOfNodes)
                        let gridOfActors : _ list = cubeOfActors.[0] // first index as index into cube
                        let listOfActors : _ list = gridOfActors.[0]
                        let actor = listOfActors.[0]                        
                        actor <! SumWeight(0.0,0.0)
                    | "imp3D" -> 
                        addNodesInCube(numOfNodes) 
                        cubeOfActors.[randomNum].[0].[0] <! SumWeight(0.0,0.0)
                    | _ -> 
                        addNodesInArray(numOfNodes)  // append  
                        listOfActors.[randomNum] <! SumWeight(0.0,0.0) // s = i, w = 1
            | StartGossip (nodes, topology) -> 
                let random = Random()
                let randomNum = random.Next(nodes) // randomly choose actor to start with
                match topology with
                    | "line" -> 
                        addNodesInArray(numOfNodes) 
                        listOfActors.[randomNum] <! Rumor("rumor") // ill not add anything to s,w since its first iteration 
                    | "3D" -> 
                        printfn "3D topology"
                        addNodesInCube(numOfNodes)
                        let gridOfActors : _ list = cubeOfActors.[0] // first index as index into cube
                        let listOfActors : _ list = gridOfActors.[0]
                        let actor = listOfActors.[0]                        
                        actor <! Rumor("rumor")
                    | "imp3D" -> 
                        addNodesInCube(numOfNodes) 
                        cubeOfActors.[randomNum].[0].[0] <! Rumor("rumor")
                    | _ -> 
                        addNodesInArray(numOfNodes)  // append  
                        listOfActors.[randomNum] <! Rumor("rumor")
            | Stop -> mailbox.Context.System.Terminate() |> ignore
            | _ -> printfn "here"   
            
        ))
//printfn "%s" inputParams.[0] // should be number of nodes

if String.Equals(alg,"gossip",StringComparison.CurrentCultureIgnoreCase)
then 
    printfn "starting gossip"
    boss <! StartGossip (inputParams.[0] |> int, inputParams.[1]) // do gossip 
else boss <! StartSum (inputParams.[0] |> int, inputParams.[1]) // otherwise do sum

let input2 = System.Console.ReadLine() |> ignore
