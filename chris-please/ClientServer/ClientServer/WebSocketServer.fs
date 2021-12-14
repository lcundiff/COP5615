module WebSharper.AspNetCore.Tests.WebSocketServer

open WebSharper
open WebSharper.AspNetCore.WebSocket.Server
open System
open System.Threading
open System.Threading.Tasks
open System.Net.Sockets
open System.IO
open System.Linq;
open System.Collections.Generic
open Newtonsoft.Json
open WebSharper.JavaScript

type MessagesToServer = 
    | Tweet of tweet : string
    | Register of account : string
    | Subscribe of userToSubTo: string
    | Unsubscribe of userToUnsubTo: string
    | Query of query: string
    | Login
    | Logout

type MessagesToClient = 
    | Connection of con : string
    | TweetFromServer of tweet : string
    | RegisteredFromServer
    | Success of succ: string
    | Failure of fail: string
    | QueryFromServer of tweet : string
    | MissedTweet of tweet: string


[<System.SerializableAttribute>]
type Tweet() =
    [<DefaultValue>]
    val mutable msg : string
    [<DefaultValue>]
    val mutable user : string

type User() = 
    [<DefaultValue>]
    val mutable user: string
    [<DefaultValue>]
    val mutable onlineStatus: int
    [<DefaultValue>]
    val mutable twitterFeed: List<Tweet>
    [<DefaultValue>]
    val mutable guId: string


let ServerStart() : StatefulAgent<MessagesToClient, MessagesToServer, int> = 
    // So we needed 3 dictionaries.
    // One maps userNames to Users
    // Another maps guIds to userNames
    // And the last maps userNames to guIds
    let mutable userNameAndUser = new Dictionary<string, User>()
    let mutable guIdAndUserName = new Dictionary<string, string>()
    let mutable userNameAndGuId = new Dictionary<string, string>()

    let mutable userNamesAndTheirSubscribers = new Dictionary<string, string list>()
    let mutable userNamesAndTweetsWithMentions = new Dictionary<string, string list>()
    let mutable hashtagsAndTweetsWithHashtags = new Dictionary<string, string list>()
    // This is just Users.
    let mutable users = new List<User>()


    let mutable clientGuIdAndWebsocket = new Dictionary<string, WebSocketClient<MessagesToClient, MessagesToServer>>()

    let createUser (account:string) (guId: string) = 
        let user = new User()
        user.user <- account
        user.onlineStatus <- 1
        user.twitterFeed <- new List<Tweet>()
        user.guId <- guId
        user

    let createTweet(user: string) (msg: string) = 
        let tweet = new Tweet()
        tweet.msg <- msg
        tweet.user <- user
        tweet

    
    let writeMessage (cl:WebSocketClient<MessagesToClient,MessagesToServer>) msg =
        async {
            do! cl.PostAsync(msg)
            printfn "We sent!"
        }

    let sendMessageWithClientId clientId msg  = 
        let clientStream = clientGuIdAndWebsocket.[clientId]
        printfn "ClientId %s" clientId 
        writeMessage clientStream msg |> Async.Ignore

    let removeAt index list =
        list |> List.indexed |> List.filter (fun (i, _) -> i <> index) |> List.map snd

    // Connects To Client via Websocket
    fun handleClientMessages -> async {
        let ip = handleClientMessages.Connection.Context.Connection.RemoteIpAddress.ToString()
        // Gets guid
        // GuId is the id of the client, thus we need a way to translate the GuId to the username
        // for that, we created a lot of dictionaries. 
        let clientGuId = System.Guid.NewGuid().ToString()
        clientGuIdAndWebsocket.Add(clientGuId, handleClientMessages)
        return 0, fun state msg -> async {
            printfn "Received message at state: %i from client: %s" state clientGuId
            match msg with 
            | Message data ->
                printfn "It is a message; I have %i users" (users.Count)
                match data with
                // All client information is received in Register, we only update it when people subscribe or whatnot
                | Register account ->
                    // TODO: Handle duplicate accounts?
                    printfn "Registered"
                    if (account.Length = 0)
                    then do! (sendMessageWithClientId (clientGuId) (Failure "Please enter a username with at least 1 character.")) |> Async.Ignore
                    else if (userNameAndUser.ContainsKey(account))
                    then do! (sendMessageWithClientId (clientGuId) (Failure "This user is already registered")) |> Async.Ignore
                    else 
                        let user = createUser account clientGuId
                        userNameAndUser.Add(account, user) |> ignore
                        guIdAndUserName.Add(clientGuId, account) |> ignore
                        userNameAndGuId.Add(account, clientGuId) |> ignore

                        users.Add(user) |> ignore
                        // I automatically make the user subscribe to themselves so they receive their own tweets.
                        userNamesAndTheirSubscribers.Add(account, [account])
                        do! (sendMessageWithClientId (clientGuId) (RegisteredFromServer)) |> Async.Ignore
                | Login ->
                    // When people log in we are going to auto-send all the messages they missed out on.
                    let userName = guIdAndUserName.[clientGuId]
                    let user = userNameAndUser.[userName]
                    if (user.onlineStatus = 1) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "You're already logged in!")) |> Async.Ignore
                    else if (user.onlineStatus = 0) then 
                        user.onlineStatus <- 1
                        do! (sendMessageWithClientId (clientGuId) (Success "You logged in successfully.")) |> Async.Ignore
                        for tweet in user.twitterFeed do 
                            do! (sendMessageWithClientId (clientGuId) (MissedTweet (JsonConvert.SerializeObject(tweet)))) |> Async.Ignore      
                        user.twitterFeed <- new List<Tweet>()
                        // Delete all missed tweets after
                | Logout ->
                    let userName = guIdAndUserName.[clientGuId]
                    let user = userNameAndUser.[userName]
                    if (user.onlineStatus = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "You're already logged out!")) |> Async.Ignore
                    else if (user.onlineStatus = 1) then 
                        user.onlineStatus <- 0
                        do! (sendMessageWithClientId (clientGuId) (Success "You logged out successfully.")) |> Async.Ignore
                | Query query ->
                    let userName = guIdAndUserName.[clientGuId]
                    let user = userNameAndUser.[userName]
                    if (user.onlineStatus = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "You're logged out cannot request messages.")) |> Async.Ignore
                    else if (query.Length = 0 || query.Length = 1)
                    then do! (sendMessageWithClientId (clientGuId) (Failure "Please input a valid query of length > 1.")) |> Async.Ignore
                    else if ((not (string(query.[0]) = "@")) && (not (string(query.[0]) = "#")))
                    then do! (sendMessageWithClientId (clientGuId) (Failure "Must start with an @ or #.")) |> Async.Ignore
                    else 
                        let symbol = string(query.[0])
                        // First part of query is symbol
                        // Rest is the phrase they want.
                        let word = query.Substring(1)
                        if (symbol = "#") 
                        then
                            if (hashtagsAndTweetsWithHashtags.ContainsKey(word)) then
                                for tweet in hashtagsAndTweetsWithHashtags.[word] do 
                                    do! (sendMessageWithClientId (clientGuId) (QueryFromServer tweet)) |> Async.Ignore
                            else 
                            do! (sendMessageWithClientId (clientGuId) (Failure ("No hashtags with the tag #" + word))) |> Async.Ignore
                        else if (symbol = "@")
                        then
                            if (userNamesAndTweetsWithMentions.ContainsKey(word)) then
                                for tweet in userNamesAndTweetsWithMentions.[word] do
                                    do! (sendMessageWithClientId (clientGuId) (QueryFromServer tweet)) |> Async.Ignore
                            else 
                            do! (sendMessageWithClientId (clientGuId) (Failure ("No mentions with the @" + word))) |> Async.Ignore
                        else 
                            do! (sendMessageWithClientId (clientGuId) (Failure ("Not a valid symbol: " + symbol))) |> Async.Ignore

                | Tweet msgTweet ->
                    
                    // TODO: Should have a check to make sure user is registed && "logged-in" (not disconnected)
                    let (senderUserName:string) = guIdAndUserName.[clientGuId]
                    let sender = userNameAndUser.[senderUserName]
                    if (sender.onlineStatus = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "Cannot tweet while you're offline")) |> Async.Ignore
                    else if (msgTweet.Length = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "Cannot tweet empty message.")) |> Async.Ignore
                    else 
                        let tweetToDistribute = createTweet senderUserName msgTweet
                        // TODO: We need to determine who the tweet should get sent to, but for now we will send it to everyone
                        // You can only receive tweets from a user you are subscribed to.
                        let subs = userNamesAndTheirSubscribers.[senderUserName]
                    
                        // Extraction -- Extract Hashtags and Mentions
                        let charArr = msgTweet |> List.ofSeq
                        let mutable currentChar = ""
                        let mutable characterIndex = 0
                        let mutable mentionedUsers = []
                        while (characterIndex < charArr.Length) do
                            currentChar <- string(charArr.[characterIndex])
                            // If we find an @, add the tweet to the userName
                            // We are assuming valid @s and Tweets
                            // I did a ton of error checking here but basically if it ends in a special character thats okay
                            if (characterIndex < (charArr.Length - 1) && currentChar = "@")
                            then
                                let mutable userName = ""
                                characterIndex <- characterIndex + 1
                                currentChar <- string(charArr.[characterIndex])
                                while ((characterIndex < charArr.Length) && (not (string(charArr.[characterIndex]) = " "))) do 
                                    currentChar <- string(charArr.[characterIndex])
                                    userName <- userName + string(currentChar)
                                    characterIndex <- characterIndex + 1
                                if (userNamesAndTweetsWithMentions.ContainsKey(userName))
                                then 
                                    userNamesAndTweetsWithMentions.[userName] <- List.append userNamesAndTweetsWithMentions.[userName] [(JsonConvert.SerializeObject(tweetToDistribute))]
                                else
                                    userNamesAndTweetsWithMentions.Add(userName, [(JsonConvert.SerializeObject(tweetToDistribute))])
                                mentionedUsers <- List.append mentionedUsers [userName]
                                printfn "Identified a mentioned user: @%s" userName
                            if (characterIndex < (charArr.Length - 1) && currentChar = "#")
                            then 
                                let mutable hashtag = ""
                                characterIndex <- characterIndex + 1
                                currentChar <- string(charArr.[characterIndex])
                                while ((characterIndex < charArr.Length) && (not (string(charArr.[characterIndex]) = " "))) do 
                                    currentChar <- string(charArr.[characterIndex])
                                    hashtag <- hashtag + string(currentChar)
                                    characterIndex <- characterIndex + 1
                                
                                if (hashtagsAndTweetsWithHashtags.ContainsKey(hashtag))
                                then 
                                    hashtagsAndTweetsWithHashtags.[hashtag] <- List.append hashtagsAndTweetsWithHashtags.[hashtag] [(JsonConvert.SerializeObject(tweetToDistribute))]
                                else
                                    hashtagsAndTweetsWithHashtags.Add(hashtag, [(JsonConvert.SerializeObject(tweetToDistribute))])
                                printfn "Identified a hashtag: #%s" hashtag
                            characterIndex <- characterIndex + 1

                        printfn "======There are %i users in user array=====" (users.Count)
                        printfn "Length of clientGuIdAndWebSocket %i" (clientGuIdAndWebsocket.Count)

                        // Send the tweet to all mentioned users
                        for user in mentionedUsers do 
                            if (userNameAndUser.ContainsKey(user))
                            then
                                let userProfile = userNameAndUser.[user]
                                if (userProfile.onlineStatus = 0)
                                then userProfile.twitterFeed.Add(tweetToDistribute)
                                else if (userProfile.onlineStatus = 1)
                                then
                                    let userGuId = userProfile.guId
                                    do! sendMessageWithClientId (userGuId) (TweetFromServer (JsonConvert.SerializeObject(tweetToDistribute))) |> Async.Ignore

                        for user in subs do
                            (*
                            // If they're offline then ....
                            if(not notifyUser.Status) then
                                notifyUser.Feeds.Add(tweet)
                            *)
                            // If the user is in mentionedUsers, they already received the tweet, so don't double send it.
                            if (not (List.contains user mentionedUsers))
                            then
                                let userProfile = userNameAndUser.[user]
                                if (userProfile.onlineStatus = 0)
                                then userProfile.twitterFeed.Add(tweetToDistribute)
                                else if (userProfile.onlineStatus = 1)
                                then
                                    printfn "They are online!"
                                    // TODO: Bug where first tweet sends find, but then after that original user stops getting htem and second user does
                                    // Then if second user tweets, no one gets it after his first one
                                    // Its probably something being overwritten.
                                    let userGuId = userProfile.guId
                                    printfn "Client ID for user is %s" userGuId
                                    // TODO: ?make sure its open
                                    do! sendMessageWithClientId (userGuId) (TweetFromServer (JsonConvert.SerializeObject(tweetToDistribute))) |> Async.Ignore
                        do! sendMessageWithClientId (clientGuId) (Success "Created Tweet") |> Async.Ignore
                | Subscribe userToSubTo ->
                    // Subscribe userToSubTo is going to add the sender's userName to userToSubTo's dictionary.
                    let senderUserName = guIdAndUserName.[clientGuId]
                    let sender = userNameAndUser.[senderUserName]
                    if (sender.onlineStatus = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "You cannot subscribe if you're logged out.")) |> Async.Ignore
                    else if (not (userNameAndGuId.ContainsKey(userToSubTo))) then
                        do! (sendMessageWithClientId (clientGuId) (Failure ("No such user @" + userToSubTo))) |> Async.Ignore
                    else 
                        // We don't need to check if our user is key in the dictionary yet since we automatically do that on register.
                        // We do need to check if our userToSubTo already contains sender's UserName though.
                        let mutable subs = userNamesAndTheirSubscribers.[userToSubTo]
                    
                        // If the user is already subscribed then send an error back (Failure)
                        if (subs.Contains(senderUserName))
                        then do! sendMessageWithClientId (clientGuId) (Failure ("Already subscribed to this user: " + userToSubTo)) |> Async.Ignore
                        else 
                            subs <- List.append subs [senderUserName]
                            userNamesAndTheirSubscribers.[userToSubTo] <- subs
                            do! sendMessageWithClientId (clientGuId) (Success ("You successfully subscribed to the user: " + userToSubTo)) |> Async.Ignore
                | Unsubscribe userToUnsubTo -> 
                    let senderUserName = guIdAndUserName.[clientGuId]
                    let sender = userNameAndUser.[senderUserName]
                    if (sender.onlineStatus = 0) then 
                        do! (sendMessageWithClientId (clientGuId) (Failure "You cannot unsubscribe if you're logged out.")) |> Async.Ignore
                    else if (not (userNameAndGuId.ContainsKey(userToUnsubTo)))
                    then do! (sendMessageWithClientId (clientGuId) (Failure ("No such user @" + userToUnsubTo))) |> Async.Ignore
                    else 
                        let mutable subs = userNamesAndTheirSubscribers.[userToUnsubTo]
                    
                        // If the userToUnsubTo contains the sender's user name, then we can successfully unsubscribe.
                        if (subs.Contains(senderUserName))
                        then
                            let mutable index = 0
                            let mutable locationOfIndex = -1
                            for sub in subs do
                                if (sub = senderUserName)
                                then 
                                    locationOfIndex <- index
                                index <- index + 1
                            subs <- removeAt locationOfIndex subs
                            userNamesAndTheirSubscribers.[userToUnsubTo] <- subs
                            do! sendMessageWithClientId (clientGuId) (Success ("You successfully subscribed to the user: " + userToUnsubTo)) |> Async.Ignore
                        // If it doesn't contain the user name, then the user is not subscribe to anyone by that nam
                        else
                            do! sendMessageWithClientId (clientGuId) (Failure ("Unable to unsubscribe since you are not subscribed to the user: " + userToUnsubTo)) |> Async.Ignore
                        
                return state + 1
            
        }
    }