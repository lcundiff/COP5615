(function(Global)
{
 "use strict";
 var assignment4,Client,ClientFunctions,Message,SC$1,assignment4_Templates,WebSharper,Concurrency,Remoting,AjaxRemotingProvider,UI,Var$1,Templating,Runtime,Server,ProviderBuilder,Handler,TemplateInstance,console,Operators,IntelliFactory,Runtime$1,Utils,List,Seq,Collections,Dictionary,Arrays,Random,Obj,Client$1,Templates;
 assignment4=Global.assignment4=Global.assignment4||{};
 Client=assignment4.Client=assignment4.Client||{};
 ClientFunctions=assignment4.ClientFunctions=assignment4.ClientFunctions||{};
 Message=ClientFunctions.Message=ClientFunctions.Message||{};
 SC$1=Global.StartupCode$assignment4$Client=Global.StartupCode$assignment4$Client||{};
 assignment4_Templates=Global.assignment4_Templates=Global.assignment4_Templates||{};
 WebSharper=Global.WebSharper;
 Concurrency=WebSharper&&WebSharper.Concurrency;
 Remoting=WebSharper&&WebSharper.Remoting;
 AjaxRemotingProvider=Remoting&&Remoting.AjaxRemotingProvider;
 UI=WebSharper&&WebSharper.UI;
 Var$1=UI&&UI.Var$1;
 Templating=UI&&UI.Templating;
 Runtime=Templating&&Templating.Runtime;
 Server=Runtime&&Runtime.Server;
 ProviderBuilder=Server&&Server.ProviderBuilder;
 Handler=Server&&Server.Handler;
 TemplateInstance=Server&&Server.TemplateInstance;
 console=Global.console;
 Operators=WebSharper&&WebSharper.Operators;
 IntelliFactory=Global.IntelliFactory;
 Runtime$1=IntelliFactory&&IntelliFactory.Runtime;
 Utils=WebSharper&&WebSharper.Utils;
 List=WebSharper&&WebSharper.List;
 Seq=WebSharper&&WebSharper.Seq;
 Collections=WebSharper&&WebSharper.Collections;
 Dictionary=Collections&&Collections.Dictionary;
 Arrays=WebSharper&&WebSharper.Arrays;
 Random=WebSharper&&WebSharper.Random;
 Obj=WebSharper&&WebSharper.Obj;
 Client$1=UI&&UI.Client;
 Templates=Client$1&&Client$1.Templates;
 Client.Main$26$21=function(rvReversed)
 {
  return function(e)
  {
   var b;
   Concurrency.StartImmediate((b=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("assignment4:assignment4.Server.Tweet:1689443245",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
    {
     rvReversed.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  };
 };
 Client.Main=function()
 {
  var rvReversed,b,R,_this,t,p,i;
  rvReversed=Var$1.Create$1("");
  return(b=(R=rvReversed.get_View(),(_this=(t=new ProviderBuilder.New$1(),(t.h.push(Handler.EventQ2(t.k,"ontweet",function()
  {
   return t.i;
  },function(e)
  {
   var b$1;
   Concurrency.StartImmediate((b$1=null,Concurrency.Delay(function()
   {
    return Concurrency.Bind((new AjaxRemotingProvider.New()).Async("assignment4:assignment4.Server.Tweet:1689443245",[e.Vars.Hole("texttoreverse").$1.Get()]),function(a)
    {
     rvReversed.Set(a);
     return Concurrency.Zero();
    });
   })),null);
  })),t)),(_this.h.push({
   $:2,
   $0:"reversed",
   $1:R
  }),_this))),(p=Handler.CompleteHoles(b.k,b.h,[["texttoreverse",0]]),(i=new TemplateInstance.New(p[1],assignment4_Templates.mainform(p[0])),b.i=i,i))).get_Doc();
 };
 Message.Simulate={
  $:13
 };
 Message.Success={
  $:12
 };
 Message.SubscribedToTweets={
  $:11
 };
 ClientFunctions.simulator=function()
 {
  var i,$1;
  (function($2)
  {
   return $2("Welcome to Twitter Simulator, how many accounts would you like to create?");
  }(function(s)
  {
   console.log(s);
  }));
  ClientFunctions.set_numOfAccounts(Operators.toInt(1));
  ClientFunctions.registerAccounts();
  (((Runtime$1.Curried3(function($2,$3,$4)
  {
   return $2(Global.String($3)+" zipfSubscribers: "+Utils.printList(Utils.prettyPrint,$4));
  }))(function(s)
  {
   console.log(s);
  }))(ClientFunctions.zipfSubscribers().get_Length()))(ClientFunctions.zipfSubscribers());
  for(i=0,$1=ClientFunctions.numOfAccounts()-1;i<=$1;i++)Global.String(i);
 };
 ClientFunctions.registerAccounts=function()
 {
  var i,$1;
  for(i=0,$1=ClientFunctions.numOfAccounts()-1;i<=$1;i++)(function(i$1)
  {
   var accountList;
   ClientFunctions.registerAccount(Global.String(i));
   accountList=List.ofSeq(Seq.delay(function()
   {
    return Seq.map(function()
    {
     return Global.String(i$1);
    },Operators.range(0,(ClientFunctions.numOfAccounts()/(i$1+1)>>0)-1));
   }));
   return ClientFunctions.set_zipfSubscribers(List.append(ClientFunctions.zipfSubscribers(),accountList));
  }(i));
  (function($2)
  {
   return $2("adding clients");
  }(function(s)
  {
   console.log(s);
  }));
 };
 ClientFunctions.registerAccount=function(accountName)
 {
  ClientFunctions.client(accountName);
  ClientFunctions.users().Add(accountName,accountName);
  ClientFunctions.set_clientIds(List.append(ClientFunctions.clientIds(),List.ofArray([accountName])));
  ClientFunctions.connectionStatus().Add(accountName,true);
 };
 ClientFunctions.clientIds=function()
 {
  SC$1.$cctor();
  return SC$1.clientIds;
 };
 ClientFunctions.set_clientIds=function($1)
 {
  SC$1.$cctor();
  SC$1.clientIds=$1;
 };
 ClientFunctions.client=function(id)
 {
  var connected,myFollowers,divisor,randomUserId,subsList,rndWord,liveData,msgType,increasedTweets,tweetProbability,randomMax,randomNumber;
  liveData=new Dictionary.New$5();
  liveData.Add("myTweets",List.T.Empty);
  liveData.Add("subscribedTo",List.T.Empty);
  liveData.Add("hashTag",List.T.Empty);
  liveData.Add("mentions",List.T.Empty);
  liveData.Add("mySubs",List.T.Empty);
  connected=true;
  myFollowers=List.T.Empty;
  msgType="Simulate";
  if(msgType==="Simulate")
   {
    divisor=ClientFunctions.numOfAccounts()/20;
    increasedTweets=myFollowers.get_Length()/divisor;
    tweetProbability=10+Operators.toInt(increasedTweets);
    randomMax=60+Operators.toInt(increasedTweets);
    randomNumber=ClientFunctions.random().Next(0,randomMax);
    if(!connected)
     randomNumber<=randomMax/5>>0?((function($1)
     {
      return function($2)
      {
       return $1(Utils.toSafe($2)+" is re-connecting.");
      };
     }(function(s)
     {
      console.log(s);
     }))(id),connected=true):void 0;
    else
     {
      randomUserId=Global.String(ClientFunctions.random().Next$1(ClientFunctions.numOfAccounts()));
      while(!ClientFunctions.users().ContainsKey(randomUserId))
       randomUserId=Global.String(ClientFunctions.random().Next$1(ClientFunctions.numOfAccounts()));
      if(randomNumber<=tweetProbability)
       liveData.set_Item("myTweets",ClientFunctions.tweet(id,randomUserId,liveData,randomNumber));
      else
       randomNumber<=tweetProbability+10?liveData.set_Item("mySubs",ClientFunctions.subscribe(id,randomUserId,liveData)):randomNumber<=tweetProbability+20?ClientFunctions.unsubscribe(id,liveData):randomNumber<=tweetProbability+30?((function($1)
       {
        return function($2)
        {
         return $1(Utils.toSafe($2)+" is requesting subscribed tweets.");
        };
       }(function(s)
       {
        console.log(s);
       }))(id),subsList=List.ofArray([id]),subsList=List.append(liveData.get_Item("mySubs"),subsList)):randomNumber<=tweetProbability+39?(rndWord=ClientFunctions.createRndWord(),(((Runtime$1.Curried3(function($1,$2,$3)
       {
        return $1(Utils.toSafe($2)+" is requesting the hashtag: "+Utils.toSafe($3)+".");
       }))(function(s)
       {
        console.log(s);
       }))(id))("#"+rndWord)):randomNumber===tweetProbability+40?((function($1)
       {
        return function($2)
        {
         return $1(Utils.toSafe($2)+" is disconnecting.");
        };
       }(function(s)
       {
        console.log(s);
       }))(id),connected=false):randomNumber<=tweetProbability+50?(((Runtime$1.Curried3(function($1,$2,$3)
       {
        return $1(Utils.toSafe($2)+" is requesting mentions of "+Utils.toSafe($3)+".");
       }))(function(s)
       {
        console.log(s);
       }))(id))(randomUserId):void 0;
     }
   }
  else
   msgType==="AddFollower"?function($1)
   {
    return $1("User: followers now (after adding):");
   }(function(s)
   {
    console.log(s);
   }):msgType==="RemoveFollower"?function($1)
   {
    return $1("followers now (after removing):");
   }(function(s)
   {
    console.log(s);
   }):msgType==="ReceiveTweets"?function($1)
   {
    return $1("received tweets");
   }(function(s)
   {
    console.log(s);
   }):msgType==="AddTweet"?function($1)
   {
    return $1("adding tweet:");
   }(function(s)
   {
    console.log(s);
   }):msgType==="Success"?function($1)
   {
    return $1("server message succeeded!");
   }(function(s)
   {
    console.log(s);
   }):function($1)
   {
    return $1("ERROR: client recieved unrecognized message");
   }(function(s)
   {
    console.log(s);
   });
 };
 ClientFunctions.unsubscribe=function(id,liveData)
 {
  var randomSubIndex,randomSubUserId;
  if(liveData.get_Item("mySubs").get_Length()>1)
   {
    randomSubIndex=ClientFunctions.random().Next(1,liveData.get_Item("mySubs").get_Length());
    randomSubUserId=liveData.get_Item("mySubs").get_Item(randomSubIndex);
    (((Runtime$1.Curried3(function($1,$2,$3)
    {
     return $1(Utils.toSafe($2)+" is unsubscribing from "+Utils.toSafe($3)+".");
    }))(function(s)
    {
     console.log(s);
    }))(id))(randomSubUserId);
    ClientFunctions._lock();
    ClientFunctions.removeFromList(randomSubUserId,liveData.get_Item("mySubs"));
   }
 };
 ClientFunctions.subscribe=function(id,rndUserId,liveData)
 {
  var rndNonSubUserId,randomSubscriberIndex,index;
  rndNonSubUserId=rndUserId;
  (((Runtime$1.Curried3(function($1,$2,$3)
  {
   return $1(Utils.toSafe($2)+": in the subscribe method: mySubs "+Utils.printList(Utils.prettyPrint,$3));
  }))(function(s)
  {
   console.log(s);
  }))(id))(liveData.get_Item("mySubs"));
  ClientFunctions._lock();
  if(ClientFunctions.zipfSubscribers().get_Length()>0)
   {
    randomSubscriberIndex=ClientFunctions.random().Next$1(ClientFunctions.zipfSubscribers().get_Length());
    index=0;
    while(List.contains(ClientFunctions.zipfSubscribers().get_Item(randomSubscriberIndex),liveData.get_Item("mySubs"))&&index<100)
     {
      randomSubscriberIndex=ClientFunctions.random().Next$1(ClientFunctions.zipfSubscribers().get_Length());
      index=index+1;
     }
    rndNonSubUserId=ClientFunctions.zipfSubscribers().get_Item(randomSubscriberIndex);
    ClientFunctions.set_zipfSubscribers(ClientFunctions.removeAt(randomSubscriberIndex,ClientFunctions.zipfSubscribers()));
   }
  else
   null;
  (((Runtime$1.Curried3(function($1,$2,$3)
  {
   return $1(Utils.toSafe($2)+" is subscribing to "+Utils.toSafe($3)+".");
  }))(function(s)
  {
   console.log(s);
  }))(id))(rndNonSubUserId);
  return List.append(liveData.get_Item("mySubs"),List.ofArray([rndNonSubUserId]));
 };
 ClientFunctions.tweet=function(id,rndUserId,liveData,rndNum)
 {
  var hashtag,tweet,rndUserId2;
  hashtag="#"+ClientFunctions.createRndWord();
  ClientFunctions.createRndWord();
  tweet="User: "+id+" tweeted @"+rndUserId;
  tweet=tweet+" "+hashtag;
  return rndNum<=2?((function($1)
  {
   return function($2)
   {
    return $1(Utils.toSafe($2)+" is retweeting.");
   };
  }(function(s)
  {
   console.log(s);
  }))(id),rndUserId2=Global.String(ClientFunctions.random().Next$1(ClientFunctions.numOfAccounts())),List.append(liveData.get_Item("myTweets"),List.ofArray([tweet]))):((function($1)
  {
   return function($2)
   {
    return $1(Utils.toSafe($2)+" is tweeting.");
   };
  }(function(s)
  {
   console.log(s);
  }))(id),List.append(liveData.get_Item("myTweets"),List.ofArray([tweet])));
 };
 ClientFunctions.removeAt=function(index,list)
 {
  function p(i,a)
  {
   return i!==index;
  }
  return List.map(function(t)
  {
   return t[1];
  },List.filter(function($1)
  {
   return p($1[0],$1[1]);
  },List.indexed(list)));
 };
 ClientFunctions.removeFromList=function(sub,subs)
 {
  return List.map(function(t)
  {
   return t[1];
  },List.filter(function(t)
  {
   return t[0];
  },List.mapi(function(i,el)
  {
   return[el!==sub,el];
  },subs)));
 };
 ClientFunctions.createRndWord=function()
 {
  var rndCharCount,chars,sz;
  rndCharCount=ClientFunctions.random().Next(0,10);
  chars=Arrays.concat([Arrays.ofSeq(Operators.charRange("a","z")),Arrays.ofSeq(Operators.charRange("A","Z")),Arrays.ofSeq(Operators.charRange("0","9"))]);
  sz=chars.length;
  return Arrays.init(rndCharCount,function()
  {
   return Arrays.get(chars,ClientFunctions.random().Next$1(sz));
  }).join("");
 };
 ClientFunctions.zipfSubscribers=function()
 {
  SC$1.$cctor();
  return SC$1.zipfSubscribers;
 };
 ClientFunctions.set_zipfSubscribers=function($1)
 {
  SC$1.$cctor();
  SC$1.zipfSubscribers=$1;
 };
 ClientFunctions._lock=function()
 {
  SC$1.$cctor();
  return SC$1._lock;
 };
 ClientFunctions.connectionStatus=function()
 {
  SC$1.$cctor();
  return SC$1.connectionStatus;
 };
 ClientFunctions.users=function()
 {
  SC$1.$cctor();
  return SC$1.users;
 };
 ClientFunctions.numOfAccounts=function()
 {
  SC$1.$cctor();
  return SC$1.numOfAccounts;
 };
 ClientFunctions.set_numOfAccounts=function($1)
 {
  SC$1.$cctor();
  SC$1.numOfAccounts=$1;
 };
 ClientFunctions.random=function()
 {
  SC$1.$cctor();
  return SC$1.random;
 };
 ClientFunctions.serverIp=function()
 {
  SC$1.$cctor();
  return SC$1.serverIp;
 };
 ClientFunctions.clientIp=function()
 {
  SC$1.$cctor();
  return SC$1.clientIp;
 };
 SC$1.$cctor=function()
 {
  SC$1.$cctor=Global.ignore;
  SC$1.clientIp="localhost";
  SC$1.serverIp="localhost";
  SC$1.random=new Random.New();
  SC$1.numOfAccounts=0;
  SC$1.users=new Dictionary.New$5();
  SC$1.connectionStatus=new Dictionary.New$5();
  SC$1._lock=new Obj.New();
  SC$1.zipfSubscribers=List.T.Empty;
  SC$1.clientIds=List.T.Empty;
 };
 assignment4_Templates.mainform=function(h)
 {
  Templates.LoadLocalTemplates("main");
  return h?Templates.NamedTemplate("main",{
   $:1,
   $0:"mainform"
  },h):void 0;
 };
}(self));
