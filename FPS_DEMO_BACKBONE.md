# GiscardPunk 1977 — FPS Demo Backbone

> **Purpose:** this document is the project's single source of truth for the first playable demo. It defines the minimum scope, the order of work, the decisions still to make, and the conditions for calling the demo complete.
>
> **How to decide:** every open decision has an ID (for example, `P0.1`). Tick one checkbox in the project file, or tell Codex: **“Choose P0.1-A, P0.2-A, P1.3-B…”** Codex can then update this document and implement the chosen option.
>
> **Implementation plan:** see [`ROADMAP.md`](ROADMAP.md) for the ordered work queue, dependencies, and phase exit gates.

## 1. Demo Vision

**One-sentence pitch:** a short, single-player FPS set aboard a moving train in an alternate, retro-futurist version of France in 1977, where the player survives a compact combat encounter and reaches the locomotive.

**Target experience:** a focused 5–10 minute desktop demo that proves three things:

1. Moving and shooting feel good in the narrow space of a train.
2. The “GiscardPunk 1977” setting has a clear identity.
3. The player can understand, complete, fail, and restart one simple objective.

**Current technical base:** Unity 6, Universal Render Pipeline (URP), Input System, `CharacterController`, existing first-person movement and camera scripts, and an existing `Train` scene.

## 2. Scope Rules

### Must be in the first demo

- One playable train level.
- One player controller with mouse and keyboard support.
- One reliable hitscan weapon.
- One enemy archetype.
- Player and enemy health, damage, death, and restart.
- One simple objective and a clear end state.
- Essential feedback: crosshair, muzzle flash, hit feedback, sound, and basic UI.
- A short introduction to the GiscardPunk identity through environment, color, sound, and props.

### Explicitly out of scope

- Multiplayer or online services. (pas pour l'instant a delete)
- Open world, branching campaign, procedural train, or multiple levels. (idée faire un dishonored fps infiltration avec espionnage enquète mais pour très long terme)
- Inventory grid, skill tree, dialogue tree.
- More than one fully developed weapon or enemy archetype. (arme vraiment parlante comme famas laser ....)
- Advanced stealth, cover AI, destruction, vehicles, or train driving.
- Cinematic cutscenes, save slots, achievements, localization, or mod support.
- Photorealism and historically exact reconstruction.

Anything outside the must-have list requires replacing an item of similar cost; it must not silently increase the demo scope.

## 3. Definition of Done

The demo is complete when a new player can launch it and, without developer help:

- Start in the train and understand the objective within 10 seconds.
- Move, look, shoot, reload, and recognize when a shot hits.
- Fight through one encounter using a stable frame rate and without progression blockers.
- Die and restart quickly.
- Reach the final objective and see an unmistakable victory state.
- Identify the world as “alternate France, 1977” from the audiovisual presentation.
- Cinematique ouverte, on peut bouger ou Giscard nous donne mission depuis Minitel futuriste avec pixel hexagolnle orange et gris

The demo must also produce no recurring console errors during a complete playthrough.

## 4. Priority Order and Decisions

Priority meanings:

- **P0 — Foundation:** required for a playable game. Do these first.
- **P1 — Core combat:** required for the demo to be enjoyable.
- **P2 — Level and game loop:** required for a complete beginning-to-end experience.
- **P3 — Identity and polish:** makes it GiscardPunk rather than a generic prototype.
- **P4 — Optional stretch:** only after the Definition of Done is met.

---

## P0 — Foundation

### P0.1 Player movement model

- [ ] **P0.1-A — Grounded and deliberate (recommended):** walk, short sprint, no jump. Best fit for narrow train corridors; easiest to tune and least likely to cause collision problems.
- [X] **P0.1-B — Classic FPS:** walk, sprint, crouch, and jump. More expressive, but adds animation, collision, level-design, and testing work.
- [ ] **P0.1-C — Minimal prototype:** walk only. Fastest, but may feel flat and makes evasion difficult.

**Acceptance:** movement is responsive, diagonal speed is normalized, stairs/door thresholds do not trap the player, and the player cannot leave the train geometry.

**Decision:** gameplay feel takes priority over the lowest implementation cost. Jumping, sprinting, and crouching are part of the demo and must be tuned for the train's narrow spaces.

### P0.2 Input support

- [X] **P0.2-A — Keyboard and mouse first (recommended):** fully polish one control scheme; add controller later only if time remains.
- [ ] **P0.2-B — Keyboard, mouse, and controller:** broader access, but requires aim sensitivity, glyph, focus, and UI testing for both.

**Minimum bindings:** move, look, fire, reload, sprint if selected, interact if selected, pause, and restart after death.

### P0.3 Train implementation

- [X] **P0.3-A — Stationary train, moving exterior illusion (recommended):** the train stays at the world origin while scenery, lighting, particles, and audio simulate motion. Stable physics and simple AI navigation.
- [ ] **P0.3-B — Physically moving train:** authentic world movement, but creates parented-character, physics, camera jitter, projectile, and navigation complexity.
- [ ] **P0.3-C — Stationary train in a station:** cheapest implementation, but weakens the central fantasy.

**Acceptance:** no visible gaps in the exterior loop from normal play positions, and motion effects do not cause discomfort.

### P0.4 Scene and project structure

Use a small, explicit structure:

```text
Assets/
  _Project/
    Art/
    Audio/
    Materials/
    Prefabs/
      Characters/
      Environment/
      Gameplay/
      UI/
    Scenes/
    Scripts/
      Combat/
      Core/
      Enemies/
      Player/
      UI/
```

- [X] **P0.4-A — One gameplay scene plus a lightweight bootstrap/menu scene (recommended):** clean restart and game-state ownership.
- [ ] **P0.4-B — One scene only:** faster at first, but menus, restarting, and persistent services can become tangled.

**Architecture rule:** prefer small components with one responsibility. Store tunable weapon/enemy data in `ScriptableObject` assets only when it reduces duplication; do not build a large framework for one instance.

### P0.5 Prototype metrics

Choose provisional numbers, then change them only after playtesting:

| Metric | Recommended starting value |
|---|---:|
| Walk speed | 4.0 m/s |
| Sprint speed | 6.0 m/s |
| Player health | 100 |
| Weapon damage | 34 |
| Magazine capacity | 8 shots before reloading |
| Reload | 1.6 s |
| Enemy health | 100 |
| Demo duration | 2–3 min |


---

## P1 — Core Combat

### P1.1 First weapon

**Decision:** the weapon must be lightweight and immediately communicate the GiscardPunk setting.

- [X] **P1.1-A — Experimental light service pistol (selected):** a compact semi-automatic sidearm built from 1970s French service-pistol shapes, with wood/bakelite furniture, brushed metal, analogue indicators, and one conspicuous prototype component. Readable, easy to balance, and specific to the setting.
- [ ] **P1.1-B — Compact submachine gun:** energetic and forgiving, but needs more recoil, ammunition, audio, effects, and balance work.
- [ ] **P1.1-C — Pure energy sidearm:** strongest retro-futurist signal, but requires more visual explanation and bespoke effects.

Use **hitscan raycasts** for the first weapon. Projectile simulation is unnecessary for a short-range train encounter.

### P1.2 Firing behaviour

- [X] **P1.2-A — Hip fire with mild spread and recoil (selected):** quickest complete solution; works in close quarters.
- [ ] **P1.2-B — Hip fire plus aim-down-sights:** familiar and polished, but requires weapon/camera animation and separate tuning.
- [ ] **P1.2-C — Perfectly accurate arcade fire:** simplest, but reduces weapon character.

**Required feedback chain:** input → weapon motion/muzzle flash → shot sound → impact effect → hit marker for valid damage → enemy reaction/death.

### P1.3 Ammunition and reload

- [ ] **P1.3-A — Magazine plus unlimited reserve (recommended):** reload remains part of combat without creating a level-breaking ammo shortage.
- [X] **P1.3-B — Magazine plus limited reserve and pickups (selected):** adds resource pressure and exploration. Balance must guarantee enough ammunition to finish the short demo.
- [ ] **P1.3-C — No reload/heat cooldown:** distinctive for an energy weapon, but removes a useful combat rhythm.

### P1.4 Health model

- [ ] **P1.4-A — Fixed health, no healing during combat (recommended):** clear and easy to balance for one short encounter; restart is quick.
- [ ] **P1.4-B — Regenerating health:** forgiving, but encourages waiting and needs safe-space rules.
- [X] **P1.4-C — Fixed health with one healing pickup:** adds a small tactical recovery beat at modest cost.

### P1.5 Enemy archetype

- [X] **P1.5-A — Ranged guard (recommended):** patrol/idle, notice player, seek line of sight, fire bursts, take cover only through authored positions, and die.
- [ ] **P1.5-B — Melee pursuer:** simpler attacks, but navigation and crowding in narrow corridors can feel unfair.
- [ ] **P1.5-C — Stationary security automaton/turret:** easiest navigation, strong retro-tech identity, but less dynamic.

**Minimal AI state flow:** `Idle/Patrol → Alert → Chase or Reposition → Attack → Dead`.

**Do not add yet:** squad tactics, searching memory, dynamic cover generation, grenades, flanking, or complex perception meters.

### P1.6 Damage contract

All damageable objects should use one small shared damage interface or component. A hit must define damage amount, hit point, hit direction, and source. This keeps weapons, enemies, props, and later extensions decoupled.

**Acceptance:** enemies cannot receive damage after death, one shot causes damage once, friendly/self hits behave consistently, and destroyed objects unsubscribe from events.

---

## P2 — Level and Complete Game Loop

### P2.1 Objective

- [X] **P2.1-A — Reach and secure the locomotive (recommended):** linear, self-explanatory, and naturally uses the train layout.
- [ ] **P2.1-B — Recover a confidential briefcase, then escape:** adds a return or extraction beat and a strong 1970s thriller tone.
- [ ] **P2.1-C — Disable a prototype device before a timer expires:** creates urgency, but timers make first-time navigation and balance harder.

### P2.2 Level shape

- [ ] **P2.2-A — Four-car linear train (recommended):** starting compartment → passenger car → dining/service car combat space → locomotive finale.
- [X] **P2.2-B — Three-car micro level:** storage/tutorial car → dining/service combat car → locomotive finale; safer for a first build, about 3–5 minutes.
- [ ] **P2.2-C — Six cars with a side route:** more exploration, but likely doubles environment and encounter work.

Every car needs a gameplay role, a recognizable landmark, and at least one safe readability line between player and threat. Avoid long empty corridors and doors narrower than the player combat space requires.

### P2.3 Encounter structure

Recommended first pass:

1. **Start / tutorial beat:** player spawns in a cluttered storage car, learns movement around deliberately placed crates, then uses a clearly lit computer to receive the locomotive objective without immediate lethal pressure.
2. **First enemy:** an isolated guard near the storage-car exit teaches firing, damage, and death feedback after movement and interaction are understood.
3. **Main encounter:** 3–4 enemies distributed across a wider car with cover and sightline breaks.
4. **Short recovery:** ammunition reload opportunity and environmental storytelling.
5. **Final encounter:** 2–3 enemies or one stronger presentation of the same archetype near the objective.
6. **Victory trigger:** player interacts with or enters the locomotive objective zone.

- [X] **P2.3-A — Authored enemy placements (recommended):** predictable, easy to tune, and appropriate for one level.
- [ ] **P2.3-B — Spawn waves:** reusable and energetic, but can feel artificial in a train.
- [ ] **P2.3-C — Randomized placements:** replayable, but makes difficulty and navigation testing less reliable.

### P2.4 Interaction model

- [ ] **P2.4-A — Automatic doors and objective triggers:** no general interaction system; fewer prompts and edge cases.
- [ ] **P2.4-B — All doors use one context-sensitive interact button:** consistent, but slows movement through main carriage transitions.
- [X] **P2.4-C — Hybrid doors (selected):** main carriage doors open automatically; small compartment, cabinet, and objective doors use one context-sensitive interact button.
### P2.5 Game states

Required states: `Starting`, `Playing`, `Paused`, `PlayerDead`, and `Victory`.

The game must stop accepting combat input when paused, dead, or victorious. Death and victory screens must offer an obvious restart or return action.

### P2.6 UI

Minimum UI:

- Central crosshair.
- Current magazine count; reserve only if limited ammunition is selected.
- Health display or a clearly legible damage-state treatment.
- Objective text shown briefly at start and updated at completion.
- Pause, death, and victory panels.

- [X] **P2.6-A — Minimal graphic HUD (recommended):** small health/ammo readouts styled like 1970s instrumentation.
- [ ] **P2.6-B — Mostly diegetic HUD:** immersive but harder to make readable and more expensive to animate.

---

## P3 — GiscardPunk Identity and Polish

### P3.1 Tone

- [X] **P3.1-A/B — Political-tech thriller with pulp satire (selected):** the world and central threat are treated seriously, while propaganda, advertisements, dialogue, and exaggerated retro-futurist details provide wit and satire.
- [ ] **P3.1-A — Pure political-tech thriller:** elegant state modernism, analogue surveillance, secret prototype, controlled tension.
- [ ] **P3.1-B — Pure pulp action satire:** louder, stranger, and more comedic; allows exaggerated characters and propaganda.
- [ ] **P3.1-C — Dark alternate-history dystopia:** oppressive and serious, but needs careful narrative context to avoid generic grimness.

The setting is fictional alternate history, not a documentary reconstruction. Use invented institutions and characters unless a deliberate historical-reference policy is later written.

### P3.2 Visual pillars

Use three consistent pillars instead of many disconnected references:

1. **1970s French modernism:** smoked glass, brushed metal, wood veneer, orange/brown/cream upholstery, geometric signage.
2. **State retro-technology:** CRTs, reel-to-reel machines, chunky switches, pneumatic mechanisms, monochrome terminals.
3. **Punk disruption:** improvised wiring, stickers, torn official graphics, harsh accent colors, modified uniforms and devices.

- [X] **P3.2-A — Clean state luxury invaded by punk sabotage (recommended):** creates an immediate visual conflict and supports environmental storytelling.
- [ ] **P3.2-B — Fully grimy underground train:** atmospheric, but loses the elegant “Giscard” contrast.
- [ ] **P3.2-C — Optimistic retro-future showcase:** distinctive and bright, but needs stronger narrative justification for combat.

### P3.3 Audio pillars

Required layers:

- Continuous rail rhythm, carriage rattle, wind, and joint impacts.
- Distinct weapon fire, reload, impact, enemy alert, hurt, and death sounds.
- Subtle public-address messages or radio texture for place and period.
- A short victory sting and clear death cue.

- [ ] **P3.3-A — Mostly environmental, no continuous music (recommended):** strong tension and lower content cost.
- [X] **P3.3-B — Period-inspired electronic score:** stronger identity, but requires music production and careful mixing.

### P3.4 Essential feel polish

Add in this order:

1. Weapon recoil and camera-safe weapon motion.
2. Muzzle flash, impact particles/decals, and hit marker.
3. Enemy hit reaction and a clear death pose/animation.
4. Damage vignette or directional indicator.
5. Train sway that affects presentation, not player collision stability.
6. Small environmental motion: lights, cables, curtains, loose objects.

Avoid strong camera shake, excessive chromatic aberration, heavy motion blur, or forced head bob. These reduce clarity and can cause discomfort.

### P3.5 Accessibility baseline

- Mouse sensitivity control.
- Master, effects, and music volume controls if music exists.
- Subtitles for meaningful speech.
- Crosshair with sufficient contrast.
- Do not communicate damage or objectives through color alone.
- Pause menu and an always-available exit path.

- [X] **P3.5-A — Add field-of-view and head-bob controls (recommended):** high comfort value for an FPS at low implementation cost.
- [ ] **P3.5-B — Fixed presentation settings:** faster, but less comfortable for many players.

---

## P4 — Optional Stretch Goals

Choose **at most one** only after the complete demo works:

- [ ] **P4-A — Secondary experimental weapon:** strongest combat extension; highest animation/effects/balance cost.
- [ ] **P4-B — One environmental hazard:** electrified floor, steam burst, or closing carriage door; improves encounter variety.
- [X] **P4-C — One short radio narrative sequence:** deepens the setting with limited gameplay risk.
- [ ] **P4-D — Destructible small props/glass:** improves weapon feel but adds performance and cleanup work.
- [ ] **P4-E — Score/rank at victory:** supports replay without changing the level.
- [ ] **P4-F — No stretch goal (recommended):** spend the time on tuning, bugs, lighting, and audio balance.

## 5. Recommended Default Package

The selected package is now the production baseline:

| Decision | Selected baseline |
|---|---|
| Movement | `P0.1-B` — walk + sprint + crouch + jump |
| Input | `P0.2-A` — keyboard and mouse first |
| Train | `P0.3-A` — stationary train, moving exterior |
| Structure | `P0.4-A` — bootstrap/menu + gameplay scene |
| Weapon | `P1.1-A` — experimental light service pistol |
| Aim | `P1.2-A` — polished hip fire |
| Ammo | `P1.3-B` — limited reserve and pickups |
| Health | `P1.4-C` — fixed health and one healing pickup |
| Enemy | `P1.5-A` — ranged guard |
| Objective | `P2.1-A` — secure the locomotive |
| Level | `P2.2-B` — three linear cars, 2–3 minutes |
| Encounters | `P2.3-A` — authored placements |
| Interaction | `P2.4-C` — automatic main doors + interactive small doors |
| HUD | `P2.6-A` — minimal graphic HUD |
| Tone | `P3.1-A/B` — political-tech thriller with pulp satire |
| Visual direction | `P3.2-A` — luxury versus sabotage |
| Music | `P3.3-B` — period-inspired electronic score |
| Comfort | `P3.5-A` — FOV and head-bob controls |
| Stretch | `P4-C` — one short radio narrative sequence |

## 6. Implementation Milestones

### Milestone 1 — Greybox controller

- Lock movement, look, collision, sensitivity, and cursor behaviour.
- Build greybox dimensions for all selected train cars.
- Implement the stationary-train/moving-exterior illusion.
- **Exit condition:** traversing the complete train is stable and comfortable.

### Milestone 2 — Combat sandbox

- Implement weapon, ammo/reload, damage contract, health, and one enemy.
- Test in one greybox car before expanding the level.
- **Exit condition:** a repeatable 60-second fight is understandable and fun.

### Milestone 3 — Complete loop

- Place all encounters and objective triggers.
- Add game states, UI, death, victory, pause, and restart.
- **Exit condition:** the demo can be completed from launch to victory and recovered from death.

### Milestone 4 — Identity pass

- Replace only the most visible greybox surfaces and props first.
- Establish lighting, train motion audio, signage, and three visual pillars.
- **Exit condition:** screenshots are recognizable as GiscardPunk 1977 rather than a generic FPS.

### Milestone 5 — Polish and release candidate

- Improve the feedback chain, balance encounters, profile performance, and fix blockers.
- Run fresh-player playtests and record recurring confusion or frustration.
- **Exit condition:** all Definition of Done items pass in two consecutive full playthroughs.

## 7. Testing Checklist

### Every feature build

- [ ] No new recurring Unity console errors.
- [ ] Player cannot escape the carriage or become permanently stuck.
- [ ] Fire/reload inputs cannot duplicate damage or ammunition.
- [ ] Enemy death and player death occur exactly once.
- [ ] Pause stops gameplay and safely releases the cursor.
- [ ] Restart restores enemies, objective, health, ammunition, and time state.

### Every milestone

- [ ] Complete keyboard-and-mouse playthrough.
- [ ] Test at low and high mouse sensitivities.
- [ ] Test narrow doors, carriage joints, corners, and enemy line of sight.
- [ ] Test losing, restarting, winning, pausing, and exiting in different orders.
- [ ] Profile the main encounter; investigate visible frame spikes before adding content.

## 8. Decision Log

Record final choices here so implementation does not drift.

| Date | Decision ID | Choice | Reason | Consequence |
|---|---|---|---|---|
| 2026-08-07 | P0.1 | B | Movement depth is central to good gameplay | Train geometry must support jumping and crouching |
| 2026-08-07 | P1.1 | A, customized | The light weapon must visibly belong to GiscardPunk | Requires a bespoke visual design, but retains simple hitscan behaviour |
| 2026-08-07 | P2.2 | B | Keep the first demo short and achievable | Three cars and a 2–3 minute target |
| 2026-08-07 | P2.4 | C | Fast flow through the train plus small tactile interactions | Implement both proximity triggers and one interact action |
| 2026-08-07 | P3.1 | A/B hybrid | Serious intrigue and pulp humour are both part of the identity | Keep stakes coherent; place satire mainly in world-building |

## 9. Change Rule

Before adding or changing a feature, answer:

1. Does it improve the core loop: **move → identify threat → shoot → survive → advance**?
2. Is it needed for the Definition of Done?
3. What does it cost in code, art, audio, level design, UI, and testing?
4. If it increases scope, which existing item is removed or simplified?

If the answers are unclear, keep the feature out of the first demo and place it in a future backlog.

## 10. Production Status

The initial direction choices are confirmed. Implementation follows the milestone order above.

- [x] **Implemented:** P0.1 — walk, sprint, crouch, jump, smooth camera height, and blocked stand-up detection.
- [ ] **Current:** P0.1 — tune movement values and validate carriage clearances in Play mode.
- [ ] **Next:** P0.3 — prototype the moving-exterior illusion.
- [ ] **Then:** P1 — build the experimental pistol and combat sandbox.

Any future direction change must be recorded in the Decision Log before implementation.
