# Sunrise

Sunrise is an anti-cheat plugin for SCP:SL servers, designed to prevent various forms of cheating and exploits.
The project is in its early stages, with more features planned for the future.

# Features

## Server-Side Backtrack

**Targeting**: Silent aim, Spinbot, Fake rotation.

**Effect**: Silent aim doesn't aim, spinbot fires in random directions, fake rotation prevents aiming.

The default latency compensation system (Backtrack) in SCP:SL allows shooters to send their own and the target's position and rotation at the moment of firing.
The server does minimal position validation and no rotation validation, opening the door for exploits like silent aim and shooting through walls.

Sunrise replaces this default system with a more secure server-side solution, effectively eliminating the possibility of such exploits.

The system works by recording a precise history of each player's position and rotation. When a player fires,
the server no longer trusts client-provided data. Instead, it locates the closest valid entry in the player’s actual historical data,
ensuring that only legitimate shots, based on real past positions and rotations, are processed.

## Ideas (not implemented yet)

- **Door interaction validation**: Patch the exploit that allows cheaters to open doors without directly looking at them.
- **Pickup validation**: Patch the exploit that allows cheaters to pick up items through walls.
- **Pickup spoofing**: Send fake data about pickups far away from players to clutter ESP cheats.

## Community

Currently, the game is overwhelmed with cheaters. Instead of keeping the solutions private, I decided to make it open-source because in my opinion
cheaters are supposed to suffer everywhere, not only on servers that can make their own anti-cheat. If you support this idea,
feel free to contribute to the project by implementing more features and researching new ways to prevent common exploits.



