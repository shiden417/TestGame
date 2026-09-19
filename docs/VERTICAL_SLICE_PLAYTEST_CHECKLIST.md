# TestGame Vertical Slice Playtest Checklist

## Goal

Verify that the first complete implementation of the game-design direction is playable from mission start to mission clear/defeat.

## Setup

- [ ] Run `git pull`.
- [ ] Open the project in Unity 6000.6.2f1.
- [ ] Open `Assets/Scenes/PrototypeScene.unity`.
- [ ] Connect a gamepad.
- [ ] Press Play.

## Phase 1 - Feel

- [ ] Left Stick moves relative to the camera.
- [ ] Right Stick rotates the camera.
- [ ] Movement accelerates and decelerates smoothly.
- [ ] Player rotates toward movement when not locked on.
- [ ] Camera remains behind/around the player.
- [ ] A/Cross jumps.
- [ ] RB/R1 dodges.
- [ ] LT/L2 toggles lock-on.
- [ ] R3 switches targets.

## Phase 2 - Combat

- [ ] RT/R2 starts a normal attack.
- [ ] Repeated attack inputs produce a three-step combo.
- [ ] The energy-blade visual appears during attacks.
- [ ] Nearby enemies take damage.
- [ ] Enemy hit reaction is visible.
- [ ] Enemy knockback is visible.
- [ ] X/Square uses Skill 1.
- [ ] Y/Triangle uses Skill 2.
- [ ] LB/L1 uses the Ultimate after enough gauge is accumulated.
- [ ] Player HP decreases when hit.

## Phase 3 - Crowd Battle

- [ ] Wave 1 spawns a large enemy group.
- [ ] Wave 1 contains basic and ranged variants.
- [ ] Wave 2 increases enemy count.
- [ ] Wave 2 contains heavy and ranged variants.
- [ ] Multiple enemies can approach simultaneously.
- [ ] Enemy attack scheduling prevents all nearby enemies from committing attacks at once.
- [ ] Defeated enemies leave the active battlefield.

## Phase 4 - Precision and Boss

- [ ] Enemy attacks display a red telegraph before impact.
- [ ] Dodging during the correct timing produces PERFECT DODGE.
- [ ] Perfect dodge creates a brief slow-motion effect.
- [ ] PERFECT DODGE displays COUNTER READY.
- [ ] The next normal attack receives the counter damage multiplier.
- [ ] The elite encounter appears after the waves.
- [ ] The boss encounter appears after the elite.
- [ ] Boss HP appears in the HUD.
- [ ] Boss telegraph and shockwave are visible.
- [ ] Boss can be defeated.

## Phase 5 - Vertical Slice

- [ ] Mission Start appears.
- [ ] Wave 1 completes.
- [ ] Wave 2 completes.
- [ ] Elite is defeated.
- [ ] Boss is defeated.
- [ ] Mission Clear appears.
- [ ] Mission Failed appears when the player dies.

## Phase 6 - Presentation

- [ ] Arena has a futuristic ruined-city atmosphere.
- [ ] Japanese-inspired structural motifs are visible.
- [ ] Energy rails are visible.
- [ ] Futuristic skyline structures are visible.
- [ ] Player armor silhouette is distinguishable from enemies.
- [ ] Attack, hit, skill, dodge, and boss effects provide readable feedback.
- [ ] Basic sound feedback is audible.

## Phase 7 - Content and Progression

- [ ] Defeated enemy count is shown.
- [ ] Experience increases after mission clear.
- [ ] Player level is shown.
- [ ] Upgrade points are shown.
- [ ] Result information appears at mission clear.

## Acceptance

The vertical slice passes when the player can use a gamepad to complete:

Move
-> Attack
-> Dodge
-> Perfect Dodge
-> Counter
-> Skill
-> Crowd Battle
-> Elite
-> Boss
-> Mission Clear

The prototype uses procedural placeholder visuals/audio. Content quality can be improved later without changing the core design direction.
