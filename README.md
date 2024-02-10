# Open Doors in Space

Lethal Company mod that lets crewmates open the door while the ship is in orbit. You won't believe what happens!

## Installing

### Thunderstore / r2modman

Requires r2modman. Follow the instructions to get started: <https://lethal.wiki/installation/installing-r2modman>.

1. Go to <https://thunderstore.io/c/lethal-company/p/netux/OpenDoorsInSpace/>
1. Press on Install with Mod Manager
1. (Re)Start the game

### Manual

Requires BepInEx. You can get the LethalCompany-specific variant from here: <https://thunderstore.io/c/lethal-company/p/BepInEx/BepInExPack/>

1. Download the latest `OpenDoorsInSpace.dll` release from [the releases tab](https://github.com/netux/LethalCompany-OpenDoorsInSpace/releases)
1. Copy `OpenDoorsInSpace.dll` into `(Game folder)/BepInEx/plugins`
1. (Re)Start the game

## Building

Requires .NET 8

1. Clone repository
1. Copy all DLLs from `(Game folder)/Lethal Company_Data/Managed` into this project's `lib/`
1. Run `dotnet build`
1. Output DLL should be in `bin/(Debug or Release)/netstandard2.1/OpenDoorsInSpace.dll`
