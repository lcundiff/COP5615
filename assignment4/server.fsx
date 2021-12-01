#r "nuget: Akka"
#r "nuget: Akka.FSharp"
#r "nuget: Akka.Remote"
open System
open System.Security.Cryptography
open System.Text
open System.Diagnostics
open Akka.Actor
open Akka.Configuration
open Akka.FSharp

open System.Collections.Generic

//let system = ActorSystem.Create("FSharp")
let serverIp = "127.0.0.1"
let random = Random()
let mutable numOfAccounts = 0
type Message =
    | Tweet of string * string * string list * string list // send tweet to server (tweet, id, hashtag list, mentions list)
    | ReTweet of string * string * string list * string list * string// send tweet to server 
    | SubscribedTweets of string list
    | HashTagTweets of string list // query for tweets with specific hashtag
    | MentionedTweets of string // query for tweets with specific user mentioned
    | ReceiveTweets of string list * string
    | AddTweet of string * string
    | Subscribe of string * string // user ids of who subscribed to who
    | AddFollower of string
    | RemoveFollower of string
    | Unsubscribe of string * string
    | SubscribedToTweets
    | Success 
    | Simulate
    | ToggleConnection of string * bool


// Configuration
let configuration = 
    ConfigurationFactory.ParseString(
        sprintf @"akka {            
            stdout-loglevel : DEBUG
            loglevel : ERROR
            actor {
                provider = ""Akka.Remote.RemoteActorRefProvider, Akka.Remote""
            }
            remote.helios.tcp {
                transport-protocol = tcp
                port = 8776
                hostname = %s
            }
        }" serverIp)

let system = ActorSystem.Create("TwitterServer", configuration)
//let reomoteServer = system.ActorSelection( sprintf "akka.tcp://TwitterServer@%s:8776/user/ServerActor" serverIp)
//let mutable clients = [] 
let users = new Dictionary<string, IActorRef>()
//connectionStatus is a dictionary of accountName and true/false depending on if theyre connected/not connected.
let connectionStatus = new Dictionary<string, bool>()

let _lock = Object()

let mutable (zipfSubscribers: string list) = []


let createRndWord() = 
    let rndCharCount = random.Next(0, 10) // word will be between 0-10 chars
    let chars = Array.concat([[|'a' .. 'z'|];[|'A' .. 'Z'|];[|'0' .. '9'|]])
    let sz = Array.length chars in
    String(Array.init rndCharCount (fun _ -> chars.[random.Next sz]))

let findTweets (keys:string list, DB:Dictionary<string, string list>) =
    let mutable tweets = []
    for key in keys do
        if (DB.ContainsKey(key)) 
        then tweets <- List.append tweets DB.[key]
    tweets


let server = spawn system "server" <| fun mailbox ->
    // These Dicts will act as our DB
    let tweetsByHash = new Dictionary<string, string list>()
    let tweetsByUser = new Dictionary<string, string list>()
    let tweetsByMention = new Dictionary<string, string list>()
    // All users subscribed to a user
    let usersSubscribers = new Dictionary<string, string list>()

    // This is a helper function for removeFollower
    // It simply removes an item from a list.
    let rec remove_if l predicate =
        match l with
        | [] -> []
        | x::rest -> 
            if predicate(x) 
            then
                (remove_if rest predicate)
            else x::(remove_if rest predicate)

    let removeFollower(subscriberId, subscribedToId) = 
        // If the subscribed to item exists
        // Remove the subscriber from that list.
        if usersSubscribers.ContainsKey(subscribedToId)
        then 
            usersSubscribers.[subscribedToId] <- remove_if usersSubscribers.[subscribedToId] (fun x -> x = subscriberId) // untested.
            zipfSubscribers <- List.append zipfSubscribers [subscriberId]
        users.[subscribedToId:string] <! RemoveFollower(subscriberId)

    let publishTweet (tweetMsg,id,hashtags,mentions) = 
        let tweet = "user: " + id + " tweeted: " + tweetMsg

        // add tweet to our "DB" - byUser, byHash and byMention
        if tweetsByUser.ContainsKey(id)
        then List.append tweetsByUser.[id] [tweet] |> ignore // append tweets to list
        else tweetsByUser.Add(id,[tweet]) // init tweet list for this user

        for hashtag in hashtags do
            if tweetsByHash.ContainsKey(hashtag)
            then List.append tweetsByHash.[hashtag] [tweet] |> ignore // append tweets to list
            else tweetsByHash.Add(hashtag,[tweet]) // init tweet list for this hashtag 
        
        for mention in mentions do
            if tweetsByMention.ContainsKey(mention)
            then List.append tweetsByMention.[mention] [tweet] |> ignore // append tweets to list
            else tweetsByMention.Add(mention,[tweet]) // init tweet list for this mention 
        
        // Shouldn't the tweet also go to the appropriate users? Its just being added to a list right now.
        // UserX should get a tweet if they are following UserY
        // the tweet is going to the user below? Also, if the user queries subscribedTo, they will get tweet from DB
        // below is just to update the client's live data
        if (not (usersSubscribers.ContainsKey(id)))
        then usersSubscribers.Add(id,[])
        
        for user in usersSubscribers.[id] do 
            if (connectionStatus.[user] = true && users.ContainsKey(user))
            then users.[user] <! AddTweet(tweetMsg, "subscribedTo")

    let addFollower(subscriberId, subscribedToId) = 
        if usersSubscribers.ContainsKey(subscribedToId)
        then List.append usersSubscribers.[subscribedToId] [subscriberId] |> ignore // append subscribers to list
        else usersSubscribers.Add(subscribedToId, [subscriberId]) // init subscriber list

        users.[subscribedToId] <! AddFollower(subscriberId)

    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        //printfn "received message %A from %A" msg sender
        match msg with
            | Tweet(tweet, id, hashtags, mentions) ->
                publishTweet(tweet,id,hashtags,mentions)
                //sender <! Success // let client know we succeeded (idk if this is neccessary, but just adding it for now)
            | Subscribe(subscriber:string, subscribedTo:string) -> 
                addFollower(subscriber, subscribedTo)
            | Unsubscribe(subscriber, subscribedTo) -> 
                removeFollower(subscriber, subscribedTo)
            | SubscribedTweets(subs: string list) -> 
                let subscribedTweets = findTweets(subs, tweetsByUser)   
                sender <! ReceiveTweets(subscribedTweets,"subscribedTo")
            | HashTagTweets(hashtags: string list) -> 
                let hashTweets = findTweets(hashtags, tweetsByHash)
                sender <! ReceiveTweets(hashTweets,"hashTag")
            | MentionedTweets(userId: string) -> 
                let mentionedTweets = findTweets([userId],tweetsByMention)
                sender <! ReceiveTweets(mentionedTweets,"mentions")
            | ToggleConnection(id, status) ->
                connectionStatus.[id] <- status
            | ReTweet(tweet, id, hashtags, mentions,originalTweeter) -> 
                let mutable ogTweeterUserId = originalTweeter
                while (not (tweetsByUser.ContainsKey(ogTweeterUserId))) do
                    ogTweeterUserId <- random.Next((numOfAccounts)) |> string 
                let rndTweet = tweetsByUser.[ogTweeterUserId].[0] // 0 is tmp
                let reTweet = " " + tweet + " Retweet: " + ogTweeterUserId + ": " + rndTweet // just modifying the tweet for retweeting
                publishTweet(reTweet,id,hashtags,mentions)
            | _ ->  
                printfn "ERROR: server recieved unrecognized message"


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

let removeAt index list =
    list |> List.indexed |> List.filter (fun (i, _) -> i <> index) |> List.map snd


System.Console.ReadLine() |> ignore // return an integer exit code

    