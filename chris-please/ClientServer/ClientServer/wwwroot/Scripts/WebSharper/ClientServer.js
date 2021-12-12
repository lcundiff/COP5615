(function(Global)
{
 "use strict";
 var WebSharper,AspNetCore,Tests,WebSocketClient,Website,SomeRecord,Client,ClientServer_JsonDecoder,ClientServer_JsonEncoder,ClientServer_Templates,UI,Doc,AttrProxy,Arrays,Concurrency,Unchecked,console,JavaScript,Promise,Utils,Var$1,WebSocket,Client$1,WithEncoding,JSON,Templating,Runtime,Server,ProviderBuilder,Handler,TemplateInstance,ClientSideJson,Provider,Client$2,Templates;
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
 Var$1=UI&&UI.Var$1;
 WebSocket=AspNetCore&&AspNetCore.WebSocket;
 Client$1=WebSocket&&WebSocket.Client;
 WithEncoding=Client$1&&Client$1.WithEncoding;
 JSON=Global.JSON;
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
 WebSocketClient.WebSocketTest$373$25=Global.id;
 WebSocketClient.WebSocketTest$368$26=Global.id;
 WebSocketClient.WebSocketTest$358$26=Global.id;
 WebSocketClient.WebSocketTest$344$26=Global.id;
 WebSocketClient.WebSocketTest$331$26=Global.id;
 WebSocketClient.WebSocketTest$316$26=Global.id;
 WebSocketClient.WebSocketTest$302$30=Global.id;
 WebSocketClient.WebSocketTest$84$22=function(retweet,msg)
 {
  return function()
  {
   return function()
   {
    return retweet(msg);
   };
  };
 };
 WebSocketClient.WebSocketTest$64$22=function(retweet,msg)
 {
  return function()
  {
   return function()
   {
    return retweet(msg);
   };
  };
 };
 WebSocketClient.WebSocketTest$44$22=function(retweet,msg)
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
  var server,serverMessagesContainer,messagesHeader,queryMessagesContainer,queryHeader,tweetContainer,queryContainer,queryRequested,userName,connectToServer,b,userToUnsubTo,tweetMessage,userToSubTo,query,registerBox,tweetBox,subscribeBox,unsubscribeBox,queryHashtagsBox;
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
  function addQueries(user,msg)
  {
   var retweetBtn,queryContainerDiv,queryContentDiv,retweetOption,$1,queryContainerContent;
   retweetBtn=Doc.Element("button",[AttrProxy.Create("class","retweet-handler btn btn-info"),AttrProxy.HandlerImpl("click",function()
   {
    return function()
    {
     return retweet(msg);
    };
   })],[Doc.TextNode("Retweet!")]);
   queryContainerDiv=self.document.createElement("div");
   queryContentDiv=self.document.createElement("div");
   retweetOption=self.document.createElement("div").appendChild(retweetBtn.elt);
   queryContentDiv.innerHTML=($1=[queryRequested,user,msg],"<div style=\"display:flex;justify-content:center\"><div>Query for "+(Arrays.get($1,0)==null?"":Global.String(Arrays.get($1,0)))+(": "+(Arrays.get($1,1)==null?"":Global.String(Arrays.get($1,1))))+("</div><div style=\"margin-left:10px\">"+(Arrays.get($1,2)==null?"":Global.String(Arrays.get($1,2))))+"</div><div>");
   queryContainerContent=queryContainerDiv.appendChild(queryContentDiv);
   queryContainerContent.appendChild(retweetOption);
   queryContainer.elt.appendChild(queryContainerContent);
  }
  function addMissedTweetToFeed(user,msg)
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
   tweetContentDiv.innerHTML=($1=[user,msg],"<div style=\"display:flex;justify-content:center\"><div>While you were away you missed: "+(Arrays.get($1,0)==null?"":Global.String(Arrays.get($1,0)))+("</div><div style=\"margin-left:10px\">"+(Arrays.get($1,1)==null?"":Global.String(Arrays.get($1,1))))+"</div><div>");
   tweetContainerContent=tweetContainerDiv.appendChild(tweetContentDiv);
   tweetContainerContent.appendChild(retweetOption);
   tweetContainer.elt.appendChild(tweetContainerContent);
  }
  function enableFunctionality()
  {
   var btn,tweetBox$1,subscribeBox$1,unsubscribeBox$1,queryBox,tweetBtn,subscribeBtn,unsubscribeBtn,queryBtn;
   btn=self.document.getElementById("register");
   btn.innerHTML="Account Registered";
   btn.setAttribute("disabled","true");
   userName.Set("Welcome to Twitter, @"+userName.Get());
   self.document.getElementById("register-form").setAttribute("disabled","true");
   self.document.getElementById("logout").removeAttribute("disabled");
   tweetBox$1=self.document.getElementById("tweetbox");
   subscribeBox$1=self.document.getElementById("subscribebox");
   unsubscribeBox$1=self.document.getElementById("unsubscribebox");
   queryBox=self.document.getElementById("querybox");
   tweetBtn=self.document.getElementById("tweetbtn");
   subscribeBtn=self.document.getElementById("subscribebtn");
   unsubscribeBtn=self.document.getElementById("unsubscribebtn");
   queryBtn=self.document.getElementById("querybtn");
   tweetBox$1.removeAttribute("disabled");
   subscribeBox$1.removeAttribute("disabled");
   unsubscribeBox$1.removeAttribute("disabled");
   queryBox.removeAttribute("disabled");
   tweetBtn.removeAttribute("disabled");
   subscribeBtn.removeAttribute("disabled");
   unsubscribeBtn.removeAttribute("disabled");
   queryBtn.removeAttribute("disabled");
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
     tweetMessage.Set("");
     return Concurrency.Zero();
    }));
   })),null);
  }
  function subscribeToUser(x,y)
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
       return $1("Subscribing to "+Utils.toSafe($2));
      };
     }(function(s)
     {
      console.log(s);
     }))(userToSubTo.Get()));
     server.$0.Post({
      $:2,
      $0:userToSubTo.Get()
     });
     userToSubTo.Set("");
     return Concurrency.Zero();
    }));
   })),null);
  }
  function unsubscribeToUser(x,y)
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
       return $1("Unsubscribing to "+Utils.toSafe($2));
      };
     }(function(s)
     {
      console.log(s);
     }))(userToUnsubTo.Get()));
     server.$0.Post({
      $:3,
      $0:userToUnsubTo.Get()
     });
     userToUnsubTo.Set("");
     return Concurrency.Zero();
    }));
   })),null);
  }
  function queryFromServer(x,y)
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
       return $1("Querying #"+Utils.toSafe($2));
      };
     }(function(s)
     {
      console.log(s);
     }))(query.Get()));
     server.$0.Post({
      $:4,
      $0:query.Get()
     });
     queryRequested=query.Get();
     query.Set("");
     return Concurrency.Zero();
    }));
   })),null);
  }
  function loginToServer(x,y)
  {
   var b$1;
   return Concurrency.Start((b$1=null,Concurrency.Delay(function()
   {
    var loginbtn,logoutbtn,tweetBox$1,subscribeBox$1,unsubscribeBox$1,queryBox,tweetBtn,subscribeBtn,unsubscribeBtn,queryBtn;
    server.$0.Post({
     $:5
    });
    loginbtn=self.document.getElementById("login");
    logoutbtn=self.document.getElementById("logout");
    tweetBox$1=self.document.getElementById("tweetbox");
    subscribeBox$1=self.document.getElementById("subscribebox");
    unsubscribeBox$1=self.document.getElementById("unsubscribebox");
    queryBox=self.document.getElementById("querybox");
    tweetBtn=self.document.getElementById("tweetbtn");
    subscribeBtn=self.document.getElementById("subscribebtn");
    unsubscribeBtn=self.document.getElementById("unsubscribebtn");
    queryBtn=self.document.getElementById("querybtn");
    tweetBox$1.removeAttribute("disabled");
    subscribeBox$1.removeAttribute("disabled");
    unsubscribeBox$1.removeAttribute("disabled");
    queryBox.removeAttribute("disabled");
    tweetBtn.removeAttribute("disabled");
    subscribeBtn.removeAttribute("disabled");
    unsubscribeBtn.removeAttribute("disabled");
    queryBtn.removeAttribute("disabled");
    loginbtn.removeAttribute("disabled");
    logoutbtn.removeAttribute("disabled");
    loginbtn.setAttribute("disabled","true");
    return Concurrency.Zero();
   })),null);
  }
  function logoutOfServer(x,y)
  {
   var b$1;
   return Concurrency.Start((b$1=null,Concurrency.Delay(function()
   {
    var loginbtn,logoutbtn,tweetBox$1,subscribeBox$1,unsubscribeBox$1,queryBox,tweetBtn,subscribeBtn,unsubscribeBtn,queryBtn;
    server.$0.Post({
     $:6
    });
    loginbtn=self.document.getElementById("login");
    logoutbtn=self.document.getElementById("logout");
    tweetBox$1=self.document.getElementById("tweetbox");
    subscribeBox$1=self.document.getElementById("subscribebox");
    unsubscribeBox$1=self.document.getElementById("unsubscribebox");
    queryBox=self.document.getElementById("querybox");
    tweetBtn=self.document.getElementById("tweetbtn");
    subscribeBtn=self.document.getElementById("subscribebtn");
    unsubscribeBtn=self.document.getElementById("unsubscribebtn");
    queryBtn=self.document.getElementById("querybtn");
    tweetBox$1.setAttribute("disabled","true");
    subscribeBox$1.setAttribute("disabled","true");
    unsubscribeBox$1.setAttribute("disabled","true");
    queryBox.setAttribute("disabled","true");
    tweetBtn.setAttribute("disabled","true");
    subscribeBtn.setAttribute("disabled","true");
    unsubscribeBtn.setAttribute("disabled","true");
    queryBtn.setAttribute("disabled","true");
    loginbtn.removeAttribute("disabled");
    logoutbtn.removeAttribute("disabled");
    logoutbtn.setAttribute("disabled","true");
    return Concurrency.Zero();
   })),null);
  }
  server=null;
  serverMessagesContainer=Doc.Element("pre",[],[]);
  messagesHeader=Doc.Element("div",[],[Doc.Element("h3",[],[Doc.TextNode("Your Twitter Feed")])]);
  queryMessagesContainer=Doc.Element("pre",[],[]);
  queryHeader=Doc.Element("div",[],[Doc.Element("h3",[],[Doc.TextNode("Your Queried Requests")])]);
  tweetContainer=Doc.Element("div",[],[]);
  queryContainer=Doc.Element("div",[],[]);
  queryRequested="";
  userName=Var$1.Create$1("");
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
        var data,responseMsg,tweet,responseMsg$1,tweet$1,hashtags,responseMsg$2,tweet$2,fail;
        (function($1)
        {
         return $1("In in here here!");
        }(function(s)
        {
         console.log(s);
        }));
        return Concurrency.Combine(msg.$==0?(data=msg.$0,data.$==1?(responseMsg=data.$0.split(","),((function($1)
        {
         return function($2)
         {
          return $1("ResponseMSG "+Utils.printArray(Utils.prettyPrint,$2));
         };
        }(function(s)
        {
         console.log(s);
        }))(responseMsg),tweet=Arrays.get(Arrays.get(responseMsg,0).split("\""),3),addTweetToFeed(Arrays.get(Arrays.get(responseMsg,1).split("\""),3),tweet),Concurrency.Zero())):data.$==6?(responseMsg$1=data.$0.split(","),((function($1)
        {
         return function($2)
         {
          return $1("ResponseMSG "+Utils.printArray(Utils.prettyPrint,$2));
         };
        }(function(s)
        {
         console.log(s);
        }))(responseMsg$1),tweet$1=Arrays.get(Arrays.get(responseMsg$1,0).split("\""),3),addMissedTweetToFeed(Arrays.get(Arrays.get(responseMsg$1,1).split("\""),3),tweet$1),Concurrency.Zero())):data.$==5?(hashtags=data.$0,(responseMsg$2=hashtags.split(","),((function($1)
        {
         return function($2)
         {
          return $1("ResponseMSG "+Utils.prettyPrint($2));
         };
        }(function(s)
        {
         console.log(s);
        }))(hashtags),tweet$2=Arrays.get(Arrays.get(responseMsg$2,0).split("\""),3),addQueries(Arrays.get(Arrays.get(responseMsg$2,1).split("\""),3),tweet$2),Concurrency.Zero()))):data.$==3?((function($1)
        {
         return function($2)
         {
          return $1("Success! "+Utils.toSafe($2));
         };
        }(function(s)
        {
         console.log(s);
        }))(data.$0),Concurrency.Zero()):data.$==4?(fail=data.$0,((function($1)
        {
         return function($2)
         {
          return $1("Failure! "+Utils.toSafe($2));
         };
        }(function(s)
        {
         console.log(s);
        }))(fail),Global.alert(fail),Concurrency.Zero())):data.$==2?(enableFunctionality(),Concurrency.Zero()):(function($1)
        {
         return $1("A message was sent back");
        }(function(s)
        {
         console.log(s);
        }),Concurrency.Zero())):(function($1)
        {
         return $1("Failure");
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
  userToUnsubTo=Var$1.Create$1("");
  tweetMessage=Var$1.Create$1("");
  userToSubTo=Var$1.Create$1("");
  query=Var$1.Create$1("");
  registerBox=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control"),AttrProxy.Create("id","register-form")],userName),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","register"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return registerAccount($1,$2);
   };
  })],[Doc.TextNode("Register")])]);
  tweetBox=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control"),AttrProxy.Create("id","tweetbox"),AttrProxy.Create("disabled","true")],tweetMessage),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","tweetbtn"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return postTweet($1,$2);
   };
  })],[Doc.TextNode("Tweet")])]);
  subscribeBox=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control"),AttrProxy.Create("id","subscribebox"),AttrProxy.Create("disabled","true")],userToSubTo),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","subscribebtn"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return subscribeToUser($1,$2);
   };
  })],[Doc.TextNode("Subscribe")])]);
  unsubscribeBox=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control"),AttrProxy.Create("id","unsubscribebox"),AttrProxy.Create("disabled","true")],userToUnsubTo),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","unsubscribebtn"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return unsubscribeToUser($1,$2);
   };
  })],[Doc.TextNode("Unsubscribe")])]);
  queryHashtagsBox=Doc.Element("div",[],[Doc.Input([AttrProxy.Create("class","form-control"),AttrProxy.Create("id","querybox"),AttrProxy.Create("disabled","true")],query),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","querybtn"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return queryFromServer($1,$2);
   };
  })],[Doc.TextNode("Query")])]);
  return Doc.Element("div",[],[registerBox,Doc.Element("div",[],[Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","login"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return loginToServer($1,$2);
   };
  })],[Doc.TextNode("Login")]),Doc.Element("button",[AttrProxy.Create("class","btn btn-primary"),AttrProxy.Create("id","logout"),AttrProxy.Create("disabled","true"),AttrProxy.HandlerImpl("click",function($1)
  {
   return function($2)
   {
    return logoutOfServer($1,$2);
   };
  })],[Doc.TextNode("Logout")])]),queryHashtagsBox,subscribeBox,unsubscribeBox,tweetBox,messagesHeader,serverMessagesContainer,tweetContainer,queryHeader,queryMessagesContainer,queryContainer]);
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
  return ClientServer_JsonDecoder._v?ClientServer_JsonDecoder._v:ClientServer_JsonDecoder._v=(Provider.DecodeUnion(void 0,"$",[[0,[["$0","con",Provider.Id(),0]]],[1,[["$0","tweet",Provider.Id(),0]]],[2,[]],[3,[["$0","succ",Provider.Id(),0]]],[4,[["$0","fail",Provider.Id(),0]]],[5,[["$0","tweet",Provider.Id(),0]]],[6,[["$0","tweet",Provider.Id(),0]]]]))();
 };
 ClientServer_JsonEncoder.j=function()
 {
  return ClientServer_JsonEncoder._v?ClientServer_JsonEncoder._v:ClientServer_JsonEncoder._v=(Provider.EncodeUnion(void 0,"$",[[0,[["$0","tweet",Provider.Id(),0]]],[1,[["$0","account",Provider.Id(),0]]],[2,[["$0","userToSubTo",Provider.Id(),0]]],[3,[["$0","userToUnsubTo",Provider.Id(),0]]],[4,[["$0","query",Provider.Id(),0]]],[5,[]],[6,[]]]))();
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
