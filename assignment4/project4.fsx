open System
open System.Security.Cryptography
open System.Text;
open System.Diagnostics
#r "nuget: Akka.FSharp" 
open Akka.Configuration
open Akka.FSharp
open Akka.Actor

let system = ActorSystem.Create("FSharp")

type Message =
    | Tweeting of string // trigger client to tweet -> will be triggered by simulator
    | Tweet of string * string // send tweet to server 
    | NewsFeed
    | MyTweets of string
    | AddTweet of string
    | Subscribe of string
    | Unsubscribe of string
    | Success 

let mutable clients = [] 


let showTweets (tweets:string list) = 
    printfn "tweets %A" tweets |> ignore // placeholder


let findSubscribers (userId:string) = 
    let subscriber = clients.[0] // placeholder 
    let subscriber2 = clients.[1] // placeholder
    let subscribers = [subscriber;subscriber2]
    subscribers

let publishTweet (tweetMsg,id) = 
    let subscribers = findSubscribers(id)
    for sub in subscribers do 
        let tweet = "user: " + id + " : " + tweetMsg
        sub <! AddTweet(tweet) //


let server = spawn system (string id) <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweet(tweets, id) ->
                publishTweet(tweets,id)
                sender <! Success
        return! loop() 
    }
    loop()  


let client (id: string) = spawn system (string id) <| fun mailbox ->
    let myTweets = [] 
    let mutable newsFeed: string list = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | NewsFeed ->
                showTweets(newsFeed)
            | Tweeting(tweet) -> 
                server <! Tweet(tweet,id)
            | AddTweet(tweet) ->
                newsFeed <- List.append newsFeed [tweet] 

        return! loop() 
    }
    loop()  

// will simulate users interacting with Twitter by sending messages to certain clients
let simulator() = 
    clients.[0] <! Tweeting("yo")

let initClients numClients = 
    for i in 0..numClients do 
        let name = i |> string
        let clientActor = [client name]
        clients <- List.append clients clientActor  // append 


[<EntryPoint>]
let main argv = 
    initClients(2)
    simulator()
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
