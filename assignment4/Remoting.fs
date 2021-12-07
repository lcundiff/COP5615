namespace assignment4

open WebSharper
open System.Security.Cryptography
open System.Text

WebSharper.Remoting.EndPoint <- "http://your-client-server-application's-url"

module Server = 

    [<Rpc>]
    let Tweet input =
        let R (tweetMsg: string) = System.String(Array.rev(tweetMsg.ToCharArray()))
        async {
            return R input
        }
