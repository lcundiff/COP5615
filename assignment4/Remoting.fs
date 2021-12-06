namespace assignment4

open WebSharper
open System.Security.Cryptography
open System.Text

module Server =

    [<Rpc>]
    let Tweet input =
        let R (tweetMsg: string) = System.String(Array.rev(tweetMsg.ToCharArray()))
        async {
            return R input
        }
