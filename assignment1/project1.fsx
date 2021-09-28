open System
//open System.IO
open System.Text;
open System.Security.Cryptography
open System.Diagnostics
#r "nuget: Akka.FSharp" 
#r "nuget: Akka.TestKit" 
open Akka.Configuration
open Akka.FSharp
open Akka.Actor

let system = System.create "my-system" <| ConfigurationFactory.Default()


type Message =
    | Stop
    | HashBrown
    | Coin of string * string
    | Start of string
    | ListOfStrings of string * string
    | Mine of string * string * string * string * string * string

printfn "input ready"
let input = Console.ReadLine() 
let proc = Process.GetCurrentProcess()
let cpu_time_stamp = proc.TotalProcessorTime
let sw = Stopwatch.StartNew()


let getLeadingZerosString (numLeadingZeros:int) : string= 
    let mutable leadingZeros = ""
    let mutable count = 0
    //printfn "%d" numLeadingZeros
    let zerosChars = [| for i in 1..numLeadingZeros -> '0' |]
    let zerosString = String(zerosChars)

    //printfn "zeros: %s" zerosString
    zerosString


let getHashBrown (lilHashBrown:string) =
    //for arg in fsi.CommandLineArgs |> Seq.skip 1 do
    //printf "%s " lilHashBrown 
    use sha256Hash = SHA256Managed.Create()
    sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(lilHashBrown))

let convertString (hashbrown: byte []) : string = 
    let sb = StringBuilder(hashbrown.Length * 2)  
    hashbrown |> Array.map (fun c -> sb.AppendFormat("{0:X2}",c)) |> ignore
    sb.ToString() 
    

let checkCoin (stringHashBrown:string) (leadingZeros:string) = 
    //let zeroCount = leadingZeros.Length
    //printfn "%s" leadingZeros
    match stringHashBrown.IndexOf(leadingZeros,0,leadingZeros.Length) with
    | 0 -> true // ex. "00000" is found at position 0 
    | -1 -> false // ex. "000" is not found
    | _ -> false // ex. "000" is found but not at position 0 (this shouldnt happen)

let generateRandomString (ufid:string) = 
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
    let charsLen = chars.Length
    let random = Random()
    // build a string of size 50
    let randomChars = [| for i in 0..20 -> chars.[random.Next(charsLen)] |]
    let newString = ufid + String(randomChars)
    //printf "%s " newString 
    newString


let rec getCoin (leadingZerosCount:string) (preHashBrown : string) = 
    //printfn "hello2"
    let leadingZerosCountInt = 
        leadingZerosCount |> int 
    let leadingZeros = getLeadingZerosString(leadingZerosCountInt)
    let mutable coin = ""

    let hashString = preHashBrown |> getHashBrown |> convertString
    let isCoin = 
        checkCoin hashString leadingZeros
    if(isCoin)
    then  
        hashString
    else "" //getCoin leadingZerosCount preHashBrown      
       
    //printfn "%s" hashString

    //printfn "%b" isCoin


    
let kidMiner = spawn system "kidMiner" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 

            //let preHashBrown = generateRandomString "62919511"

            match msg with
            | ListOfStrings (zeros,randomString) -> 
                //printfn "mine until we get this many leading 0s: %s" zeros
                let splitLine = (fun (line : string) -> Seq.toList (line.Split ','))
                let randomStrings = splitLine randomString

                for str in randomStrings do
                    //printfn "random string: %s" str 
                    //printfn "%A" randomString
                    let coin = getCoin zeros str
                    if (coin.Length > 0)
                    then 
                        //printfn "here"
                        sender <! Coin(coin, str)
                    //else 
                    //    printfn "no coin"
                    //    sender <! Mine msg
            | _ -> printfn "wut" 

            // handle an incoming message
            return! loop()
        }
        loop()

let kidMiner2 = spawn system "kidMiner2" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            match msg with
            | ListOfStrings (zeros,randomString) -> 
                let splitLine = (fun (line : string) -> Seq.toList (line.Split ','))
                let randomStrings = splitLine randomString

                for str in randomStrings do
                    let coin = getCoin zeros str
                    if (coin.Length > 0)
                    then 
                        sender <! Coin(coin, str)
            | _ -> printfn "wut" 

            // handle an incoming message
            return! loop()
        }
        loop()

let kidMiner3 = spawn system "kidMiner3" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            match msg with
            | ListOfStrings (zeros,randomString) -> 
                let splitLine = (fun (line : string) -> Seq.toList (line.Split ','))
                let randomStrings = splitLine randomString
                for str in randomStrings do
                    //printfn "random string: %s" str 
                    let coin = getCoin zeros str
                    if (coin.Length > 0)
                    then sender <! Coin(coin, str)
            | _ -> printfn "wut" 
            return! loop()
        }
        loop()
let kidMiner4 = spawn system "kidMiner4" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            match msg with
            | ListOfStrings (zeros,randomString) -> 
                let splitLine = (fun (line : string) -> Seq.toList (line.Split ','))
                let randomStrings = splitLine randomString
                for str in randomStrings do
                    //printfn "random string: %s" str 
                    let coin = getCoin zeros str
                    if (coin.Length > 0)
                    then sender <! Coin(coin, str)
            | _ -> printfn "wut" 
            return! loop()
        }
        loop()
let kidMiner5 = spawn system "kidMiner5" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 
            match msg with
            | ListOfStrings (zeros,randomString) -> 
                let splitLine = (fun (line : string) -> Seq.toList (line.Split ','))
                let randomStrings = splitLine randomString
                for str in randomStrings do
                    //printfn "random string: %s" str 
                    let coin = getCoin zeros str
                    if (coin.Length > 0)
                    then sender <! Coin(coin, str)
            | _ -> printfn "wut" 
            return! loop()
        }
        loop()

let hashSlingingSlasher = 
    spawn system "hashSlingingSlasher" <| fun mailbox ->
            let rec loop() = actor {
                let! msg = mailbox.Receive()
                let sender = mailbox.Sender() 
                let mutable listOfStrings = generateRandomString "62919511;"
                let mutable listOfStrings2 = generateRandomString "62919511;"
                let mutable listOfStrings3 = generateRandomString "62919511;"
                let mutable listOfStrings4 = generateRandomString "62919511;"
                let mutable listOfStrings5 = generateRandomString "62919511;"

                //printfn "hashing. leading 0s: %s" msg
                for i in 0 .. 25 do
                    let randomString = generateRandomString "62919511;"
                    listOfStrings <- listOfStrings + "," + randomString
                    let randomString2 = generateRandomString "62919511;"
                    listOfStrings2 <- listOfStrings2 + "," + randomString2
                    let randomString3 = generateRandomString "62919511;"
                    listOfStrings3 <- listOfStrings3 + "," + randomString3
                    let randomString4 = generateRandomString "62919511;"
                    listOfStrings4 <- listOfStrings4 + "," + randomString4
                    let randomString5 = generateRandomString "62919511;"
                    listOfStrings5 <- listOfStrings5 + "," + randomString5

                sender <! Mine (msg, listOfStrings,listOfStrings2,listOfStrings3,listOfStrings4,listOfStrings5)
                sender <! HashBrown

                return! loop()
            }
            loop()

let mutable mining = true 
let mutable leadingZeros = "1"

let boss = 
    spawn system "boss" 
        (actorOf2 (fun mailbox msg ->
            
            let eventStream = mailbox.Context.System.EventStream
            //printfn "mining: %b" mining 
                        
            match msg with
            | Start input -> leadingZeros <- input //hashSlingingSlasher <! input //leadingZeros := input
            | Mine (zeros, randomStrings1, randomStrings2, randomStrings3,randomStrings4,randomStrings5) -> 
                //printfn "mining queue"
                //printfn "mining: %b" mining
                if(mining)
                then 
                    kidMiner <! ListOfStrings(zeros,randomStrings1)
                    kidMiner2 <! ListOfStrings(zeros,randomStrings2)
                    kidMiner3 <! ListOfStrings(zeros,randomStrings3)
                    kidMiner4 <! ListOfStrings(zeros,randomStrings4)
                    kidMiner5 <! ListOfStrings(zeros,randomStrings5)
                else printfn ""
            | HashBrown -> 
                //printfn "hasbrown queue" // %b" mining
                if(mining) // \!mining is dereference
                then 
                    hashSlingingSlasher <! leadingZeros
                else printfn ""
            | Coin(coinString,hash) -> 
                //mining := false
                //mining.Value <- false
                //printfn "mining: %b" mining.Value
                mining <- false //
                let cpuTime = (proc.TotalProcessorTime-cpu_time_stamp).TotalMilliseconds
                sw.Stop()
                printfn "%s   %s" hash coinString
                //printfn "CPU time = %dms" (int64 cpuTime)
                //printfn "REAL time = %fms" sw.Elapsed.TotalMilliseconds
                mailbox.Context.System.Terminate() |> ignore
                
                //printfn "mining: %b" mining
            | Stop -> mining <- false //mining := false //
            //| Stop -> stop typeof<Message> mailbox.Self eventStream |> ignore
            | _ -> printfn "here"

            //kidMiner <! preHashBrown
        ))
printfn "%s" input        
boss <! Start input
boss <! HashBrown



let input2 = System.Console.ReadLine() |> ignore

// actorOf2 (fn : Actor<'Message> -> 'Message -> unit) (mailbox : Actor<'Message>) : Cont<'Message, 'Returned>
// - uses a function, which takes both the message and an Actor instance as the parameters. Mailbox parameter is injected by spawning functions.

//let aref = spawn system "my-actor" (actorOf2 handleMessage)
//let blackHole = spawn system "black-hole" (actorOf (fun msg -> ()))