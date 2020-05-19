This folder contains sources for the GXPEngine and the Algorithms starting code.
To start on the assignment, open the assignment/algorithms.sln with Visual Studio.
Put all your code in the SOLUTION folder.

FAQ:
Q1. I don't have visual studio, what now?
A1. Any C# code editor will do, with a high enough SDK. Just make sure to add all the .cs files from all subfolders.

Q2. I get a warning about Toolsversion 15.0 and graph?.Generate() doesn't work
A2. Try creating an empty C# project from scratch in the IDE of your choice

Q3. The assets cannot be found, I get an exception during load.
A3. Make sure the working directly is set correctly. 
	Assuming your working directory in bin/Debug by default, use ..\..\ (the assignment folder containing the .sln file)
	
Q4: Why are you inheriting from Canvas instead of EasyDraw
A4: EasyDraw is just a wrapper around graphics.etc, feel free to change :Canvas into :EasyDraw if you like that better.

