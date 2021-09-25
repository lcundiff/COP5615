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

let system = System.create "my-system" <| ConfigurationFactory.Default()

type Message =
    | Stop
    | Hash
    | Start of string
    | Coin of string
    | Mine of string
    | GetCoin of string * string

let getLeadingZerosString (numLeadingZeros:int) : string= 
    let mutable leadingZeros = ""
    let mutable count = 0
    //printfn "%d" numLeadingZeros
    let zerosChars = [| for i in 1..numLeadingZeros -> '0' |]
    let zerosString = String(zerosChars)

    //printfn "zeros: %s" zerosString
    zerosString


let getHashbrown (lilHash:string) =
    //for arg in fsi.CommandLineArgs |> Seq.skip 1 do
    //printf "%s " lilHash 
    use sha256Hash = SHA256Managed.Create()
    sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(lilHash))

let convertString (hashbrown: byte []) : string = 
    let sb = StringBuilder(hashbrown.Length * 2)  
    hashbrown |> Array.map (fun c -> sb.AppendFormat("{0:X2}",c)) |> ignore
    sb.ToString() 
    

let checkCoin (stringHash:string) (leadingZeros:string) = 
    //let zeroCount = leadingZeros.Length
    //printfn "%s" leadingZeros
    match stringHash.IndexOf(leadingZeros,0,leadingZeros.Length) with
    | 0 -> true // ex. "00000" is found at position 0 
    | -1 -> false // ex. "000" is not found
    | _ -> false // ex. "000" is found but not at position 0 (this shouldnt happen)

let generateRandomString (ufid:string) = 
    let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
    let charsLen = chars.Length
    let random = Random()
    // build a string of size 50
    let randomChars = [| for i in 0..50 -> chars.[random.Next(charsLen)] |]
    let newString = ufid + String(randomChars)
    //printf "%s " newString 
    newString


let rec getCoin leadingZerosCount preHash : string = 
    //printfn "hello2"
    let leadingZerosCountInt = 
        leadingZerosCount |> int
    let leadingZeros = getLeadingZerosString(leadingZerosCountInt)
    //for i in 0 .. 100 do 
    let hashString = preHash |> getHashbrown |> convertString 

    //printfn "%s" hashString
    let isCoin = 
        checkCoin hashString leadingZeros
    //printfn "%b" isCoin
    if (isCoin)
    then hashString
    else getCoin leadingZerosCount (generateRandomString "62919511")
    
let kidMiner = spawn system "kidMiner" <| fun mailbox ->
        let rec loop() = actor {
            let! msg = mailbox.Receive()
            let sender = mailbox.Sender() 

            printfn "gonna mine until we get this many leading 0s: %s" msg 
            let preHash = generateRandomString "62919511"

            let coin = 
                getCoin msg preHash
            if (coin.Length > 0)
            then 
                sender <! Coin coin
            else 
                printfn "no coin"
                sender <! Mine msg
            // handle an incoming message
            return! loop()
        }
        loop()


let hashSlingingSlasher = 
    spawn system "hashSlingingSlasher" <| fun mailbox ->
            let rec loop() = actor {
                let! msg = mailbox.Receive()
                let sender = mailbox.Sender() 

                printfn "gonna make a string: %s" msg

                let preHash = generateRandomString "62919511"
                sender <! Mine preHash
                return! loop()
            }
            loop()

let boss = 
    spawn system "boss" 
        (actorOf2 (fun mailbox msg ->
            
            let eventStream = mailbox.Context.System.EventStream
            //printfn "msg: %s" msg
            let preHash = generateRandomString "62919511"
            let leadingZeros = ref "1"
            //let preHash = ref "62919511"

            match msg with
            | Start input -> leadingZeros := input
            | Mine randomString -> kidMiner <! randomString
            | Hash -> hashSlingingSlasher <! "tst"
            | Coin coinString -> printfn "coin: %s" coinString
            //| Stop -> stop typeof<Message> mailbox.Self eventStream |> ignore
            | _ -> printfn "here"
         
            //kidMiner <! preHash
        ))
        


let input = System.Console.ReadLine() 
let sw = Stopwatch.StartNew()
boss <! Start input
boss <! Mine input

sw.Stop()   
printfn "%f" sw.Elapsed.TotalMilliseconds


let handleMessage (mailbox: Actor<'a>) msg =
    match msg with
    | Some x -> printf "%A" x
    | None -> ()

// actorOf2 (fn : Actor<'Message> -> 'Message -> unit) (mailbox : Actor<'Message>) : Cont<'Message, 'Returned>
// - uses a function, which takes both the message and an Actor instance as the parameters. Mailbox parameter is injected by spawning functions.

let aref = spawn system "my-actor" (actorOf2 handleMessage)
let blackHole = spawn system "black-hole" (actorOf (fun msg -> ()))