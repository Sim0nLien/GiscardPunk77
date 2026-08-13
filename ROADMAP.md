# GiscardPunk 1977 — FPS Demo Roadmap

> **Goal:** deliver a stable 2–3 minute single-player FPS demo set in a three-car train.
>
> **Scope authority:** [`FPS_DEMO_BACKBONE.md`](FPS_DEMO_BACKBONE.md) defines the approved design. This roadmap defines the implementation order. When the two documents disagree, update the backbone decision log first.

## How to Use This Roadmap

- Work from top to bottom; later phases depend on earlier exit gates.
- Keep only one gameplay feature in progress at a time.
- A checked task means it has been implemented **and tested in Play mode**.
- “Session” means one focused development block. It is an effort guide, not a deadline.
- Do not begin a stretch goal before the release-candidate gate passes.

## Critical Path

```text
Stable movement
  → Train greybox and motion illusion
  → Weapon and damage
  → Enemy and encounter
  → Complete objective loop
  → GiscardPunk identity
  → Playtest, optimize, release
```

## Phase R0 — Project Baseline

**Purpose:** make the project safe to extend and establish repeatable testing.

**Estimated effort:** 1–2 sessions.

- [x] Confirm Unity 6, URP, and Input System setup.
- [x] Implement mouse look, walk, sprint, crouch, and jump.
- [x] Prevent standing up when an obstacle blocks the player.
- [ ] Test and tune movement inside the `Train` scene.
- [ ] Confirm player collider dimensions against doors, seats, and carriage joints.
- [ ] Add editable mouse sensitivity and field-of-view settings.
- [ ] Create the agreed `_Project` folder structure without moving unrelated user assets blindly.
- [ ] Create a lightweight bootstrap/menu scene and register the gameplay scene in Build Profiles.
- [ ] Record one known-good baseline playthrough with no recurring console errors.

**Recommended starting values:** walk 4 m/s, sprint 6 m/s, crouch 2.5 m/s. Tune jump height for useful obstacle traversal, not platforming.

**Exit gate:** the player can traverse the complete greybox comfortably, cannot escape the train, and can restart testing without errors.

## Phase R1 — Train Greybox and Motion

**Purpose:** establish the entire playable space before combat code grows around temporary geometry.

**Estimated effort:** 2–4 sessions.

### Three-car layout

- [ ] **Car 1 — Storage/tutorial car:** safe spawn in a cluttered utility car, crates arranged to teach movement, and a computer used to introduce interaction and the objective; keep the first isolated enemy near the exit.
- [ ] **Car 2 — Dining/service car:** wider main combat arena, cover, healing pickup position.
- [ ] **Car 3 — Locomotive:** short final encounter and victory objective.
- [ ] Give every car a distinct silhouette, color block, and landmark.
- [ ] Validate combat sightlines from standing and crouched heights.
- [ ] Add collision proxies before decorative meshes.

### Train-motion illusion

- [ ] Keep the playable train stationary at the world origin.
- [ ] Build a looping exterior segment or scenery pool moving past the windows.
- [ ] Add wheel rhythm, carriage rattle, wind, and joint-impact audio hooks.
- [ ] Add subtle visual sway to presentation objects only; do not destabilize the player collider or AI.
- [ ] Hide exterior loop seams from every normal player position.
- [ ] Add a comfort toggle for presentation sway/head bob.

### Doors and navigation

- [ ] Prototype automatic main carriage doors.
- [ ] Reserve small doors, cabinets, and the final objective for the interact button.
- [ ] Mark or bake walkable AI areas after geometry is stable.

**Exit gate:** a player can walk from the spawn to the locomotive, the train convincingly appears to move, and no carriage transition traps the player.

## Phase R2 — Combat Sandbox

**Purpose:** make one 60-second fight enjoyable before building the whole encounter sequence.

**Estimated effort:** 4–7 sessions.

### Damage foundation

- [ ] Define one shared damage contract with amount, hit point, direction, and source.
- [ ] Implement reusable health with one-shot death protection.
- [ ] Add player damage, death event, and invulnerability rules if needed.
- [ ] Add a temporary damage target for isolated testing.

### Experimental light service pistol

- [ ] Create the first-person weapon anchor and temporary greybox model.
- [ ] Implement semi-automatic hitscan fire.
- [ ] Implement an 8-shot magazine, limited reserve, and 1.6-second reload.
- [ ] Prevent firing during reload, death, pause, and victory.
- [ ] Add mild recoil and spread suitable for close quarters.
- [ ] Add muzzle flash, firing sound hook, impact effect, and valid-hit marker.
- [ ] Add ammunition pickups with a clear maximum reserve.
- [ ] Establish the visual brief: compact 1970s French service-pistol form, wood/bakelite, brushed metal, analogue indicator, and one prototype component.

### Player survival

- [ ] Implement 100 player health.
- [ ] Add readable damage feedback that does not rely on color alone.
- [ ] Add one healing pickup in the dining/service car.

**Exit gate:** firing, hitting, reloading, running out of ammunition, taking damage, healing, and dying all work reliably in a test room.

## Phase R3 — Enemy and Main Encounter

**Purpose:** prove the core loop against one readable ranged enemy archetype.

**Estimated effort:** 4–7 sessions.

- [ ] Create the ranged guard prefab with health and hit volumes.
- [ ] Implement `Idle/Patrol → Alert → Reposition → Attack → Dead`.
- [ ] Use sight and range checks with clearly defined values.
- [ ] Implement burst fire with visible anticipation and recovery time.
- [ ] Use authored firing/cover positions; do not build dynamic cover discovery.
- [ ] Add alert, attack, hurt, and death audio hooks.
- [ ] Add visible hit reaction and unambiguous death state.
- [ ] Test navigation through each carriage connection.
- [ ] Build and tune one 60-second dining-car encounter.
- [ ] Confirm enemies cannot shoot through train walls or closed doors.

**First balance target:** standard guard health 100; pistol damage 34; approximately three confirmed hits per guard.

**Exit gate:** the main encounter is understandable, repeatable, winnable without perfect aim, and interesting enough to replay several times during tuning.

## Phase R4 — Complete Demo Loop

**Purpose:** turn the combat prototype into a game with a beginning, objective, failure, and ending.

**Estimated effort:** 3–5 sessions.

### Encounter sequence

- [ ] Spawn the player safely in the storage area of Car 1, with enough clear space to look around before moving.
- [ ] Arrange crates to teach walking, sprinting, crouching, jumping, and navigating narrow gaps without trapping the player.
- [ ] Add a clearly lit computer interaction that presents the objective: reach and secure the locomotive.
- [ ] Place one isolated teaching enemy near the exit of Car 1, after the movement and computer tutorial.
- [ ] Place the authored main encounter in Car 2.
- [ ] Place ammunition and the single healing pickup deliberately.
- [ ] Place a short final encounter in Car 3 using the same guard archetype.
- [ ] Add the locomotive interaction and victory trigger.

### State and UI

- [ ] Implement `Starting`, `Playing`, `Paused`, `PlayerDead`, and `Victory` states.
- [ ] Add crosshair, health, magazine/reserve ammunition, and objective text.
- [ ] Add pause, death, and victory panels.
- [ ] Add quick restart after death and full state reset.
- [ ] Block player and enemy combat outside the `Playing` state.
- [ ] Verify cursor lock/unlock during play, pause, death, and victory.

**Exit gate:** a new player can launch, understand, complete, fail, and restart the full 2–3 minute demo without developer help.

## Phase R5 — GiscardPunk Identity Pass

**Purpose:** replace the generic prototype impression with the selected political-tech thriller and pulp-satire identity.

**Estimated effort:** 5–10 sessions, depending on asset production.

### Visual priorities

- [ ] Establish the palette: cream, tobacco brown, burnt orange, smoked glass, brushed metal, and one harsh punk accent.
- [ ] Replace the most visible greybox materials first: walls, floor, seats, doors, weapon, and enemy.
- [ ] Add wood veneer, bakelite controls, analogue displays, CRTs, and geometric signage.
- [ ] Contrast clean state luxury with improvised wiring, stickers, torn propaganda, and sabotage.
- [ ] Create one readable invented institution/logo; avoid depending on real political branding.
- [ ] Light each car for navigation and enemy readability before decorative mood.

### Audio and narrative

- [ ] Add the period-inspired electronic music layer and mix it below combat-critical sounds.
- [ ] Record or prototype one short radio narrative sequence with subtitles.
- [ ] Add public-address/radio texture, weapon sounds, impacts, enemy cues, death cue, and victory sting.
- [ ] Keep satire mainly in advertisements, propaganda, announcements, and environmental details.

### Feel polish

- [ ] Polish weapon recoil and camera-safe motion.
- [ ] Improve muzzle, impact, hit, and enemy reaction feedback.
- [ ] Add subtle environmental movement: lights, cables, curtains, or loose objects.
- [ ] Add settings for FOV, mouse sensitivity, head bob/sway, and audio levels.

**Exit gate:** an uncaptioned screenshot and 20 seconds of audio are recognizable as GiscardPunk 1977 rather than a generic FPS prototype.

## Phase R6 — Release Candidate

**Purpose:** stabilize, optimize, and package the demo.

**Estimated effort:** 3–6 sessions plus playtest feedback.

- [ ] Complete at least three internal full playthroughs using different routes and behaviours.
- [ ] Run at least two fresh-player playtests without coaching.
- [ ] Record confusion, frustration, bugs, and completion time separately.
- [ ] Fix progression blockers, crashes, recurring errors, and state-reset bugs first.
- [ ] Tune encounter difficulty and resource placement from observed play, not assumptions.
- [ ] Profile the main encounter and remove visible frame spikes.
- [ ] Verify objectives and damage remain understandable without sound and without relying on color alone.
- [ ] Verify settings, pause, exit, death, victory, and restart paths.
- [ ] Produce a Windows development build, then a clean release build.
- [ ] Complete two consecutive error-free full playthroughs on the release build.

**Exit gate:** every Definition of Done item in the backbone passes and the build can be shared without setup instructions beyond controls.

## Stretch Gate — Radio Sequence

The short radio sequence is the selected stretch goal, but it is only allowed after Phase R4 is complete. If schedule or quality slips, keep a short placeholder transmission and spend the remaining effort on combat feedback and stability.

## Current Work Queue

### Now

1. Tune walk, sprint, crouch, jump, mouse sensitivity, and collider values in `Train`.
2. Test every existing door, seat gap, and carriage transition while standing, crouching, and jumping.
3. Decide the final standing/crouching clearance measurements from those tests.

### Immediately after

1. Lock the three-car greybox.
2. Implement the stationary-train/moving-exterior prototype.
3. Add automatic main-door and manual small-door prototypes.

### First combat slice

1. Shared damage and health components.
2. Greybox pistol with hitscan fire.
3. Magazine, limited reserve, reload, and one ammunition pickup.
4. One damage target, then one ranged guard.

## Risks and Scope Defences

| Risk | Early warning | Response |
|---|---|---|
| Movement breaks in narrow geometry | Frequent snagging or camera clipping | Fix dimensions before art; simplify collision meshes |
| Moving-train complexity grows | Player/AI jitter or physics hacks appear | Keep train stationary; motion stays visual/audio only |
| Bespoke weapon art delays combat | Combat waits for a final model | Use a greybox pistol until Phase R5 |
| Enemy AI expands beyond one archetype | Flanking, squads, or dynamic cover enter the task list | Return to authored positions and five-state AI |
| Limited ammunition causes unwinnable runs | Average player reaches finale empty | Add guaranteed pickups and tune reserve from playtests |
| Three-car demo grows | New rooms/cars appear before full loop works | Replace or postpone content; never add a fourth car |
| Style work hides weak gameplay | Art tasks begin before the 60-second fight is fun | Enforce R2/R3 exit gates before the full identity pass |

## Roadmap Completion Rule

A phase advances only when its exit gate passes. Bugs discovered in a completed phase re-enter the current work queue if they block the core loop. New ideas go into a future backlog unless they replace an approved item of similar cost.
