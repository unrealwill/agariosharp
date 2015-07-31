# agariosharp
Agar.io c# interface

This is a c# port of https://github.com/pulviscriptor/agario-client

I tried to stay as minimal as possible.

Look at AgarioGUI for example of how to use.

Clone websocket-sharp master branch and build it 
Clone agariosharp
(The nuget version has a bug that prevents websockets to work on port 443 that agar.io sometimes use)
reference the websocket-sharp library you just built
Set AgarioGUI as startup project then build And run

Not all functionnalities are yet implemented but the game is functionnal.

It is for Mono (linux) as it uses Gtk. Although could easily be adapted to windows.


It connects to the official servers.

It is intended for botting purposes. 

But you can play :
mouse to naviguate
f to spectate
s to spawn
space to split
w to eject

I won't probably update the code frequently

Happy botting
