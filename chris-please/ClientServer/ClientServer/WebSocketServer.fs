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

type MessagesToClient = 
    | Connection of con : string
    | TweetFromServer of tweet : string
    | RegisteredFromServer of register : string


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

    // Used to send message to the client based on the cleint id.
    let sendMessageWithClientId clientId msg  = 
        let clientStream = clientGuIdAndWebsocket.[clientId]
        printfn "ClientId %s" clientId 
        writeMessage clientStream msg |> Async.Ignore

    // Connects To Client via Websocket
    fun handleClientMessages -> async {
        let ip = handleClientMessages.Connection.Context.Connection.RemoteIpAddress.ToString()
        // Gets guid
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
                    let user = createUser account clientGuId
                    userNameAndUser.Add(account, user) |> ignore
                    guIdAndUserName.Add(clientGuId, account) |> ignore
                    userNameAndGuId.Add(account, clientGuId) |> ignore
                    users.Add(user) |> ignore
                    do! (writeMessage handleClientMessages (RegisteredFromServer "Registered."))
                | Tweet msgTweet ->
                    // TODO: Should have a check to make sure user is registed && "logged-in" (not disconnected)
                    let (clientName:string) = guIdAndUserName.[clientGuId]
                    let sender = userNameAndUser.[clientName]
                    let tweetToDistribute = createTweet clientName msgTweet
                    // TODO: Increment totalTweets
                    // TODO: We need to determine who the tweet should get sent to, but for now we will send it to everyone
                    
                    printfn "======There are %i users in user array=====" (users.Count)
                    printfn "Length of clientGuIdAndWebSocket %i" (clientGuIdAndWebsocket.Count)
                    for user in users do
                        (*
                        // If they're offline then ....
                        if(not notifyUser.Status) then
                            notifyUser.Feeds.Add(tweet)
                        *)
                        if (user.onlineStatus = 0)
                        then user.twitterFeed.Add(tweetToDistribute)
                        else if (user.onlineStatus = 1)
                        then
                            printfn "They are online!"
                            // TODO: Bug where first tweet sends find, but then after that original user stops getting htem and second user does
                            // Then if second user tweets, no one gets it after his first one
                            // Its probably something being overwritten.
                            let userGuId = user.guId
                            printfn "Client ID for user is %s" userGuId
                            // TODO: ?make sure its open
                            do! sendMessageWithClientId (userGuId) (TweetFromServer(JsonConvert.SerializeObject(tweetToDistribute))) |> Async.Ignore
                    do! sendMessageWithClientId (clientGuId) (TweetFromServer "Created Tweet") |> Async.Ignore
                return state + 1
            | Error exn -> return state
            | Close -> return state
        }
    }