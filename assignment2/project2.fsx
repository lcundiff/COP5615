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
    | FirstMessage of int
    | Tuple of float * float
    | Estimate

printfn "input ready"
let inputLine = Console.ReadLine() 
let splitLine = (fun (line : string) -> Seq.toList (line.Split ' '))
let inputParams = splitLine inputLine

let proc = Process.GetCurrentProcess()
let cpu_time_stamp = proc.TotalProcessorTime
let sw = Stopwatch.StartNew()





let mutable listOfActors = []//[0..inputParams.[0] |> int] // list of actors as long as the nodes inputted
//let mutable gridOfActors = [listOfActors]
let mutable cubeOfActors = []
let allActors = Map.empty
let mutable numOfNodes = 0; 
let pushSum (name:string) = spawn system name <| fun mailbox ->
        //let mutable keepMessaging = true
        let rec loop(s,w, count) = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            let mutable localS = s
            let mutable localW = w
            let mutable counter = count


            match msg with
            | FirstMessage numOfNodes -> 
                let random = Random()
                let randomNum = random.Next(numOfNodes) // randomly choose actor to send to
                localW <- localW/2.0 // keep half and send half
                localS <- localS/2.0
                if count < 3
                then 
                    listOfActors.[randomNum] <! (Tuple(localS,localW)) // send half of s and w to next actor            
            | Tuple (recievedS,recievedW) -> 
                //if 
                //printfn "local s: %f" localS
                //printfn "recieved s: %f" recievedS
                localW <- localW + recievedW
                localS <- localS + recievedS
                
                localS <- localS/2.0; // keep half
                localW <- localW/2.0;
                let random = Random()
                //printfn "len: %d" listOfActors.Length
                //printfn "local s again: %f " localS
                let randomNum = random.Next(numOfNodes) // randomly choose actor to send to
                if count < 3
                then 
                    //printfn "calling actor: %d" randomNum
                    listOfActors.[randomNum] <! (Tuple(localS,localW)) // send half of s and w to next actor
            | Estimate ->   
                let estimate =  s/w 
                printfn "estimate: %f" estimate
            | _ -> printfn "wut" 
            
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
                printfn "diff: %.10f" diff
                counter <- 0

            if counter > 2
            then 
                printfn "actor: %s terminating" name
                //keepMessaging <- false
                mailbox.Context.System.Terminate() |> ignore

            //then sender <! Stop
           
            // handle an incoming message
            return! loop(localS,localW,counter) // store the new s,w into the next state of the actor
        }
        let initialS = name |> float
        //printfn "initial s: %d" initialS
        loop(initialS,1.0,0) // all actors start out with an s and w value that is maintained 


let mutable summing = true 

let addNodesInArray nodes = 
    for i in 1..nodes do 
        let name = i |> string
        let actor = [pushSum(name)]
        listOfActors <- List.append listOfActors actor  // append 

let find3dNeighbor (x:int) (y:int) (z:int) = 
    (*let neighbor1 = [x-1,y,z]
    let neighbor2 = [x+1,y,z]
    let neighbor3 = [x,y-1,z]
    let neighbor4 = [x,y+1,z]
    let neighbor5 = [x,y,z-1]
    let neighbor6 = [x,y,z+1]*)
    let mutable index = [1;2;1]
    
    // which axis to find neighbor on
    let random = Random()
    let randomAxisIndex = random.Next(3)
    let mutable axis = index.[randomAxisIndex] 
    let randomDirection = random.Next(1)

    // which direction 
    if randomDirection = 1
    then axis <- axis + 1
    else axis <- axis - 1 

    //let pos, value = int (index.[0]), int (index.[1])
    let neighbor = index |> List.mapi (fun i v -> if i = randomAxisIndex then axis else v) 
    let firstIndex = neighbor.[0]   
    let secondIndex = neighbor.[1]   
    let thirdIndex = neighbor.[2]   

    let gridOfActors = cubeOfActors.[firstIndex] // first index as index into cube
    let listOfActors = gridOfActors.[secondIndex]
    let actor = listOfActors.[thirdIndex]
    actor
    //cubeOfActors.[neighbor.[0]][neighbor.[1]][neighbor.[2]] // return actor neighbor



let addNodesInCube nodes = 
    let cubeLength = Math.Cbrt(nodes |> float) |> int
    for grid in 1..cubeLength do 
        let mutable gridOfActors = [] // make a new grid
        for row in 1..cubeLength do 
            let mutable rowOfActors = []
            for cell in 1..cubeLength do
                let name = row |> string
                let actor = [pushSum(name)]
                rowOfActors <- List.append rowOfActors actor  // append actor 
            let rowOfActors2 = [rowOfActors]
            gridOfActors <- List.append gridOfActors rowOfActors2 // append row of actors 
        
        let gridOfActors2 = [gridOfActors] 
        cubeOfActors <- List.append cubeOfActors gridOfActors2 // append grid for each layer of depth
    

let boss = 
    spawn system "boss" 
        (actorOf2 (fun mailbox msg ->
            
            match msg with
            | StartSum (nodes, topology) -> 
                numOfNodes <- nodes 
                match topology with
                    | "line" -> addNodesInArray(numOfNodes)  
                    | "3D Grid" -> addNodesInCube(numOfNodes)
                    | "3D Grid Random" -> addNodesInCube(numOfNodes) 
                    | _ -> addNodesInArray(numOfNodes)  // append     
                
                    
                    //List.map(spawn system (i |> string)) |> ignore
                    
                //allActors |> List.iter (fun item -> 
                //    item <! Tuple(0,0,0))
                //printfn "name: %A" allActors
                let random = Random()
                let randomNum = random.Next(nodes) // randomly choose actor to start with
                listOfActors.[randomNum] <! FirstMessage(nodes) // s = i, w = 1
            | Stop -> mailbox.Context.System.Terminate() |> ignore
            | _ -> printfn "here"            
        ))
//printfn "%s" inputParams.[0] // should be number of nodes
let alg = inputParams.[2]
if String.Equals(alg,"gossip",StringComparison.CurrentCultureIgnoreCase)
then boss <! StartSum (inputParams.[0] |> int, inputParams.[1]) // do gossip 
else boss <! StartSum (inputParams.[0] |> int, inputParams.[1]) // otherwise do sum


let input2 = System.Console.ReadLine() |> ignore
