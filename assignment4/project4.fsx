open System
open System.Security.Cryptography
open System.Text;
open System.Diagnostics
#r "nuget: Akka.FSharp" 
open Akka.Configuration
open Akka.FSharp
open Akka.Actor
open System.Collections.Generic

let system = ActorSystem.Create("FSharp")

type Message =
    | Tweeting of string // trigger client to tweet -> will be triggered by simulator
    | Tweet of string * string // send tweet to server 
    | NewsFeed
    | MyTweets of string
    | AddTweet of string
    | Subscribing of string // cause client/user to subscribe to passed in userid 
    | Subscribe of string * string // user ids of who subscribed to who
    | AddFollower of string
    | Unsubscribe of string
    | Success 

//let mutable clients = [] 
let users = new Dictionary<string, IActorRef>()

let showTweets (tweets:string list) = 
    printfn "tweets %A" tweets |> ignore // placeholder


let findSubscribers (userId:string) = 
    let subscriber = users.["0"] // placeholder 
    let subscriber2 = users.["1"] // placeholder
    let subscribers = [subscriber;subscriber2]
    subscribers

let publishTweet (tweetMsg,id) = 
    let subscribers = findSubscribers(id)
    for sub in subscribers do 
        let tweet = "user: " + id + " : " + tweetMsg
        sub <! AddTweet(tweet) //

let addFollower(subscriberId, subscribedToId) = 
    users.[subscribedToId:string] <! AddFollower(subscriberId)

let server = spawn system (string id) <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweet(tweets, id) ->
                publishTweet(tweets,id)
                sender <! Success // let client know we succeeded (idk if this is neccessary, but just adding it for now)
            | Subscribe(subscriber, subscribedTo) -> 
                addFollower(subscriber, subscribedTo)
        return! loop() 
    }
    loop()  


let client (id: string) = spawn system (string id) <| fun mailbox ->
    let mutable myTweets: string list  = [] 
    let mutable mySubs: string list  = []
    let mutable myFollowers: string list  = []
    let mutable newsFeed: string list = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweeting(tweet) -> // user tweeted (triggered by simulator)
                myTweets <- List.append myTweets [tweet] // store users own tweets for querying
                server <! Tweet(tweet,id) // tell the server we tweeted
            | Subscribing(subscribedTo) -> // user clicked subscribe on a user (triggered by simulator)
                mySubs <- List.append mySubs [subscribedTo] 
                server <! Subscribe(id,subscribedTo) 
            | AddTweet(tweet) -> // server telling us someone we subscribed to tweeted
                newsFeed <- List.append newsFeed [tweet] 
            | AddFollower(subscriber) ->
                myFollowers <- List.append myFollowers [subscriber] 
            | NewsFeed -> // Views tweets of users they subscribed to (simulator)
                showTweets(newsFeed) // idk if this will be needed, just adding it here for now
            | Success -> 
                printfn "server message succeeded!"

        return! loop() 
    }
    loop()  

// will simulate users interacting with Twitter by sending messages to certain clients
let simulator() = 
    users.["0"] <! Tweeting("yo")

let initClients numClients = 
    for i in 0..numClients do 
        let name = i |> string
        let clientActor = client name
        users.Add(name,clientActor)
        //clients <- List.append clients clientActor  // append 


[<EntryPoint>]
let main argv = 
    initClients(2)
    simulator()
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
