# TestGame AI Implementation Rules

This file defines the mandatory workflow for AI-assisted implementation in this repository.

## Source of Truth

Before making changes, read:

1. `docs/GAME_DESIGN.md`
2. The existing code relevant to the requested change.

`docs/GAME_DESIGN.md` is the canonical game-design source.

If existing prototype code conflicts with the specification, the specification wins unless the specification is explicitly changed.

## Character Art Check

When changing the protagonist model, procedural appearance, weapon presentation, or character materials, also read:

docs/PLAYER_CHARACTER_ART_DIRECTION.md

The following visual identity must be preserved unless the character-art specification is deliberately updated:

- Narrow-waisted agile silhouette.
- Asymmetric shoulders with heavier sword-side protection.
- Sealed visor and vertical crest.
- Futuristic combat armor derived from Japanese design language.
- Compact rear energy unit.
- Waist-mounted sword.
- Graphite/deep blue-gray base with muted crimson identity accents and restrained cyan energy.
- High mobility and unobstructed sword movement.

Do not add decorative parts that materially reduce readability or imply a heavy-mech character unless the art specification is changed first.

## Mandatory Pre-Change Check

Before changing code, answer these questions internally:

- What part of the game specification does this change support?
- Does it preserve the futuristic Japanese SF world?
- Does it preserve gamepad-first control?
- Does it preserve responsive, high-speed action?
- Does it preserve or intentionally support crowd combat where applicable?
- Does it avoid introducing unnecessary systems before core combat is proven?
- Does it preserve the requirement for original creative content?

For substantial changes, record the relevant specification section in the commit message or change description when practical.

## Mandatory Post-Change Check

After implementation, verify:

- The implementation still matches `docs/GAME_DESIGN.md`.
- No requirement was accidentally weakened or replaced.
- The player-control design remains gamepad-first.
- The visual/world direction has not drifted into historical Sengoku or unrelated settings.
- New systems have a clear responsibility and do not unnecessarily couple unrelated features.
- Existing tests/build checks are still appropriate and should be run when available.

## Specification Changes

When a requested feature conflicts with the current specification:

Do NOT silently implement the conflicting feature.

Instead:

1. Propose the necessary specification change.
2. Update `docs/GAME_DESIGN.md`.
3. Implement the feature against the updated specification.
4. Treat the specification update as a deliberate design decision.

## Prototype Rules

Temporary prototypes may use:

- Primitive Unity objects.
- Runtime-generated scenes.
- Simplified animations.
- Temporary keyboard support.

However, these are implementation shortcuts only.

They must not redefine the long-term game design.

## Priority Rule

When choosing between:

- more content, and
- better core combat feel,

prefer better core combat feel until the first playable combat loop is convincing.

## Change Checklist

Use this checklist for every meaningful implementation change:

### Before
- [ ] Read `docs/GAME_DESIGN.md`.
- [ ] Identify affected systems.
- [ ] Identify specification constraints.
- [ ] Confirm the change does not unintentionally alter the game direction.

### After
- [ ] Compare the result with `docs/GAME_DESIGN.md`.
- [ ] Check gamepad-first behavior.
- [ ] Check responsive-action behavior.
- [ ] Check architecture/responsibility boundaries.
- [ ] Run relevant tests/builds where practical.
- [ ] Note any deliberate specification change.

## Golden Rule

Never let incremental implementation turn TestGame into a different game by accident.
