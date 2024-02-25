# Friday Night Funkin PS4

⚠️ Warning the MakeFile script is broken in linux short of⚠️

This is the repository for Friday Night Funkin PS4 Port, a game originally made for Ludum Dare 47 "Stuck In a Loop".

Play the Ludum Dare prototype here: https://ninja-muffin24.itch.io/friday-night-funkin
Play the Newgrounds one here: https://www.newgrounds.com/portal/view/770371
Support the project on the itch.io page: https://ninja-muffin24.itch.io/funkin

## Credits / shoutouts

- [ninjamuffin99 (me!)](https://twitter.com/ninja_muffin99) - Programmer
- [PhantomArcade3K](https://twitter.com/phantomarcade3k) and [Evilsk8r](https://twitter.com/evilsk8r) - Art
- [Kawaisprite](https://twitter.com/kawaisprite) - Musician
- [Marcussacana](https://github.com/marcussacana) - PS4 Port

This game was made with love to Newgrounds and its community. Extra love to Tom Fulp.

## Installation
- Download the PKG [Clicking here](https://github.com/marcussacana/FridayNightFunkin/releases/)
- Install as any other game
- IF you don't know how to install FPKG you can watch this video [here](https://www.youtube.com/watch?v=ljw-EizKTas) by modded warfare

## Build instructions
- Install the Visual Studio 2022 or rider
- [Enable .Net Framework 4.5 Support in the VS2022](https://stackoverflow.com/questions/70022194/open-net-framework-4-5-project-in-vs-2022-is-there-any-workaround)
- Install the OpenOrbis SDK 
- Setup the OpenOrbis SDK Environment variable "OO_PS4_TOOLCHAIN" you Can set it up on windows by fellowing this guide by Spector [here](https://www.youtube.com/watch?v=pqzsva6OjuE)

- Clone this repo with those commands:
```sh
git clone https://github.com/marcussacana/FridayNightFunkin
cd FridayNightFunkin
git submodule init
git submodule update --init --recursive
```
- Download the game assets here: https://www.mediafire.com/file/v7pysvi0he51lhl/assets.zip
- Place the assets (without extracting) at "assets\misc"
- Open the OrbisGL.sln project and  fnd the bulid text on top it say bulid and click that then you will see bulid solution click that
- Open the "Developer Command Prompt for VS 2022" 
- Run in this project root directory the command in the Developer Command Prompt or Developer powershell works for me
```
build-windows release
```

## Debugging
Is possible to debug the game directly on PS4 or in your PC (but without audio),
for debug in PS4 you will need use the Jetbrains Rider IDE or Visual Studio 2022 and setup the remote debug, for debug in your PC you will need run the GLTest project.  
The problem is that GLTest it require the OpenGL dlls that aren't included, you can copy from your pc browser the files `libEGL.dll` and `libGLESv2.dll` it can be find in `C:\Users\<Current-user>\AppData\Local\Microsoft\Edge\` or `C:\program(x86)\Microsoft\Edge\you will see the version you runing for exmple 121.?` place in the debug output directory.

## Screenshots
![pic](https://github.com/marcussacana/FridayNightFunkin/assets/10576957/bb31fa58-ff8b-43c5-b780-ae6c9dd9d149)  
![pic2](https://github.com/marcussacana/FridayNightFunkin/assets/10576957/cd22a728-5f59-45d5-87ff-3d274d731822)
