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

let engine = spawn system "engine" <| fun (mailbox: Actor<obj>) ->
    let rec loop() = actor {
        let! msg = mailbox.Receive() 
        let sender = mailbox.Sender()
        match msg with
            | Tweeting(tweet) -> // user tweeted (triggered by simulator)
                printfn "Sending a message to appropriate people."
            | Subscribing(subscribedTo) -> // user clicked subscribe on a user (triggered by simulator)
            | AddTweet(tweet) -> // server telling us someone we subscribed to tweeted
            | AddFollower(subscriber) ->
            | NewsFeed -> // Views tweets of users they subscribed to (simulator)
            | Success -> 
                printfn "server message succeeded!"

        return! loop() 
    }
    loop()