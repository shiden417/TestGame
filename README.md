# TestGame

A Unity 6 third-person action prototype focused on:

- Futuristic SF world
- Japanese-inspired visual design
- High-speed character control
- Gamepad-first combat
- Large enemy groups
- Dodge and perfect-dodge counter gameplay
- Lock-on
- Stylish melee combat
- Elite and boss encounters
- A complete first vertical slice

## Source of Truth

Game design:

`docs/GAME_DESIGN.md`

AI implementation rules:

`AGENTS.md`

Implementation status:

`docs/IMPLEMENTATION_STATUS.md`

Complete playtest checklist:

`docs/VERTICAL_SLICE_PLAYTEST_CHECKLIST.md`

## Local Playtest

1. Run `git pull`.
2. Open Unity 6000.6.2f1.
3. Open `Assets/Scenes/PrototypeScene.unity`.
4. Press Play.
5. Connect/use a gamepad.

The prototype builds the battlefield, player, enemies, camera, HUD, visual effects, and audio feedback at runtime. Manual scene-object placement is not required.

## Gamepad Controls

- Left Stick: Move
- Right Stick: Camera
- RT / R2: Normal attack
- RB / R1: Dodge
- A / Cross: Jump
- X / Square: Skill 1
- Y / Triangle: Skill 2
- LB / L1: Ultimate
- LT / L2: Lock-on
- R3: Target switch
- Start/Menu: reserved for pause integration

Keyboard/mouse bindings remain available as development fallback.

## Development Rule

Maintain the identity:

**Future SF + Japanese-inspired design + high-speed action + crowd combat**

Before meaningful changes, read `docs/GAME_DESIGN.md`.

When a change conflicts with the specification, update the specification deliberately before changing the implementation.
