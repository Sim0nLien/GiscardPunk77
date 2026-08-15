# Contrat d'implémentation PNJ — GiscardPunk 1977

> **Statut :** baseline P00 auditée et H0 confirmée. P09 est validé par l'utilisateur. P10 est livré à la porte humaine H4 ; P11 n'est pas commencé.

## But de la tranche

Construire progressivement la première tranche PNJ de la démo sans modifier des scènes ni systèmes hors du lot actif.

La cible immédiate est un garde capsule E1+ et des civils capsules C0/C1. Les conséquences civiles systémiques, le LLM, le GOAP, les humanoïdes et les animations finales restent hors de la tranche principale.

## Sources d'autorité

1. Instructions utilisateur actuelles.
2. `FPS_DEMO_BACKBONE.md` — vision et décisions de démo.
3. `ROADMAP.md` — dépendances et ordre de production.
4. `NPC_DEVIS_DE_PROMPTS.md` — lots P00–P19 et critères d'acceptation.
5. `NPC_ETAT_DE_L_ART.md` — contexte et intentions annotées.
6. Code, scènes et tests actuels.

La mémoire `.agents/ingeesclave/` est une aide de reprise ; elle ne prime jamais sur ces sources.

## Scènes

| Rôle | Scène | État P00 |
|---|---|---|
| Intégration PNJ confirmée | `Assets/Scenes/SampleScene.unity` | seule scène active dans les Build Settings ; H0 confirmée |
| Scène de test PNJ | `Assets/_Project/Scenes/Tests/NpcSandbox.unity` | créée au P04 sous une racine générée isolée ; bake NavMesh à valider à H2 |
| `Train` | `Assets/Scenes/Train.unity` | caméra et lumière seulement ; ne pas intégrer de PNJ ici |

P00 ne sérialise, ne renomme et ne déplace aucune scène.

## Baseline technique vérifié

- Unity `6000.5.5f1`, URP et Input System.
- `com.unity.ai.navigation` `2.0.14` est installé. Unity Behavior `1.0.13` est embarqué sous `Packages/com.unity.behavior` afin de conserver cette version tout en adaptant dix appels répartis dans sept fichiers aux API devenues bloquantes avec Unity `6000.5.5f1`.
- `SampleScene` contient le `Player` avec `PlayerController`, la `Main Camera` avec `FpsCameraController`, `CapsuleWeapon`, 11 composants `SlidingDoor`, `TrainScenerySystem`, `LoopingGround` et `SunLight`.
- `PlayerController` fournit déjà marche, sprint, saut et accroupissement via `CharacterController`; P01 peut adapter la posture sans changer le mouvement.
- `CapsuleWeapon` instancie aujourd'hui des capsules physiques avec `Rigidbody` et `CapsuleProjectile`. Le backbone et P03 demandent un hitscan séparé : conserver le prototype, ne pas le remplacer avant P03.
- Aucun contrat partagé de dégâts/santé, prefab PNJ, NavMesh enregistré, perception ou graphe de comportement n'existe encore.

## Composants à préserver

- `Assets/Script/PlayerController.cs`
- `Assets/Script/FpsCameraController.cs`
- `Assets/Script/CapsuleWeapon.cs`
- `Assets/Script/CapsuleProjectile.cs`
- `Assets/Script/SlidingDoor.cs`
- `Assets/Script/Environment/TrainScenerySystem.cs`
- `Assets/Script/Environment/LoopingGround.cs`
- `Assets/Script/Environment/SunLight.cs`
- les modifications utilisateur déjà présentes dans le worktree et dans `SampleScene.unity`.

Un lot futur ne modifie un de ces composants que lorsque son prompt l'autorise explicitement.

## Architecture PNJ figée pour P01+

```text
GiscardPunk77.Core
  <- GiscardPunk77.Gameplay
       <- GiscardPunk77.AI
```

- Core : types neutres, identité, contrats de frontière.
- Gameplay : dégâts, santé, armes, portes et réservations.
- AI : perception, navigation, Behavior, garde, civil et debug.
- Unity Behavior orchestre ; il ne possède ni raycasts, ni dégâts, ni déplacement direct, ni logique de porte.
- Configuration réutilisable et état runtime restent séparés.

## Règles d'exécution

- Un seul lot P00–P19 à la fois, dépendances validées avant de continuer.
- Préserver les GUID Unity et ne pas déplacer d'assets sans procédure autorisée.
- Utiliser `apply_patch` pour les modifications manuelles.
- Ne pas introduire de singleton mutable, gestionnaire universel ou dépendance circulaire.
- Ne pas rechercher des objets à chaque `Update`.
- Tout événement, abonnement ou réservation futur doit être libéré au disable, reset et décès.
- Réserver l'intégration dans `SampleScene` au P16.
- Valider chaque porte humaine avant son lot suivant.

## Démonstration et feedback

Chaque lot PNJ conserve un compte rendu tâche par tâche sous `.agents/ingeesclave/REPORTS/<LOT_ID>.md` et se termine par un protocole concret : le point d'entrée à ouvrir ou la commande à lancer, les étapes de vérification, le résultat observable attendu, le signe d'échec pertinent et la limite de validation (automatique, manuelle ou non vérifiée).

Le canal de discussion durable est `.agents/ingeesclave/FEEDBACK.md`. L'utilisateur peut y ajouter un retour `FB-XXX` ou l'écrire en conversation ; l'agent le consignera sans modifier son texte et indiquera son statut de traitement. `FB-001`, qui portait la confirmation H0, est traité.

## Dépendances du prochain lot

P10 dépend de P05, P08 et P09, tous validés par l'utilisateur. Après audit des huit asmdefs résolus, `GiscardPunk77.AI` référence uniquement l'assembly runtime `Unity.Behavior`, sans inverser `GiscardPunk77.Core <- GiscardPunk77.Gameplay <- GiscardPunk77.AI`. L'implémentation P10 est arrêtée à H4 ; P11 attend la validation humaine du graphe, du debug et de l'échec explicite.

## H0 — décision confirmée

L'utilisateur a confirmé le 2026-08-13 que `Assets/Scenes/SampleScene.unity` est la scène jouable actuelle et la future scène d'intégration PNJ au P16. Cette confirmation n'autorise toujours aucune modification de scène avant P16.

## Contrats disponibles après P01

- `ActorKind` : `Unknown`, `Player`, `Guard`, `Civilian`.
- `TeamId` : identifiant entier immutable, avec `Neutral` égal à la valeur par défaut.
- `ActorIdentity` : paire immutable `ActorKind`/`TeamId`.
- `IVisibilityTarget` : `VisibilityPoint` et `IsCrouching` uniquement.
- `PlayerController` implémente `IVisibilityTarget` sans changement de sa logique de mouvement.

## Contrats disponibles après P07

- `NpcVisionParameters` : réglages bornés de portée, cône, exposition et posture.
- `NpcVisionScore` / `NpcVisionEvaluation` : calcul géométrique, exposition et phase sans état de garde.
- `NpcVisionObservation` : instantané immutable de la dernière mesure.
- `NpcVisionSensor` : sampling, raycast et publication d'observations ; aucune suspicion ou alerte.

## Contrats disponibles après P08

- `NpcAwarenessConfig` : asset partagé des vitesses, seuils et options de présentation.
- `NpcAwarenessModel` : progression, décroissance et transitions à hystérésis sans Unity scene.
- `NpcAwareness` : mémoire courte, suspicion runtime et événements d'état, sans raycast.
- `NpcAwarenessIndicator` : billboard de présentation, sans logique de détection ou de suspicion.

## Contrats disponibles après P09

- `AlertLevel` : niveau partagé `Calm` / `Alerted`.
- `AlertSnapshot` : point/heure initiaux immuables, sans cible ou `Transform` vivant.
- `AlertService` : composant de scène explicitement référencé, diffusion unique et reset idempotent; pas de singleton statique.
- `NpcAlertReporter` : pont d'une `NpcAwareness` explicitement assignée vers un `AlertService` assigné.
