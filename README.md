
# Friday Night Funkin PS4

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

## Build instructions
- Install the Visual Studio
- Install the OpenOrbis SDK 
- Setup the OpenOrbis SDK Environment variable "OO_PS4_TOOLCHAIN" 
- Clone this repo with those commands:
```sh
git clone https://github.com/marcussacana/FridayNightFunkin
git submodule init
git submodule update --init --recursive
```
- Download the game assets here: https://www.mediafire.com/file/v7pysvi0he51lhl/assets.zip
- Place the assets (without extract) at "assets\misc" 
- Open the "Developer Command Prompt for VS 2022"
- Run in this project root directory the command
```
build-windows release
```

## Screenshots
![pic](https://github.com/marcussacana/FridayNightFunkin/assets/10576957/bb31fa58-ff8b-43c5-b780-ae6c9dd9d149)  
![pic2](https://github.com/marcussacana/FridayNightFunkin/assets/10576957/cd22a728-5f59-45d5-87ff-3d274d731822)


