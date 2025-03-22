# Sunrise

Sunrise is an anti-cheat plugin for SCP:SL servers, designed to prevent various forms of cheating and exploits.  
The project is in its early stages, with more features to come in the future.

# Features

## Server-Side Backtrack

**Effect**: Silent aim doesn't aim, spinbot fires in random directions, fake rotation prevents aiming.  
Cheaters are forced to use aim lock, which is much easier to notice.

The default latency compensation system (Backtrack) in SCP:SL allows shooters to send their own and the target's position and rotation at the moment of firing.  
The server does minimal position validation and no rotation validation, opening the door for exploits like silent aim and shooting through walls.

We replace this default system with a more secure server-side solution, effectively eliminating the possibility of such exploits.

The system works by recording a precise history of each player's position and rotation. When a player fires,  
the server no longer trusts client-provided data. Instead, it locates the closest valid entry in the player’s actual historical data,  
ensuring that only legitimate shots, based on real past positions and rotations, are processed.

## Ideas (not implemented yet)

- **Door interaction validation**: Patch the exploit that allows cheaters to open doors without directly looking at them.
- **Pickup validation**: Patch the exploit that allows cheaters to pick up items through walls.
- **Pickup spoofing**: Send fake data about pickups far away from players to clutter ESP cheats.

# Community

Currently, the game is overwhelmed with cheaters. Instead of keeping the solutions private, we decided to make it open-source because we believe  
cheaters are supposed to suffer everywhere, not only on servers that can make their own anti-cheat. If you support this idea,  
feel free to contribute to the project by implementing more features and researching new ways to prevent common exploits.

The project is fully self-contained; all you need to do to build it yourself is clone the repo. We will be actively reviewing pull requests,  
but it’s better to discuss changes in the Discord server before starting to work on them.

If you are willing to contribute, or just like the project, feel free to join the [Discord server](https://discord.gg/9nAaRVNCq3).

# Installation

The only dependency is [EXILED](https://github.com/ExMod-Team/EXILED). To install the plugin, simply download the  
latest release from the [releases page](https://github.com/Banalny-Banan/Sunrise/releases) and put it in the `Plugins` folder.

# License

Sunrise is licensed under the MIT License with the Commons Clause condition.

- You are free to use, modify, and include this project (or parts of it) in private or commercial projects.
- However, you **may not sell or distribute Sunrise or derivative works as a standalone commercial product**.
- This restriction is in place to prevent the development of paid anti-cheat services, which we consider unethical.

See [LICENSE](./LICENSE) for full legal terms.