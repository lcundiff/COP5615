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
    | Tweeting of string * string list * string list // trigger client to tweet -> will be triggered by simulator
    | Tweet of string * string * string list * string list // send tweet to server 
    | ReTweet of string * string * string // send tweet to server 
    | ReTweeting of string * string list * string list * string // trigger client to retweet -> will be triggered by simulator
    | SubscribedTweets of string list
    | HashTagTweets of string list // query for tweets with specific hashtag
    | MentionedTweets of string // query for tweets with specific user mentioned
    | ReceiveTweets of string list * string
    | AddTweet of string * string
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

// These Dicts will act as our DB
let tweetsByHash = new Dictionary<string, string list>()
let tweetsByUser = new Dictionary<string, string list>()
//let subsByUser = new Dictionary<string, string list>()




let findTweets (keys:string list, DB:Dictionary<string, string list>) =
    let mutable tweets = []
    for key in keys do
        tweets <- List.append tweets DB.[key]
    tweets

let publishTweet (tweetMsg,id,hashtags,ogTweeter) = 
    let tweet = "user: " + id + " tweeted: " + tweetMsg

    // add tweet to our "DB"
    if tweetsByUser.ContainsKey(id)
    then List.append tweetsByUser.[id] [tweet] |> ignore // append tweets to list
    else tweetsByUser.Add(id,[tweet]) // init tweet list for this user
    
    for hashtag in hashtags do
        if tweetsByUser.ContainsKey(hashtag)
        then List.append tweetsByHash.[id] [tweet] |> ignore // append tweets to list
        else tweetsByHash.Add(hashtag,[tweet]) // init tweet list for this hashtag 
        


let addFollower(subscriberId, subscribedToId) = 
    users.[subscribedToId:string] <! AddFollower(subscriberId)

let removeFollower(subscriberId, subscribedToId) = 
    users.[subscribedToId:string] <! RemoveFollower(subscriberId)

let server = spawn system (string id) <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweet(tweets, id, hashtags, mentions) ->
                publishTweet(tweets,id,hashtags,mentions)
                sender <! Success // let client know we succeeded (idk if this is neccessary, but just adding it for now)
            | Subscribe(subscriber, subscribedTo) -> 
                addFollower(subscriber, subscribedTo)
            | Unsubscribe(subscriber, subscribedTo) -> 
                removeFollower(subscriber, subscribedTo)
            | SubscribedTweets(subs) -> 
                let subscribedTweets = findTweets(subs, tweetsByUser)
                sender <! ReceiveTweets(subscribedTweets,"subscribedTo")
            | HashTagTweets(hashtags) -> 
                let hashTweets = findTweets(hashtags, tweetsByHash)
                sender <! ReceiveTweets(hashTweets,"hashTag")
            | MentionedTweets(userId) -> 
                let mentionedTweets = findTweets([userId],tweetsByUser)
                sender <! ReceiveTweets(mentionedTweets,"mentions")

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
    
    let showTweets (tweets:string list, tweetType:string) = 
        printfn "%s tweets %A" tweetType tweets |> ignore // placeholder
    
    // store client-side data for "live delivery" as described in project description
    let liveData = new Dictionary<string, string list>()
    liveData.Add("myTweets",[])
    liveData.Add("subscribedTo",[])
    liveData.Add("hashTag",[]) // stores most recently loaded tweets by hashtag
    liveData.Add("mentions",[])

    let mutable mySubs: string list  = []
    let mutable myFollowers: string list  = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweeting(tweetMsg,hashTags,mentions) -> // user tweeted (triggered by simulator)
                //printfn("tweeting")
                liveData.["myTweets"] <- List.append liveData.["myTweets"] [tweetMsg] // store users own tweets
                server <! Tweet(tweetMsg,id,hashTags,mentions) // tell the server to send tweet to subscribers
                sender <! Success
            | ReTweeting(tweet,hashtags,mentions,tweeter) ->
                let reTweet = "retweet from user: " + tweeter + ": " + tweet // just modifying the tweet for retweeting
                server <! Tweet(reTweet,id,hashtags,mentions) // do same as normal tweet but send the original author (tweeter)
            | Subscribing(subscribedTo) -> // user clicked subscribe on a user (triggered by simulator)
                //printfn("subscribing")
                mySubs <- List.append mySubs [subscribedTo] 
                server <! Subscribe(id,subscribedTo) 
                sender <! Success // async needs this to continue
            | Unsubscribing(subscribedTo) -> // simulator trigger unsubscribe
                removeFromList(subscribedTo, mySubs) |> ignore
                server <! Unsubscribe(id,subscribedTo)
            | AddFollower(subscriber) ->
                myFollowers <- List.append myFollowers [subscriber] 
            | RemoveFollower(subscriber) ->
                removeFromList(subscriber, myFollowers) |> ignore             
            | SubscribedToTweets -> // Views tweets of users they subscribed to (simulator)
                //printfn("here2")
                server <! SubscribedTweets(mySubs)
            | HashTagTweets(hashtag) ->
                server <! HashTagTweets(hashtag)
            | MentionedTweets(userId) ->
                server <! MentionedTweets(userId)
            | ReceiveTweets(tweets,tweetType) ->
                liveData.[tweetType] <- tweets // replace client side data 
                showTweets(tweets,tweetType)
            | MyTweets -> // idk if this will be needed, just adding it here for now
                 printfn "my tweets %A" liveData.["myTweets"] |> ignore 
            // Add tweet is for live loading data after its already been queried 
            | AddTweet(tweet,tweetType) -> // server telling us someone we subscribed to tweeted (can be used for live data)
                //printfn("adding tweet: %s") tweet
                liveData.[tweetType] <- List.append liveData.[tweetType] [tweet] 
            | Success -> 
                printfn "server message succeeded!"

        return! loop() 
    }
    loop()  

// this can be called by simulator to register a new account, which will start up a new actor
let registerAccount accountName = 
        let clientActor = client accountName
        users.Add(accountName,clientActor)

// this is used for testing "Simulate as many users as you can"
let registerAccounts numAccounts = 
    for i in 0..numAccounts do 
        let name = i |> string
        registerAccount(name)
    printfn "%i accounts created" numAccounts
        
// will simulate users interacting with Twitter by sending messages to certain clients
let simulator() = 
    // user 0 subscribes to user 1
    let subscribingRes = ( users.["0"] <? Subscribing("1") )
    let subscribed = Async.RunSynchronously (subscribingRes, 1000)
    
    let tweeted = Async.RunSynchronously (users.["1"] <? Tweeting("yo",["#yo"],["@0"]), 1000)
    
    users.["0"] <! SubscribedToTweets // view tweets of who they follow

    // TODO: "Simulate periods of live connection and disconnection for users"

    // TODO: "Simulate a Zipf distribution on the number of subscribers. For accounts with a lot of subscribers, increase the number of tweets. Make some of these messages re-tweets"

[<EntryPoint>]
let main argv = 
    printfn "Welcome to Twitter Simulator, how many accounts would you like to create?"
    let inputLine = Console.ReadLine() 
    let inputLine2 = Console.ReadLine() 
    let accountNum = inputLine |> int // cast to int
    registerAccounts(accountNum) // init some test accounts
    simulator()

    // TODO: "You need to measure various aspects of your simulator and report performance"
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
