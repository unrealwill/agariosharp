# agariosharp
Agar.io c# interface

This is a c# port of https://github.com/pulviscriptor/agario-client <br/>

I tried to stay as minimal as possible. <br/>

Look at AgarioGUI for example of how to use. <br/>

Clone agariosharp <br/>
Clone websocket-sharp master branch and build it : https://github.com/sta/websocket-sharp <br/>
(The nuget version has a bug that prevents websockets to work on port 443 that agar.io sometimes use) <br/>
Reference the websocket-sharp library you just built <br/>
Set AgarioGUI as startup project then build And run <br/>

Not all functionnalities are yet implemented (scores and leaderboard are missing for the moment) but the game is functionnal.

It is for Mono (linux) as it uses Gtk<br/>
It is not thread safe but all processing is handled in the main (gui) thread. 
So you must access AgarioClient from the main (gui) thread and you shouldn't encounter threading issues.

It can easily be adapted to windows by using Application instead of Gtk.Application <br/>


It connects to the official servers.

It is intended for botting purposes. 

But you can play : <br/>
mouse to naviguate <br/>
f to spectate <br/>
s to spawn <br/>
space to split <br/>
w to eject <br/>

I won't probably update the code frequently <br/>

Happy botting <br/>

License
MIT
