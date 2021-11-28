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
let random = Random()

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
    | Start
    | ToggleConnection of string * bool

//let mutable clients = [] 
let users = new Dictionary<string, IActorRef>()
//connectionStatus is a dictionary of accountName and true/false depending on if theyre connected/not connected.
let connectionStatus = new Dictionary<string, bool>()
// These Dicts will act as our DB
let tweetsByHash = new Dictionary<string, string list>()
let tweetsByUser = new Dictionary<string, string list>()
let tweetsByMention = new Dictionary<string, string list>()

// All users subscribed to a user
let usersSubscribers = new Dictionary<string, string list>()




let findTweets (keys:string list, DB:Dictionary<string, string list>) =
    let mutable tweets = []
    for key in keys do
        if (DB.ContainsKey(key)) 
        then tweets <- List.append tweets DB.[key]
    tweets

let publishTweet (tweetMsg,id,hashtags,mentions) = 
    let tweet = "user: " + id + " tweeted: " + tweetMsg

    // add tweet to our "DB"
    if tweetsByUser.ContainsKey(id)
    then List.append tweetsByUser.[id] [tweet] |> ignore // append tweets to list
    else tweetsByUser.Add(id,[tweet]) // init tweet list for this user

    // add 
    for hashtag in hashtags do
        if tweetsByHash.ContainsKey(hashtag)
        then List.append tweetsByHash.[hashtag] [tweet] |> ignore // append tweets to list
        else tweetsByHash.Add(hashtag,[tweet]) // init tweet list for this hashtag 
    
    for mention in mentions do
        if tweetsByHash.ContainsKey(mention)
        then List.append tweetsByMention.[mention] [tweet] |> ignore // append tweets to list
        else tweetsByMention.Add(mention,[tweet]) // init tweet list for this mention 
    
    // TODO:
    // Shouldn't the tweet also go to the appropriate users? Its just being added to a list right now.
    // UserX should get a tweet if they are following UserY
    for user in usersSubscribers.[id] do 
        if (connectionStatus.[user] = true && users.ContainsKey(user))
        then users.[user] <! AddTweet(tweetMsg, "subscribedTo")

let addFollower(subscriberId, subscribedToId) = 
    if usersSubscribers.ContainsKey(subscribedToId)
    then List.append usersSubscribers.[subscribedToId] [subscriberId] |> ignore // append subscribers to list
    else usersSubscribers.Add(subscribedToId, [subscriberId]) // init subscriber list

    users.[subscribedToId] <! AddFollower(subscriberId)

// This is a helper function for removeFollower
// It simply removes an item from a list.
let rec remove_if l predicate =
    match l with
    | [] -> []
    | x::rest -> 
        if predicate(x) then
        (remove_if rest predicate)
        else x::(remove_if rest predicate)

let removeFollower(subscriberId, subscribedToId) = 
    // If the subscribed to item exists
    // Remove the subscriber from that list.
    if usersSubscribers.ContainsKey(subscribedToId)
    then usersSubscribers.[subscribedToId] <- remove_if usersSubscribers.[subscribedToId] (fun x -> x = subscriberId) // untested.
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
                sender <! ReceiveTweets(subscribedTweets,"myFeed")
            | HashTagTweets(hashtags) -> 
                let hashTweets = findTweets(hashtags, tweetsByHash)
                sender <! (hashTweets,"hashTag")
            // TODO: Your mentioned tweets does not have the right functionality
            // Right now it just pulls all tweets that a user posted
            // It does not pull all tweets that mention a user.
            | MentionedTweets(userId) -> 
                let mentionedTweets = findTweets([userId],tweetsByUser)
                sender <! ReceiveTweets(mentionedTweets,"mentions")
            | ToggleConnection(id, status) ->
                connectionStatus.[id] <- status


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
    liveData.Add("myFeed",[]) //myFeed is any tweets from users im subscribed to
    liveData.Add("subscribedTo",[]) //
    liveData.Add("hashTag",[]) // stores most recently loaded tweets by hashtag
    liveData.Add("mentions",[]) //
    let mutable connected = true
    let mutable mySubs: string list  = []
    let mutable myFollowers: string list  = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Start -> system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds(0.0), TimeSpan.FromMilliseconds(5000.0), fun () -> 
                // Every second there is a chance to disconnect or reconnect.
                // Currently it is 1/100 chance every second to disconnect, and 1/100 chance every second to reconnect.
                let randomNumber = random.Next(0, 60)
                if (not connected)
                then 
                    if (randomNumber <= 12) // 50 arbitrarily. 
                    then 
                        printfn "%s is re-connecting." id
                        connected <- true
                        server <! ToggleConnection(id, connected)
                        // if we connect, we should query as well.
                else 
                    // TWEET 
                    // <= 10 send a tweet
                    if (randomNumber <= 10) 
                    then 
                        printfn "%s is tweeting." id
                        let mutable randomUser = random.Next((users.Count)) // TODO: Just a random user -- can be changed
                        while (not (users.ContainsKey(string randomUser))) do
                            randomUser <- random.Next((users.Count))
                        let tweet = "Some random tweet " + (string randomUser)
                        let hashtag = "#Hashtag" + (string randomUser)
                        let mention = "@" + (string (randomUser)) // assume users start from 0 and increment 
                        server <! Tweet(tweet, id, [hashtag], [mention])
                    // SUBSCRIBER TO A USER
                    // <= 20 subscribe to a user
                    else if (randomNumber <= 20)
                    then 
                        // We will only do a subscribe IF we haven't subscribed to everyone yet.
                        // This will prevent us from getting in an infinite loop.

                        if ((liveData.["subscribedTo"]).Length < users.Count) 
                        then     
                            let mutable randomUser = random.Next((users.Count))
                            while (List.contains (string randomUser) (liveData.["subscribedTo"])) do 
                                randomUser <- random.Next((users.Count))
                            printfn "%s is subscribing to %i." id randomUser    
                            server <! Subscribe(id, string(randomUser))
                    // UNSUBSCRIBE
                    else if (randomNumber <= 30)
                    then 
                        // We will only unsubscribe if we have subscribed to someone already.
                        // This will prevent us from getting in an infinite loop
                        if ((liveData.["subscribedTo"]).Length > 0)
                        then 
                            let mutable randomUser = random.Next((liveData.["subscribedTo"].Length))
                            while (not (List.contains (string randomUser) (liveData.["subscribedTo"]))) do 
                                randomUser <- random.Next((users.Count))
                            printfn "%s is unsubscribing from %i." id randomUser    
                            server <! Unsubscribe(id, string(randomUser))
                    // SUBSCRIBED_TO_TWEETS
                    // RETRIEVE TWEETS FROM USERS THAT THIS USER IS SUBSCRIBED TO.
                    else if (randomNumber <= 40)
                    then
                        printfn "%s is requesting subscribed tweets." id    
                        server <! SubscribedTweets(mySubs)
                    // HASHTAG TWEETS
                    else if (randomNumber <= 49)
                    then
                        let mutable randomUser = random.Next((users.Count)) // TODO: Just a random user -- can be changed
                        while (not (users.ContainsKey(string randomUser))) do
                            randomUser <- random.Next((users.Count))
                        let hashtag = "#Hashtag" + (string randomUser)
                        printfn "%s is requesting the hashtag: %s." id hashtag    
                        server <! HashTagTweets([hashtag])
                    // DISCONNECT
                    else if (randomNumber = 50)
                    then 
                        printfn "%s is disconnecting." id   
                        connected <- false
                        server <! ToggleConnection(id, connected)
                    // MENTIONS
                    else if (randomNumber <= 60)
                    then
                        let mutable randomUser = random.Next((users.Count)) // TODO: Just a random user -- can be changed
                        while (not (users.ContainsKey(string randomUser))) do
                            randomUser <- random.Next((users.Count))
                        let mention = string randomUser
                        printfn "%s is requesting mentions of %s." id mention 
                        server <! MentionedTweets(mention)
                    // RETWEET
                )
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
                printfn "%s: followers now (after adding): %A" id myFollowers
            | RemoveFollower(subscriber) ->
                removeFromList(subscriber, myFollowers) |> ignore 
                printfn "%s: followers now (after removing): %A" id myFollowers
            | ReceiveTweets(tweets,tweetType) ->
                liveData.[tweetType] <- tweets // replace client side data 
                showTweets(tweets,tweetType)
            // Add tweet is for live loading data after its already been queried 
            | AddTweet(tweet,tweetType) -> // server telling us someone we subscribed to tweeted (can be used for live data)
                //printfn("adding tweet: %s") tweet
                printfn "%s received a new tweet: %s" id tweet

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
        connectionStatus.Add(accountName, true)
        usersSubscribers.Add(accountName, [])

// this is used for testing "Simulate as many users as you can"
let registerAccounts numAccounts = 
    for i in 0..numAccounts do 
        let name = i |> string
        registerAccount(name)
        // Added <! Start to enable disconnect and reconnect.
        users.[name] <! Start
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
    let accountNum = inputLine |> int // cast to int
    registerAccounts(accountNum) // init some test accounts
    // simulator()

    // TODO: "You need to measure various aspects of your simulator and report performance"
    System.Console.ReadLine() |> ignore
    0 // return an integer exit code
