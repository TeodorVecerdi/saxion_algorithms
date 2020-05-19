# GXPEngine #

This is the GXPEngine. It's used for learning about game development at Saxion University of Applied Sciences.

### How do I get set up? ###

* Clone the latest version of the master branch
* Open the "GXPEngine.sln" file to open the project using Visual Studio Community (or whichever IDE you prefer)

### Possible improvements (actively) being worked on ###

* Making destroying objects less prone to memory leaks
* Replace FMOD with something open source/open licensed
* Make full screen crashes function better

### I have some ideas for new features! Who do I talk to? ###

* Contact any of the Game Programming teachers (check blackboard for an up-to-date list)
* You can implement it yourself (following the coding conventions of course) and create a pull request

### Improvements made, to be included in next release (+tested?):

* Splitscreen functionality (See Camera and Window) (PBO06)
* Cache sounds - no memory leaks (PBO06)
* Viewport resolution independent from "game logic" resolution (game.width and game.height) (PBO06)
* Adding diagnostic tools to Game (report number of colliders, objects in hierarchy, etc) (PBO06)
* Option to keep textures in cache (to prevent too much file loading and stuttering) (PBO06)
* Trying to read non-existing sound file gives exception (PBO06)
* Prevented Canvas Destroy Exception (PBO06)
* Adding EasyDraw class, very much inspired by the Processing API (PBO06)
* Enabling pixel art - see the Game constructor (PBO06)
* Adding TiledMapParser - note that this now requires fiddling with .exe.config files on Mac/Linux... (PBO06)
* Adding Settings.cs (loading parameters at runtime)
* Adding HierarchyManager (allowing delayed Destroy, after the update loop)