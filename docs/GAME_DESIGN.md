# TestGame Game Design Specification

Version: 1.0
Status: Canonical
Last Updated: 2026-09-19

## 1. Purpose

This document is the canonical game design specification for TestGame.

When implementing or modifying the game, this document takes precedence over temporary prototype implementations, convenience changes, and individual feature ideas.

The goal is to prevent the project from gradually drifting away from its intended game identity.

---

## 2. Core Game Concept

TestGame is an original third-person 3D action game built around:

- High-speed character control and responsive action.
- Large groups of enemies and satisfying crowd combat.
- Precise evasion, lock-on, and counter-oriented combat.
- A futuristic world with Japanese-inspired visual design.
- A stylized, flashy presentation rather than a realistic historical setting.

The intended experience combines two high-level inspirations:

1. The large-scale enemy clearing, spectacle, and exhilaration associated with Sengoku BASARA-like action.
2. The responsive movement, dodge timing, lock-on, and fast combat feel associated with Punishing: Gray Raven-like action.

These are reference points for experience and direction only. The project must use original characters, assets, animations, effects, UI, story, names, and other creative content.

---

## 3. Non-Negotiable Design Principles

The following principles must not be removed or weakened without an explicit specification change.

### 3.1 Gamepad-first

The game is designed for a gamepad first.

Keyboard/mouse support may exist for development or testing, but it is secondary.

The player must be able to complete the core combat loop using a gamepad.

### 3.2 Responsive character control

The player character should feel:

- Immediate.
- Agile.
- Easy to turn.
- Easy to stop.
- Able to transition quickly between movement, attack, and dodge.

Input responsiveness is more important than realistic physical weight.

### 3.3 Crowd combat

The core combat experience includes fighting multiple enemies at once.

The game should support battles that visually and mechanically communicate large enemy groups, while enemy attack scheduling prevents unfair simultaneous attacks.

### 3.4 Precision evasion

Dodging is a core combat mechanic, not an emergency-only action.

Well-timed evasion should create a meaningful advantage, such as a brief slow-motion or counter opportunity.

### 3.5 Stylish futuristic Japanese identity

The setting is NOT historical Sengoku Japan.

It is a future world where Japanese cultural and visual motifs coexist with advanced technology, ruined civilization, machinery, energy weapons, and futuristic architecture.

### 3.6 Original IP

Do not directly reproduce copyrighted characters, costumes, animations, UI layouts, logos, names, sound effects, environments, or other protected creative elements from reference works.

Use the references to define the intended feel, not the exact content.

---

## 4. World Setting

### 4.1 Era

The story takes place in a distant future after a large-scale civilization collapse.

Humanity once achieved highly advanced technology. After a major catastrophe, many regions of the world became ruined or uninhabitable.

Remnants of advanced civilization coexist with nature and newly developed settlements.

### 4.2 Visual Identity

The target visual direction is:

**Future SF + Ruins + Japanese-inspired design + Stylish action**

Examples of environmental elements:

- Collapsed skyscrapers.
- Futuristic laboratories.
- Abandoned military facilities.
- Energy infrastructure.
- Ruined transportation systems.
- Forests and mountains reclaiming urban areas.
- Japanese-inspired architecture and motifs adapted into futuristic forms.
- Holograms, displays, neon-like energy, and mechanical structures.

The world should feel like a future civilization that developed its own modern interpretation of Japanese culture.

### 4.3 Why Bladed Weapons Exist

Modern ranged and energy weapons are common.

However, some hostile entities possess special defenses that reduce the effectiveness of conventional ranged attacks.

High-density energy weapons can concentrate destructive force at close range, making advanced blades and other melee weapons practical.

This provides an in-world reason for the protagonist to fight primarily with a sword-like weapon.

---

## 5. Player Character

The initial protagonist is:

**A high-speed melee combatant from the ruined future world.**

The protagonist is not a historical samurai. He is a future warrior whose combat equipment evolved from Japanese sword and armor design.

Visual identity:

- Compact futuristic combat suit.
- Dark graphite and deep blue-gray base.
- Muted crimson identity accents.
- Cyan energy systems.
- Sealed visor and distinctive crest.
- Asymmetric shoulders, with heavier protection on the sword side.
- Layered waist armor inspired by segmented Japanese armor.
- Compact rear energy unit and fins.
- Waist-mounted sword sheath.
- Energy blade that activates during combat.

The intended silhouette is narrow-waisted, agile, asymmetric, and immediately readable in a large crowd.

Detailed art direction is defined in docs/PLAYER_CHARACTER_ART_DIRECTION.md.

The first playable character should be completed before expanding to multiple characters.

Do not prioritize a roster of characters until the basic combat experience is fun and stable.

---

## 6. Core Combat Loop

The core loop is:

1. Move into combat.
2. Attack groups of enemies.
3. Use skills to control the battlefield.
4. Observe enemy attacks.
5. Dodge at the correct moment.
6. Create a counter opportunity.
7. Re-engage.
8. Clear the enemy group.
9. Advance through the battlefield.
10. Fight elite enemies and bosses.

The intended feeling is:

**Move -> Attack -> Dodge -> Counter -> Re-engage**

Combat must not devolve into stationary attack-button spam.

---

## 7. Player Controls

The exact physical controller labels may vary between controller types, so Unity Input System actions should be used instead of hard-coding semantic labels such as A/B/X/Y.

Recommended action mapping:

| Action | Primary Input |
|---|---|
| Move | Left Stick |
| Camera | Right Stick |
| Normal Attack | Right-side face/trigger attack input |
| Dodge | Face button / shoulder input |
| Skill 1 | Face button |
| Skill 2 | Face button |
| Ultimate | Shoulder/trigger input |
| Lock-On | Shoulder input |
| Target Switch | Right Stick Button |
| Pause | Start/Menu |

The specific final bindings may be tuned after playtesting, but the gamepad-first principle is fixed.

---

## 8. Player Combat

### 8.1 Normal Attacks

Normal attacks should support a combo chain.

The player should be able to transition fluidly between:

- Movement
- Attack
- Dodge
- Skill
- Re-engagement

### 8.2 Dodge

Dodge must support:

- Invulnerability or equivalent protection during the intended timing window.
- Directional movement.
- A forgiving but skill-based timing system.
- A meaningful reward for precise timing.

### 8.3 Perfect Dodge / Counter Window

A well-timed dodge should be recognizable and rewarding.

Possible rewards:

- Brief time slowdown.
- Enemy vulnerability.
- Counter opportunity.
- Increased combo or damage opportunity.

The exact implementation may evolve, but the design principle remains.

### 8.4 Skills

The first combat prototype should have at least two active skills.

Suggested roles:

- Forward/high-speed attack.
- Area-of-effect crowd attack.

### 8.5 Ultimate

The protagonist should eventually have a visually powerful ultimate attack.

The ultimate should reinforce the game's spectacle and crowd-combat identity.

---

## 9. Targeting and Camera

### 9.1 Third-Person Camera

The default camera is a responsive third-person camera.

The player should remain clearly visible while allowing the camera to communicate combat space.

### 9.2 Free Camera

Normal combat supports free camera control.

### 9.3 Lock-On

Lock-on is optional rather than mandatory.

Use cases:

- Boss fights.
- Elite enemies.
- Precise 1v1 combat.
- Situations where target tracking improves control.

Large crowd combat should still be comfortable without permanent lock-on.

---

## 10. Enemy Design

The first enemy roster should stay small and purposeful.

### 10.1 Basic Enemy

- Low-to-medium HP.
- Common enemy.
- Large numbers.
- Simple attacks.

### 10.2 Heavy Enemy

- Higher HP.
- Slower but more threatening attacks.
- Higher resistance to knockback.

### 10.3 Ranged Enemy

- Attacks from distance.
- Encourages movement and target prioritization.

### 10.4 Elite Enemy

- Clearly stronger than regular enemies.
- More complex attack behavior.
- Used as a mini-boss or major threat.

Additional enemy types should be added only when they provide a meaningful gameplay difference.

---

## 11. Enemy AI and Crowd Control

Large battles must be designed around readability and fairness.

An example battle may contain 30-50 visible enemies while only a smaller number actively attack the player at the same instant.

A combat director should eventually manage:

- Enemy activation.
- Attack slots.
- Reinforcement timing.
- Spawn pacing.
- Crowd density.
- Elite timing.

The design goal is to create the perception of overwhelming numbers without producing unavoidable simultaneous damage.

---

## 12. Enemy Combat States

Enemies should eventually support states such as:

- Idle.
- Detect.
- Approach.
- Attack preparation.
- Attack.
- Hit.
- Knockback.
- Downed.
- Recovery.
- Death.

These states should be implemented in a way that allows additional enemy types to reuse the same core combat concepts.

---

## 13. Stage Structure

The long-term stage loop is:

Title
-> Stage Select
-> Battlefield
-> Enemy Group
-> Reinforcements
-> Elite Enemy
-> Further Combat
-> Boss
-> Clear
-> Rewards
-> Upgrade
-> Next Stage

The first prototype may use a much simpler version of this flow.

---

## 14. Stage Themes

The world should support multiple futuristic environments.

Examples:

### Ruined Future City
Collapsed towers, roads, holographic remnants, abandoned infrastructure.

### Research District
Laboratories, energy systems, containment areas, experimental technology.

### Mountain Settlement
Future infrastructure mixed with Japanese-inspired buildings and natural terrain.

### Underground Industrial Zone
Factories, machinery, power systems, underground transport networks.

### Central Tower
A major endgame location connected to the world's catastrophe.

These are example themes, not immediate implementation requirements.

---

## 15. Boss Design

Boss encounters should emphasize:

- Telegraphing.
- Positioning.
- Dodge timing.
- Lock-on.
- Pattern recognition.
- Counter opportunities.

Boss combat should feel different from crowd-clearing combat while using the same core player controls.

---

## 16. Progression

The long-term game may include:

- Player level.
- Weapon upgrades.
- Skill upgrades.
- New abilities.
- New stages.
- New enemy types.
- New bosses.
- Additional playable characters later.

Do not implement a complex progression system before the core combat is proven enjoyable.

There is no requirement to introduce a gacha system.

---

## 17. UI

The battle HUD should remain visually clean.

Important information:

- Player HP.
- Skill/ultimate status.
- Target information when appropriate.
- Boss HP when appropriate.
- Minimal combat feedback.

Combat visuals should remain the primary focus.

---

## 18. Technical Architecture

The preferred high-level Unity structure is:

Game
- GameFlow
- Input
- Player
  - Movement
  - Combat
  - Dodge
  - Skills
  - Health
- Enemy
  - AI
  - Combat
  - Health
  - Spawning
- Battle
  - CombatDirector
  - DamageSystem
  - TargetSystem
- Camera
  - ThirdPerson
  - LockOn
- Stage
  - StageManager
  - Objectives
  - SpawnPoints
- UI
- VFX
- Audio
- Data

The exact folder/class names may change as implementation evolves, but responsibilities should remain separated.

---

## 19. Data-Driven Design

Gameplay tuning data should be separable from core code where practical.

Examples:

- Weapon damage.
- Skill damage.
- Cooldown.
- Range.
- Knockback.
- Enemy HP.
- Enemy attack parameters.
- Stage spawn parameters.

Unity ScriptableObjects are a suitable mechanism for many of these data definitions.

---

## 20. Performance Requirements

The game is intentionally designed around large numbers of enemies.

Therefore, performance architecture must anticipate:

- Many active enemies.
- Repeated VFX.
- Repeated projectiles if introduced.
- Frequent spawning and despawning.

Object pooling should be used where profiling shows repeated allocation/deallocation is a bottleneck, especially for enemies and frequently reused effects.

Do not optimize blindly before there is evidence of a bottleneck.

---

## 21. Development Priorities

Priority order:

### Phase 1 - Feel
- Gamepad input.
- Third-person movement.
- Third-person camera.
- Normal attack.
- Dodge.
- Lock-on foundation.

### Phase 2 - Combat
- Combo.
- Damage.
- Knockback.
- Skills.
- Perfect dodge.
- Counter.
- Enemy death.

### Phase 3 - Crowd Battle
- Multiple enemy types.
- Spawn system.
- Crowd management.
- 10 -> 20 -> 30 -> 50 enemy scale tests.

### Phase 4 - Boss
- Boss AI.
- Patterns.
- Lock-on.
- Dodge/counter interaction.

### Phase 5 - Vertical Slice
A complete short stage that can be played from start to finish.

### Phase 6 - Presentation
- Character model.
- Animations.
- Weapons.
- VFX.
- Environments.
- Audio.
- UI polish.

### Phase 7 - Content
- More stages.
- More enemies.
- More bosses.
- Progression.
- Additional playable content.

---

## 22. Prototype Status Rule

The current prototype is not automatically considered specification-compliant.

When replacing or extending prototype code, compare the change against this document first.

Temporary prototype shortcuts are acceptable only when they do not establish the wrong long-term direction.

For example:

- Keyboard-only input may exist temporarily for development, but the target architecture must remain gamepad-first.
- Primitive placeholder graphics are acceptable, but the intended visual direction remains futuristic Japanese SF.
- Simple runtime-generated objects are acceptable for prototyping, but they must not silently redefine the long-term game structure.

---

## 23. Change Control

Before implementing any non-trivial feature, check:

- Does it reinforce the core game concept?
- Does it preserve gamepad-first control?
- Does it preserve responsive action?
- Does it support stylish futuristic Japanese SF?
- Does it support crowd combat or another clearly justified game need?
- Does it avoid unnecessary complexity before the core combat is proven?
- Does it introduce content that conflicts with the original-IP requirement?

After implementation, check the same points again.

If a requested change conflicts with this specification, do not silently change the game's direction. Update this specification first, then implement the change.

---

## 24. Definition of Success

The first major success criterion is not the number of systems implemented.

It is whether a player can:

1. Pick up a gamepad.
2. Move a futuristic sword-wielding character naturally.
3. Fight a group of enemies.
4. Attack and chain attacks.
5. Dodge enemy attacks.
6. Perform a well-timed dodge.
7. Counterattack.
8. Fight an elite enemy.
9. Defeat a boss.
10. Feel that the combat is fast, stylish, readable, and satisfying.

If those elements are not fun, adding more content is not the next priority.
