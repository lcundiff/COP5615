module WebSharper.AspNetCore.Tests.WebSocketClient

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.AspNetCore.WebSocket
open WebSharper.AspNetCore.WebSocket.Client
open WebSocketServer
open System

module Server = WebSocketServer


[<JavaScript>]
let WebSocketTest (endpoint: WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer>) = 
    // Check on this
    let mutable server: WebSocketServer<Server.MessagesToClient, MessagesToServer> option = None

    let serverMessagesContainer = Elt.pre [] []
    let messagesHeader = Elt.div [] [
        Elt.h3[][text "Messages from server"]
    ]

    let tweetContainer = Elt.div[][]


    let retweet msg =
        server.Value.Post(Server.Tweet msg)

    let addTweetToFeed user msg = 
        let retweetBtn = 
            Elt.button [
            
            attr.``class`` "retweet-handler btn btn-info"
            on.click (fun _ _ ->
                retweet (msg))
            ] [
                text "Retweet!"
            ]
        let tweetContainerDiv = JS.Document.CreateElement("div")
        let tweetContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let reteetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(reteetButtonDom)
        tweetContentDiv.InnerHTML <- String.Format("<div style=\"display:flex;justify-content:center\"><div>{0}</div><div style=\"margin-left:10px\">{1}</div><div>",user,msg)
        let tweetContainerContent = tweetContainerDiv.AppendChild(tweetContentDiv)
        tweetContainerContent.AppendChild(retweetOption) |> ignore
        tweetContainer.Dom.AppendChild(tweetContainerContent)|>ignore

    // Connects to Server and Waits for Tweets...?
    let connectToServer = 
        async { 
            printfn "In here!"
            return! ConnectStateful endpoint <| fun server -> async {
                return 0, fun state msg -> async {
                    printfn "In in here here!"

                    match msg with 
                    | Message data ->
                        match data with 
                        | TweetFromServer response ->
                            if (response.Contains("")) then 
                                let responseMsg = response.JS.Split(",")
                                printfn "ResponseMSG %A" (responseMsg)
                                let tweet =  responseMsg.[0].JS.Split("\"").[3]
                                let user =  responseMsg.[1].JS.Split("\"").[3]
                                addTweetToFeed user tweet
                        | _ -> printfn "A message was sent back"
                    | _ -> printfn "FUCK"
                    return (state + 1)
                    
                }    
            }
        }

    // start the server
    connectToServer.AsPromise().Then(fun x -> server <- Some(x)) |> ignore


    let tweetMessage = Var.Create ""
    let userName = Var.Create ""


    let registerAccount (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                printfn "Trying to connect."
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Registering"
            server.Value.Post(Server.Register userName.Value)
        }
        |> Async.Start

    let postTweet (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Tweeting %s" tweetMessage.Value
            server.Value.Post(Server.Tweet tweetMessage.Value)
        }
        |> Async.Start

    let registerForm = 
        div [][
                Doc.Input [
                    attr.``class`` "form-control"]  userName
                button [ 
                    attr.``class`` "btn btn-primary"
                    on.click (registerAccount) ] [ text "Register" ]
            ]

    let createTweetForm = 
        div [] [
            Doc.Input [
                attr.``class`` "form-control"] tweetMessage
            button [
                attr.``class`` "btn btn-primary"
                on.click (postTweet) ] [text "Tweet"]
        ]

    div []
        [
            registerForm
            createTweetForm
            messagesHeader
            serverMessagesContainer
            tweetContainer
        ]

    

    (*let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.DoSomething input
            )
        div [] [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hr [] []
            h4 [attr.``class`` "text-muted"] [text "The server responded:"]
            div [attr.``class`` "jumbotron"] [h1 [] [textView vReversed]]
        ]
    *)
open WebSharper.AspNetCore.WebSocket

let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer> = 
    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)