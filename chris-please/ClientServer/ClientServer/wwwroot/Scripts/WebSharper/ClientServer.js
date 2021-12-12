(function(Global)
{
 "use strict";
 var WebSharper,AspNetCore,Tests,WebSocketClient,Website,SomeRecord,Client,ClientServer_JsonDecoder,ClientServer_JsonEncoder,ClientServer_Templates,UI,Doc,AttrProxy,Arrays,Concurrency,Unchecked,console,JavaScript,Promise,Utils,WebSocket,Client$1,WithEncoding,JSON,Var$1,Templating,Runtime,Server,ProviderBuilder,Handler,TemplateInstance,ClientSideJson,Provider,Client$2,Templates;
 WebSharper=Global.WebSharper=Global.WebSharper||{};
 AspNetCore=WebSharper.AspNetCore=WebSharper.AspNetCore||{};
 Tests=AspNetCore.Tests=AspNetCore.Tests||{};
 WebSocketClient=Tests.WebSocketClient=Tests.WebSocketClient||{};
 Website=Tests.Website=Tests.Website||{};
 SomeRecord=Website.SomeRecord=Website.SomeRecord||{};
 Client=Website.Client=Website.Client||{};
 ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder=Global.ClientServer_JsonDecoder||{};
 ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder=Global.ClientServer_JsonEncoder||{};
 ClientServer_Templates=Global.ClientServer_Templates=Global.ClientServer_Templates||{};
 UI=WebSharper&&WebSharper.UI;
 Doc=UI&&UI.Doc;
 AttrProxy=UI&&UI.AttrProxy;
 Arrays=WebSharper&&WebSharper.Arrays;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Unchecked=WebSharper&&WebSharper.Unchecked;
 console=Global.console;
 JavaScript=WebSharper&&WebSharper.JavaScript;
 Promise=JavaScript&&JavaScript.Promise;
 Utils=WebSharper&&WebSharper.Utils;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client$1=WebSocket&&WebSocket.Client;
 WithEncoding=Client$1&&Client$1.WithEncoding;
 JSON=Global.JSON;
 Var$1=UI&&UI.Var$1;
 Templating=UI&&UI.Templating;
 Runtime=Templating&&Templating.Runtime;
 Server=Runtime&&Runtime.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 ClientSideJson=WebSharper&&WebSharper.ClientSideJson;
 Provider=ClientSideJson&&ClientSideJson.Provider;
 Client$2=UI&&UI.Client;
 Templates=Client$2&&Client$2.Templates;
 WebSocketClient.WebSocketTest$120$26=Global.id;
 WebSocketClient.WebSocketTest$111$30=Global.id;
 WebSocketClient.WebSocketTest$37$22=function(retweet,msg)
 {
  return function()
  {
   return function()
   {
    return retweet(msg);
   };
  };
 };
 WebSocketClient.WebSocketTest=function(endpoint)
 {
  var server,serverMessagesContainer,messagesHeader,tweetContainer,connectToServer,b,tweetMessage,userName;
  function retweet(msg)
  {
   server.$0.Post({
    $:0,
    $0:msg
   });
  }
  function addTweetToFeed(user,msg)
  {
   var retweetBtn,tweetContainerDiv,tweetContentDiv,retweetOption,$1,tweetContainerContent;
   retweetBtn=Doc.Element("button",[AttrProxy.Create("class","retweet-handler btn btn-info"),AttrProxy.HandlerImpl("click",function()
   {
    return function()
    {
     return retweet(msg);
    };
   })],[Doc.TextNode("Retweet!")]);
   tweetContainerDiv=self.document.createElement("div");
   tweetContentDiv=self.document.createElement("div");
   retweetOption=self.document.createElement("div").appendChild(retweetBtn.elt);
   tweetContentDiv.innerHTML=($1=[user,msg],"<div style=\"display:flex;justify-content:center\"><div>"+(Arrays.get($1,0)==null?"":Global.String(Arrays.get($1,0)))+("</div><div style=\"margin-left:10px\">"+(Arrays.get($1,1)==null?"":Global.String(Arrays.get($1,1))))+"</div><div>");
   tweetContainerContent=tweetContainerDiv.appendChild(tweetContentDiv);
   tweetContainerContent.appendChild(retweetOption);
   tweetContainer.elt.appendChild(tweetContainerContent);
  }
  function registerAccount(x,y)
  {
   var b$1;
   return Concurrency.Start((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Combine(Unchecked.Equals(server,null)?(function($1)
    {
     return $1("Trying to connect.");
    }(function(s)
    {
     console.log(s);
    }),Promise.OfAsync(connectToServer).then(function(x$1)
    {
     server={
      $:1,
      $0:x$1
     };
    }),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
    {
     (function($1)
     {
      return $1("Registering");
     }(function(s)
     {
      console.log(s);
     }));
     server.$0.Post({
      $:1,
      $0:userName.Get()
     });
     return Concurrency.Zero();
    }));
   })),null);
  }
  function postTweet(x,y)
  {
   var b$1;
   return Concurrency.Start((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Combine(Unchecked.Equals(server,null)?(Promise.OfAsync(connectToServer).then(function(x$1)
    {
     server={
      $:1,
      $0:x$1
     };
    }),Concurrency.Zero()):Concurrency.Zero(),Concurrency.Delay(function()
    {
     ((function($1)
     {
      return function($2)
      {
       return $1("Tweeting "+Utils.toSafe($2));
      };
     }(function(s)
     {
      console.log(s);
     }))(tweetMessage.Get()));
     server.$0.Post({
      $:0,
      $0:tweetMessage.Get()
     });
     return Concurrency.Zero();
    }));
   })),null);
  }
  server=null;
  serverMessagesContainer=Doc.Element("pre",[],[]);
  messagesHeader=Doc.Element("div",[],[Doc.Element("h3",[],[Doc.TextNode("Messages from server")])]);
  tweetContainer=Doc.Element("div",[],[]);
  connectToServer=(b=null,Concurrency.Delay(function()
  {
   (function($1)
   {
    return $1("In here!");
   }(function(s)
   {
    console.log(s);
   }));
   return WithEncoding.ConnectStateful(function(a)
   {
    return JSON.stringify((ClientServer_JsonEncoder.j())(a));
   },function(a)
   {
    return(ClientServer_JsonDecoder.j())(JSON.parse(a));
   },endpoint,function()
   {
    var b$1;
    b$1=null;
    return Concurrency.Delay(function()
    {
     return Concurrency.Return([0,function(state)
     {
      return function(msg)
      {
       var b$2;
       b$2=null;
       return Concurrency.Delay(function()
       {
        var data,response,responseMsg,tweet;
        (function($1)
        {
         return $1("In in here here!");
        }(function(s)
        {
         console.log(s);
        }));
        return Concurrency.Combine(msg.$==0?(data=msg.$0,data.$==1?(response=data.$0,response.indexOf("")!=-1?(responseMsg=response.split(","),((function($1)
        {
         return function($2)
         {
          return $1("ResponseMSG "+Utils.printArray(Utils.prettyPrint,$2));
         };
        }(function(s)
        {
         console.log(s);
        }))(responseMsg),tweet=Arrays.get(Arrays.get(responseMsg,0).split("\""),3),addTweetToFeed(Arrays.get(Arrays.get(responseMsg,1).split("\""),3),tweet),Concurrency.Zero())):Concurrency.Zero()):(function($1)
        {
         return $1("A message was sent back");
        }(function(s)
        {
         console.log(s);
        }),Concurrency.Zero())):(function($1)
        {
         return $1("FUCK");
        }(function(s)
        {
         console.log(s);
        }),Concurrency.Zero()),Concurrency.Delay(function()
        {
         return Concurrency.Return(state+1);
        }));
       });
      };
     }]);
    });
   });
  }));
  Promise.OfAsync(connectToServer).then(function(x)
  {
   server={
    $:1,
    $0:x
   };
  });
  tweetMessage=Var$1.Create$1("");
  userName=Var$1.Create$1("");
  return Doc.Element("div",[],[Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],userName),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return registerAccount($1,$2);
   };
  })],[Doc.TextNode("Register")])]),Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control")],tweetMessage),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return postTweet($1,$2);
   };
  })],[Doc.TextNode("Tweet")])]),messagesHeader,serverMessagesContainer,tweetContainer]);
 };
 SomeRecord.New=function(Name)
 {
  return{
   Name:Name
  };
 };
 Client.Main=function(aboutPageLink,wsep)
 {
  var b,_this,W,_this$1,p,i;
  return(b=(_this=(W=WebSocketClient.WebSocketTest(wsep),(_this$1=new ProviderBuilder.New$1(),(_this$1.h.push({
   $:0,
   $0:"websockettest",
   $1:W
  }),_this$1))),(_this.h.push({
   $:1,
   $0:"aboutpagelink",
   $1:aboutPageLink
  }),_this)),(p=Handler.CompleteHoles(b.k,b.h,[]),(i=new TemplateInstance.New(p[1],ClientServer_Templates.body(p[0])),b.i=i,i))).get_Doc();
 };
 ClientServer_JsonDecoder.j=function()
 {
  return ClientServer_JsonDecoder._v?ClientServer_JsonDecoder._v:ClientServer_JsonDecoder._v=(Provider.DecodeUnion(void 0,"$",[[0,[["$0","con",Provider.Id(),0]]],[1,[["$0","tweet",Provider.Id(),0]]],[2,[["$0","register",Provider.Id(),0]]]]))();
 };
 ClientServer_JsonEncoder.j=function()
 {
  return ClientServer_JsonEncoder._v?ClientServer_JsonEncoder._v:ClientServer_JsonEncoder._v=(Provider.EncodeUnion(void 0,"$",[[0,[["$0","tweet",Provider.Id(),0]]],[1,[["$0","account",Provider.Id(),0]]]]))();
 };
 ClientServer_Templates.body=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",{
   $:1,
   $0:"body"
  },h):void 0;
 };
}(self));
