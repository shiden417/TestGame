# Phase 1 Playtest Checklist

Target: `docs/GAME_DESIGN.md` Phase 1 - Feel

## Before Play

- [ ] `git pull`
- [ ] Open `Assets/Scenes/PrototypeScene.unity`
- [ ] Press Play
- [ ] Connect a gamepad before or during play

## Gamepad

- [ ] Left Stick moves the player relative to the camera.
- [ ] Right Stick rotates the third-person camera.
- [ ] RT/R2 performs the normal attack.
- [ ] RB/R1 performs a directional dodge.
- [ ] A/Cross performs jump.
- [ ] LT/L2 toggles lock-on.
- [ ] R3 switches the lock-on target when multiple targets are available.

## Movement Feel

- [ ] Movement responds immediately to stick input.
- [ ] The player accelerates and decelerates smoothly.
- [ ] The player faces the movement direction when not locked on.
- [ ] The player faces the locked target while locked on.
- [ ] The player does not become stuck inside the arena boundaries.

## Camera

- [ ] The camera follows the player smoothly.
- [ ] The camera can orbit horizontally and vertically.
- [ ] Camera pitch remains within a playable range.
- [ ] The camera does not collapse into the player when following.
- [ ] Lock-on smoothly biases the camera toward the target.

## Attack

- [ ] An energy-blade placeholder appears during the attack.
- [ ] Normal attacks can chain into a 3-step combo by timing repeated inputs.
- [ ] The attack can hit nearby training units.
- [ ] A training unit disappears after receiving enough damage.

## Dodge

- [ ] Dodge direction follows the current movement input.
- [ ] With no movement input, dodge moves backward relative to the camera.
- [ ] Dodge temporarily reports an invulnerable state for future combat systems.
- [ ] Dodge has a short cooldown.
- [ ] Starting a dodge cancels the current attack.

## Lock-On

- [ ] LT/L2 selects the nearest useful target within the lock-on range.
- [ ] A visible marker identifies the current target.
- [ ] LT/L2 again releases the target.
- [ ] R3 cycles to another available target.
- [ ] A defeated target can no longer remain as the active target.

## Acceptance Criteria

Phase 1 is accepted when the core loop can be tested entirely with a gamepad:

Move -> Attack -> Dodge -> Lock-On -> Re-engage

The prototype does not need finished art, full enemy AI, skills, ultimate attacks, or boss behavior yet.

Those belong to later phases.
