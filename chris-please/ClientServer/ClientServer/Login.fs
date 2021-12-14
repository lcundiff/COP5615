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
let WebSocketLogin (endpoint: WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer>) = 
    // Check on this
    let mutable server: WebSocketServer<Server.MessagesToClient, MessagesToServer> option = None

    let serverMessagesContainer = Elt.pre [] []
    let messagesHeader = Elt.div [] [
        Elt.h3[][text "Your Twitter Feed"]
    ]

    let queryHeader = Elt.div[] [
        Elt.h3[][text "Your Queried Requests"]
    ]

    let tweetContainer = Elt.div[][]
    let queryContainer = Elt.div[][]
    let mutable queryRequested = ""


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
        let retweetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(retweetButtonDom)
        tweetContentDiv.InnerHTML <- String.Format("<div style=\"display:flex;justify-content:center\"><div>{0}</div><div style=\"margin-left:10px\">{1}</div><div>",user,msg)
        let tweetContainerContent = tweetContainerDiv.AppendChild(tweetContentDiv)
        tweetContainerContent.AppendChild(retweetOption) |> ignore
        tweetContainer.Dom.AppendChild(tweetContainerContent)|>ignore

    let addQueries user msg = 
        let retweetBtn = 
            Elt.button [
            
            attr.``class`` "retweet-handler btn btn-info"
            on.click (fun _ _ ->
                retweet (msg))
            ] [
                text "Retweet!"
            ]
        let queryContainerDiv = JS.Document.CreateElement("div")
        let queryContentDiv = JS.Document.CreateElement("div")
        let retweetButtonDiv = JS.Document.CreateElement("div")
        let retweetButtonDom =  retweetBtn.Dom
        let retweetOption =  retweetButtonDiv.AppendChild(retweetButtonDom)
        queryContentDiv.InnerHTML <- String.Format("<div style=\"display:flex;justify-content:center\"><div>Query for {0}: {1}</div><div style=\"margin-left:10px\">{2}</div><div>",queryRequested,user,msg)
        let queryContainerContent = queryContainerDiv.AppendChild(queryContentDiv)
        queryContainerContent.AppendChild(retweetOption) |> ignore
        queryContainer.Dom.AppendChild(queryContainerContent)|>ignore



    let enableFunctionality () = 
        let btn = JS.Document.GetElementById("register")
        btn.InnerHTML <- "Account Registered"
        btn.SetAttribute("disabled", "true")
        userName.Value <- ("Welcome to Twitter, @" + userName.Value)
        let form = JS.Document.GetElementById("register-form")
        form.SetAttribute("disabled", "true")
        let loginoutbtn = JS.Document.GetElementById("logout")
        loginoutbtn.RemoveAttribute("disabled")

        // enable everything on register
        let tweetBox = JS.Document.GetElementById("tweetbox")
        let subscribeBox = JS.Document.GetElementById("subscribebox")
        let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
        let queryBox = JS.Document.GetElementById("querybox")
        let tweetBtn = JS.Document.GetElementById("tweetbtn")
        let subscribeBtn = JS.Document.GetElementById("subscribebtn")
        let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
        let queryBtn = JS.Document.GetElementById("querybtn")
        tweetBox.RemoveAttribute("disabled")
        subscribeBox.RemoveAttribute("disabled")
        unsubscribeBox.RemoveAttribute("disabled")
        queryBox.RemoveAttribute("disabled")
        tweetBtn.RemoveAttribute("disabled")
        subscribeBtn.RemoveAttribute("disabled")
        unsubscribeBtn.RemoveAttribute("disabled")
        queryBtn.RemoveAttribute("disabled")

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
                            let responseMsg = response.JS.Split(",")
                            printfn "ResponseMSG %A" (responseMsg)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addTweetToFeed user tweet
                        | MissedTweet response ->
                            let responseMsg = response.JS.Split(",")
                            printfn "ResponseMSG %A" (responseMsg)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addMissedTweetToFeed user tweet
                        | QueryFromServer hashtags ->
                            let responseMsg = hashtags.JS.Split(",")
                            printfn "ResponseMSG %A" (hashtags)
                            let tweet =  responseMsg.[0].JS.Split("\"").[3]
                            let user =  responseMsg.[1].JS.Split("\"").[3]
                            addQueries user tweet
                        | Success succ ->
                            printfn "Success! %s" succ
                        | Failure fail ->
                            printfn "Failure! %s" fail
                            JS.Alert(fail)
                        | RegisteredFromServer ->
                            enableFunctionality()
                        | _ -> printfn "A message was sent back"
                        
                    | _ -> printfn "Failure"
                    return (state + 1)
                    
                }    
            }
        }

    // start the server
    connectToServer.AsPromise().Then(fun x -> server <- Some(x)) |> ignore


    let userToUnsubTo = Var.Create ""
    let tweetMessage = Var.Create ""
    let userToSubTo = Var.Create ""
    let query = Var.Create ""


    let registerAccount (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            if (server = None) then 
                printfn "Trying to connect."
                connectToServer.AsPromise().Then(fun x -> server <-Some(x)) |> ignore   
            printfn "Registering"
            server.Value.Post(Server.Register userName.Value)
            // userName.Value <- ""
        }
        |> Async.Start


    let loginToServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Login)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.RemoveAttribute("disabled")
            subscribeBox.RemoveAttribute("disabled")
            unsubscribeBox.RemoveAttribute("disabled")
            queryBox.RemoveAttribute("disabled")
            tweetBtn.RemoveAttribute("disabled")
            subscribeBtn.RemoveAttribute("disabled")
            unsubscribeBtn.RemoveAttribute("disabled")
            queryBtn.RemoveAttribute("disabled")


            loginbtn.RemoveAttribute("disabled")
            logoutbtn.RemoveAttribute("disabled")
            loginbtn.SetAttribute("disabled", "true")
        }

        |> Async.Start

    let logoutOfServer (x: Dom.Element) (y: Dom.MouseEvent) = 
        async {
            server.Value.Post(Server.Logout)
            let loginbtn = JS.Document.GetElementById("login")
            let logoutbtn = JS.Document.GetElementById("logout")
            let tweetBox = JS.Document.GetElementById("tweetbox")
            let subscribeBox = JS.Document.GetElementById("subscribebox")
            let unsubscribeBox = JS.Document.GetElementById("unsubscribebox")
            let queryBox = JS.Document.GetElementById("querybox")
            let tweetBtn = JS.Document.GetElementById("tweetbtn")
            let subscribeBtn = JS.Document.GetElementById("subscribebtn")
            let unsubscribeBtn = JS.Document.GetElementById("unsubscribebtn")
            let queryBtn = JS.Document.GetElementById("querybtn")
            tweetBox.SetAttribute("disabled", "true")
            subscribeBox.SetAttribute("disabled", "true")
            unsubscribeBox.SetAttribute("disabled", "true")
            queryBox.SetAttribute("disabled", "true")
            tweetBtn.SetAttribute("disabled", "true")
            subscribeBtn.SetAttribute("disabled", "true")
            unsubscribeBtn.SetAttribute("disabled", "true")
            queryBtn.SetAttribute("disabled", "true")


            loginbtn.RemoveAttribute("disabled")
            logoutbtn.RemoveAttribute("disabled")
            logoutbtn.SetAttribute("disabled", "true")
        }

        |> Async.Start

    let registerBox = 
        div [][
                Doc.Input [
                    attr.``class`` "form-control"
                    attr.``id`` "register-form"]  userName
                button [ 
                    attr.``class`` "tweetBox__tweetButton" 
                    attr.``id`` "register"
                    on.click (registerAccount) ] [ text "Register" ]
            ]

    let logButtons = 
        div [] [
           button [
           
                attr.``class`` "tweetBox__tweetButton"
                attr.``id`` "login"
                attr.``disabled`` "true"
                on.click (loginToServer) ] [text "Login"]
           button [
               attr.``class`` "tweetBox__tweetButton"
               attr.``id`` "logout"
               attr.``disabled`` "true"
               on.click (logoutOfServer) ] [text "Logout"]
        ]

    div []
        [
            registerBox
            logButtons
            queryHashtagsBox
            subscribeBox
            unsubscribeBox
            tweetBox
            messagesHeader
            serverMessagesContainer
            tweetContainer
            queryHeader
            queryContainer
        ]

open WebSharper.AspNetCore.WebSocket

let MyEndPoint (url: string) : WebSharper.AspNetCore.WebSocket.WebSocketEndpoint<Server.MessagesToClient, Server.MessagesToServer> = 
    WebSocketEndpoint.Create(url, "/ws", JsonEncoding.Readable)