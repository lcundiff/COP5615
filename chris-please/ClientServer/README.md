# F# Twitter

## TASKS COMPLETED 
Designed a JSON based API that  represents all messages and their replies (including errors)
Re-wrote engine using WebSharper to implement the WebSocket interface
Re-wrote client to use WebSockets.
(We also implemented websharper with a twitter UI template) 

## Requirements for running the project
.NET 3.1 SDK - https://dotnet.microsoft.com/download/dotnet/6.0
.NET Runtime 5 - https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-5.0.12-windows-x64-installer
(net 5 SDK wasn't working for some reason, see debugging for more info)
also downloaded desktop runtime - https://dotnet.microsoft.com/download/dotnet/thank-you/runtime-desktop-6.0.0-windows-x64-installer
Visual Studio 2015+ (2022 preview recommended)
Installing WebSharper vsix through Developer PowerShell -> VSIXInstaller.exe location_of_WebSharper.4.7.3.424.vsix

## How we set up the project
We based our project from the websharper client-server template: https://github.com/dotnet-websharper/core#helloworld
We followed the instructions on how to set up the template from: https://developers.websharper.com/docs/v4.x/fs/install
Also used this page for setup, although its the same info as above link: https://websharper.com/downloads#with-vs

**We installed the websharper-web template for f#**
The following tutorials helped setup the websharper functions: https://developers.websharper.com/docs/v3.x/fs/running-snippets
We then used a Twitter HTML/CSS template found from this github: https://github.com/somanath-goudar/html-css-projects/tree/master/twitter-clone


## How to run (Make sure to install .NET 3.1 since that is version we used)
**dotnet build**
if no errors from building, 
**dotnet run**
Go to localhost:5000

## Debugging
### Cannot build error
this link shows helpful command for debugging: https://github.com/dotnet-websharper/core/issues/1044
The command is: **dotnet build -v d**
### Incorrect runtime error 
This could also be helpful for installing .NET tool versions from command line: https://stackoverflow.com/questions/68112732/the-framework-microsoft-netcore-app-version-5-was-not-found-while-microsoft
### .NET version  not found 
This github issue shows this issue can be fixed by setting target framework to .NET 3.1
