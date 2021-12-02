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
let serverIp = "localhost"
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
    | Start of DateTime


// Configuration
let configuration = 
    ConfigurationFactory.ParseString(
        sprintf @"akka {            
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
//let mutable clients = [] 
let users = new Dictionary<string, ActorSelection>()
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

// this can be called by simulator to register a new account, which will start up a new actor
let registerAccount accountName = 
        printfn "adding user %s" accountName
        let client = system.ActorSelection ( sprintf "akka.tcp://TwitterClient@localhost:4000/user/%s" accountName)
        users.Add(accountName,client)
        connectionStatus.Add(accountName, true)

// this is used for testing "Simulate as many users as you can"
let registerClients(clients) = 
    for i in clients do 
        let name = i |> string
        registerAccount(name)
    //printfn "%i accounts created" (numOfAccounts)

let ServerActor (mailbox:Actor<_>) =
    // These Dicts will act as our DB
    let tweetsByHash = new Dictionary<string, string list>()
    let tweetsByUser = new Dictionary<string, string list>()
    let tweetsByMention = new Dictionary<string, string list>()
    // All users subscribed to a user
    let usersSubscribers = new Dictionary<string, string list>()
    let mutable serverStartTime = DateTime.Now 
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
        users.[subscribedToId:string] <! ("RemoveFollower",subscriberId,[""],"")

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
            then users.[user] <! ("AddTweet","subscribedTo",[""],tweetMsg)

    let addFollower(subscriberId, subscribedToId) = 
        if usersSubscribers.ContainsKey(subscribedToId)
        then List.append usersSubscribers.[subscribedToId] [subscriberId] |> ignore // append subscribers to list
        else usersSubscribers.Add(subscribedToId, [subscriberId]) // init subscriber list

        users.[subscribedToId] <! ("AddFollower",subscriberId,[""],"")

    let rec loop() = actor {
        let! (msg:obj) = mailbox.Receive() 
        let sender = mailbox.Sender()
        let (mtype,string1,string2,list1,list2,string3) : Tuple<string,string,string,string list,string list,string> = downcast msg
        //let client = system.ActorSelection( sprintf "akka.tcp://TwitterClient@%s:8776/user/ServerActor" serverIp)
        printfn "received message %A from %A" msg sender
        match mtype with
            | "Start" -> 
                let (_,tweet, id, _, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                serverStartTime <- DateTime.Now
            | "Tweet"-> 
                let (_,tweet, id, hashtags, mentions,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                publishTweet(tweet,id,hashtags,mentions)
                //sender <! Success // let client know we succeeded (idk if this is neccessary, but just adding it for now)
            | "Subscribe" -> 
                let (_,subscriber, subscribedTo, _, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                addFollower(subscriber, subscribedTo)
            | "Unsubscribe" -> 
                let (_,subscriber, subscribedTo, _, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                removeFollower(subscriber, subscribedTo)
            | "SubscribedTweets" -> 
                let (_,_, _, subs, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                let subscribedTweets = findTweets(subs, tweetsByUser)   
                sender <! ("ReceiveTweets","subscribedTo",subscribedTweets,"")
            | "HashTagTweets" -> 
                let (_,_, _, hashtags, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                let hashTweets = findTweets(hashtags, tweetsByHash)
                sender <! ("ReceiveTweets","hashTag",hashTweets,"")
            | "MentionedTweets" -> 
                let (_,userId, _, _, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                let mentionedTweets = findTweets([userId],tweetsByMention)
                sender <! ("ReceiveTweets","mentions",mentionedTweets,"")
            | "ToggleConnection" ->
                let (_, id, status, _, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                let mutable newStatus = false
                if status = "true"
                then newStatus <- true
                connectionStatus.[id] <- newStatus
            | "ReTweet"-> 
                let (_,tweet, id, hashtags, mentions, originalTweeter) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                let mutable ogTweeterUserId = originalTweeter
                while (not (tweetsByUser.ContainsKey(ogTweeterUserId))) do
                    ogTweeterUserId <- random.Next((numOfAccounts)) |> string 
                let rndTweet = tweetsByUser.[ogTweeterUserId].[0] // 0 is tmp
                let reTweet = " " + tweet + " Retweet: " + ogTweeterUserId + ": " + rndTweet // just modifying the tweet for retweeting
                publishTweet(reTweet,id,hashtags,mentions)
            | "RegisterClients" -> 
                let (_,_, _, clientIds, _,_) : Tuple<string,string,string,string list,string list,string> = downcast msg 
                registerClients(clientIds)
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


let server = spawn system "ServerActor" ServerActor
server <! ("Start","","",[""],[""],"")
system.WhenTerminated.Wait()
System.Console.ReadLine() |> ignore // return an integer exit code

    //'System.Tuple`6[System.String,System.String,System.String,Microsoft.FSharp.Collections.FSharpList`1[System.Object],Microsoft.FSharp.Collections.FSharpList`1[System.Object],System.String]'
    //'System.Tuple`6[System.String,System.String,System.String,Microsoft.FSharp.Collections.FSharpList`1[System.String],Microsoft.FSharp.Collections.FSharpList`1[System.String],System.String]'.
