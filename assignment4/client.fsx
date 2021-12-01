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
let clientIp = "127.0.0.1"
let serverIp = "127.0.0.1"
let port = "4000"
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
                port = %s
                hostname = %s
            }
    }" port clientIp)

let system = ActorSystem.Create("TwitterClient", configuration)
let server = system.ActorSelection( sprintf "akka.tcp://TwitterServer@%s:8776/user/ServerActor" serverIp)
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


let removeFromList(sub, subs) =
    subs 
    // Associate each element with a boolean flag specifying whether 
    // we want to keep the element in the resulting list
    |> List.mapi (fun i el -> (el <> sub, el)) 
    // Remove elements for which the flag is 'false' and drop the flags
    |> List.filter fst |> List.map snd

let removeAt index list =
    list |> List.indexed |> List.filter (fun (i, _) -> i <> index) |> List.map snd


let tweet(id:string,rndUserId:string, liveData:Dictionary<string,string list>,rndNum:int) = 
    let hashtag = "#" + createRndWord()
    let mention = (rndUserId) // assume users start from 0 and increment 
    let tweetMsg = createRndWord()
    let mutable tweet = "User: " + id + " tweeted @" + mention
    tweet <- tweet + " " + hashtag

    if(rndNum <= 2) // 2/10 times its a retweet
    then
        printfn "%s is retweeting." id
        let mutable rndUserId2 = random.Next((numOfAccounts)) |> string 
        server <! ReTweet(tweet, id, [hashtag], [mention],rndUserId2)
        List.append liveData.["myTweets"] [tweet] 
    else 
        printfn "%s is tweeting." id
        server <! Tweet(tweet, id, [hashtag], [mention])
        List.append liveData.["myTweets"] [tweet] 

let subscribe(id:string,rndUserId:string, liveData:Dictionary<string,string list>,mailbox:Actor<_>) = 
    let mutable rndNonSubUserId = rndUserId
    printfn "%s: in the subscribe method: mySubs %A" id (liveData.["mySubs"])
    lock _lock (fun () -> 
        // We will only do a subscribe IF we haven't subscribed to everyone yet.
        // This will prevent us from getting in an infinite loop. 
        // ZIPF Distribution
        if(zipfSubscribers.Length > 0 )
        then 
            let mutable randomSubscriberIndex = random.Next((zipfSubscribers.Length))
            let mutable index = 0 // Index is just a variable that ensures we don't get stuck in this loop forever.
            while (List.contains zipfSubscribers.[randomSubscriberIndex] liveData.["mySubs"] && index < 100) do 
                randomSubscriberIndex <- random.Next((zipfSubscribers.Length))
                index <- index + 1
            // Gets the id
            //printfn "index %i" randomSubscriberIndex
            rndNonSubUserId <- zipfSubscribers.[randomSubscriberIndex]
            // Removes index from list.
            zipfSubscribers <- removeAt randomSubscriberIndex zipfSubscribers
            // printfn "zipfSubscribers: %A" zipfSubscribers
    )
    printfn "%s is subscribing to %s." id rndNonSubUserId    
    server.Tell(Subscribe(id, rndNonSubUserId), mailbox.Self)
    List.append liveData.["mySubs"] [rndNonSubUserId] // update local data

let unsubscribe(id:string, liveData:Dictionary<string,string list>,mailbox:Actor<_>) =
    // We will only unsubscribe if we have subscribed to someone already.
    // This will prevent us from getting in an infinite loop
    if ((liveData.["mySubs"]).Length > 1)
    then 
        let randomSubIndex = random.Next(1, (liveData.["mySubs"].Length))
        let randomSubUserId = liveData.["mySubs"].[randomSubIndex]
        printfn "%s is unsubscribing from %s." id randomSubUserId    
        server.Tell(Unsubscribe(id, randomSubUserId), mailbox.Self)
        lock _lock (fun () ->
            removeFromList(randomSubUserId, liveData.["mySubs"]) |> ignore // remove local data
        )

let client (id: string) = spawn system (string id) <| fun mailbox ->
    
    let showTweets (tweets:string list, tweetType:string) = 
        printfn "%s tweets %A" tweetType tweets |> ignore // placeholder
    
    // store client-side data for "live delivery" as described in project description
    let liveData = new Dictionary<string, string list>()
    liveData.Add("myTweets",[]) // stores local live data of all tweets of this user
    liveData.Add("subscribedTo",[]) // stores local live data of any tweets from users im subscribed to
    liveData.Add("hashTag",[]) // stores local live data of most recent query of tweets by hashtag
    liveData.Add("mentions",[]) // stores local live data of most recent query of tweets by mentions from a specific user (could be me)
    liveData.Add("mySubs",[]) // stores local live data of user ids of who im subsribed to
    let mutable connected = true
    let mutable myFollowers: string list  = []
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Simulate ->   
                system.Scheduler.Advanced.ScheduleRepeatedly (TimeSpan.FromMilliseconds(0.0), TimeSpan.FromMilliseconds(1000.0), fun () -> 
                printfn "mailbox2 %A" mailbox.Self

                // ==== Cause Users with More Followers To Tweet More Often ====
                let mutable divisor = float numOfAccounts / float 20

                let increasedTweets = float myFollowers.Length / divisor
                let tweetProbability = 10 + int increasedTweets
                let randomMax = 60 + int increasedTweets
                // ==============================================================

                let randomNumber = random.Next(0, randomMax)
                printfn "%s has %d followers, probability to tweet is : %d" id myFollowers.Length tweetProbability
                // Every second there is a chance to disconnect or reconnect.
                // Currently it is 1/60 chance every second to disconnect, and 1/5 chance every second to reconnect.
                if (not connected)
                then 
                    if (randomNumber <= (randomMax / 5)) // Gives a 20% chance to reconnect.
                    then 
                        printfn "%s is re-connecting." id
                        connected <- true
                        server <! (ToggleConnection(id, connected), mailbox.Self) // if we connect, we should query as well. TODO: Logan?
                else 
                    let mutable randomUserId = random.Next((numOfAccounts)) |> string // TODO: Just a random user -- can be changed
                    while (not (users.ContainsKey(randomUserId))) do // is this neccessary? 
                        randomUserId <- random.Next((numOfAccounts)) |> string
                    // TWEET 
                    if (randomNumber <= tweetProbability) 
                    then liveData.["myTweets"] <- tweet(id,randomUserId,liveData,randomNumber)

                    // SUBSCRIBER TO A USER
                    else if (randomNumber <= tweetProbability + 10)
                    then liveData.["mySubs"] <- subscribe(id,randomUserId,liveData,mailbox)
                    // UNSUBSCRIBE
                    else if (randomNumber <= tweetProbability + 20)
                    then unsubscribe(id,liveData,mailbox)

                    // RETRIEVE SUBSCRIBED_TO_TWEETS: TWEETS FROM USERS THAT THIS USER IS SUBSCRIBED TO.
                    else if (randomNumber <= tweetProbability + 30)
                    then
                        printfn "%s is requesting subscribed tweets." id    
                        server.Tell(SubscribedTweets(liveData.["mySubs"]), mailbox.Self)
                    // RETRIEVE TWEETS FOR HASHTAG
                    else if (randomNumber <= tweetProbability + 39)
                    then
                        let rndWord = createRndWord() 
                        let hashtag = "#" + (rndWord) 
                        printfn "%s is requesting the hashtag: %s." id hashtag    
                        server.Tell(HashTagTweets([hashtag]), mailbox.Self)
                    // RETRIEVE TWEETS FROM MENTIONS
                    // DISCONNECT
                    else if (randomNumber = tweetProbability + 40) // Gives a 1/60 - 1/80 chance to disconnect.
                    then 
                        printfn "%s is disconnecting." id   
                        connected <- false
                        server <! ToggleConnection(id, connected)
                    else if (randomNumber <= tweetProbability + 50)
                    then
                        printfn "%s is requesting mentions of %s." id randomUserId 
                        server.Tell(MentionedTweets(randomUserId), mailbox.Self)
                )
            | AddFollower(subscriber) ->
                myFollowers <- List.append myFollowers [subscriber] 
                printfn "User %s: followers now (after adding): %A" id myFollowers
            | RemoveFollower(subscriber) ->
                removeFromList(subscriber, myFollowers) |> ignore 
                printfn "%s: followers now (after removing): %A" id myFollowers
            | ReceiveTweets(tweets:string list,tweetType:string) ->
                printfn "received tweets %A" tweets 
                liveData.[tweetType] <- tweets // replace client side data 
                showTweets(tweets,tweetType)
            // Add tweet is for live loading data after its already been queried 
            | AddTweet(tweet,tweetType) -> // server telling us someone we subscribed to tweeted (can be used for live data)
                //printfn("adding tweet: %s") tweet
                printfn "%s received a new tweet: %s" id tweet
                liveData.[tweetType] <- List.append liveData.[tweetType] [tweet] 
            | Success -> 
                printfn "server message succeeded!"
            | _ -> 
                printfn "ERROR: client recieved unrecognized message"

        return! loop() 
    }
    loop()  

// this can be called by simulator to register a new account, which will start up a new actor
let registerAccount accountName = 
        let clientActor = client accountName
        users.Add(accountName,clientActor)
        connectionStatus.Add(accountName, true)
        //usersSubscribers.Add(accountName, [])

// this is used for testing "Simulate as many users as you can"
let registerAccounts() = 
    for i in 0..numOfAccounts-1 do 
        let name = i |> string
        registerAccount(name)
        let accountList = [for n in 0 .. (numOfAccounts/(i+1))-1 -> (string i)]
        zipfSubscribers <- List.append zipfSubscribers accountList
    printfn "%i accounts created" (numOfAccounts)
        
// will simulate users interacting with Twitter by sending messages to certain clients
let simulator() = 
    for i in 0..numOfAccounts-1 do 
        let name = i |> string
        users.[name] <! Simulate // Added <! Simulate to enable disconnect and reconnect.
    
    //let tweeted = Async.RunSynchronously (users.["1"] <? Tweeting("yo",["#yo"],["@0"]), 1000)
    // TODO: "Simulate a Zipf distribution on the number of subscribers. For accounts with a lot of subscribers, increase the number of tweets. Make some of these messages re-tweets"



printfn "Welcome to Twitter Simulator, how many accounts would you like to create?"
let inputLine = Console.ReadLine() 
numOfAccounts <- (inputLine |> int) // please leave this as (inputLine |> int) do not add a -1 to this or it will mess up my code. i adjusted everything else with -1  
registerAccounts() // init some test accounts
printfn "%i zipfSubscribers: %A" zipfSubscribers.Length zipfSubscribers
//users.["0"] <! ReceiveTweets(["yo"],"mentions")
simulator() // go through those accounts and start simulations for each

// TODO: "You need to measure various aspects of your simulator and report performance"
System.Console.ReadLine() |> ignore // return an integer exit code

    