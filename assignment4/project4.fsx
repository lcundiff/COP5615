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
    | ReTweet of string * string * string // send tweet to server 
    | ReTweeting of string * string // send tweet to server 
    | HashTagTweets of string // sends hashtag to server to query for tweets
    | ReceiveTweets of string list
    | AddTweet of string
    | Subscribing of string // cause client/user to subscribe to passed in userid 
    | Subscribe of string * string // user ids of who subscribed to who
    | AddFollower of string
    | RemoveFollower of string
    | Unsubscribing of string
    | Unsubscribe of string * string
    | SubscribedToTweets
    | MyTweets
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

let removeFollower(subscriberId, subscribedToId) = 
    users.[subscribedToId:string] <! RemoveFollower(subscriberId)

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
            | Unsubscribe(subscriber, subscribedTo) -> 
                removeFollower(subscriber, subscribedTo)
        return! loop() 
    }
    loop()  

let removeFromList(sub, subs) =
    subs 
    // Associate each element with a boolean flag specifying whether 
    // we want to keep the element in the resulting list
    |> List.mapi (fun i el -> (el <> sub, el)) 
    // Remove elements for which the flag is 'false' and drop the flags
    |> List.filter fst |> List.map snd

let client (id: string) = spawn system (string id) <| fun mailbox ->
    let mutable myTweets: string list  = [] 
    let mutable mySubs: string list  = []
    let mutable myFollowers: string list  = []
    let mutable SubscribedToTweets: string list = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweeting(tweet) -> // user tweeted (triggered by simulator)
                myTweets <- List.append myTweets [tweet] // store users own tweets for querying
                server <! Tweet(tweet,id) // tell the server to send tweet to subscribers
            | ReTweeting(tweet,tweeter) ->
                 server <! ReTweet(tweet,id,tweeter) // do same as normal tweet but send the original author (tweeter)
            | Subscribing(subscribedTo) -> // user clicked subscribe on a user (triggered by simulator)
                mySubs <- List.append mySubs [subscribedTo] 
                server <! Subscribe(id,subscribedTo) 
            | Unsubscribing(subscribedTo) -> // simulator trigger unsubscribe
                removeFromList(subscribedTo, mySubs) |> ignore
                server <! Unsubscribe(id,subscribedTo)
            | AddTweet(tweet) -> // server telling us someone we subscribed to tweeted
                printfn("adding tweet: %s") tweet
                SubscribedToTweets <- List.append SubscribedToTweets [tweet] 
            | AddFollower(subscriber) ->
                myFollowers <- List.append myFollowers [subscriber] 
            | RemoveFollower(subscriber) ->
                removeFromList(subscriber, myFollowers) |> ignore             
            | SubscribedToTweets -> // Views tweets of users they subscribed to (simulator)
                showTweets(SubscribedToTweets) 
            | HashTagTweets(hashtag) ->
                server <! HashTagTweets(hashtag)
            | ReceiveTweets(tweets) ->
                showTweets(tweets)
            | MyTweets -> // idk if this will be needed, just adding it here for now
                showTweets(myTweets)
            | Success -> 
                printfn "server message succeeded!"

        return! loop() 
    }
    loop()  

// will simulate users interacting with Twitter by sending messages to certain clients
let simulator() = 
    users.["0"] <! Subscribing("1") // user 0 subscribes to user 1
    users.["1"] <! Tweeting("yo")
    // we need to find way to wait for async call to finish before continuing, but not sure how in f#
    users.["0"] <! SubscribedToTweets // view tweets of who they follow
    

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
