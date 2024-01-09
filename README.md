# Open Doors in Space

Lethal Company mod that lets crewmates open the door while the ship is in orbit. You won't believe what happens!

## Installing

Requires BepInEx.

1. Copy `OpenDoorsInSpace.dll` into `(Game folder)/BepInEx/plugins`
1. (Re)Start the game

## Building

Requires .NET 8

1. Clone repository
1. Copy all DLLs from `(Game folder)Lethal Company_Data/Managed` into this project's `lib/`
1. Run `dotnet build`
1. Output DLL should be in `bin/(Debug or Release)/netstandard2.1/OpenDoorsInSpace.dll`