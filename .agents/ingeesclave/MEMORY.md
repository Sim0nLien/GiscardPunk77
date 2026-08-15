# Ingéesclave — Mémoire stable de GiscardPunk77

> Dernière revue : 2026-08-15
> Contient uniquement des faits stables et vérifiés. Ne jamais stocker de secret.

## Identité du projet

- Projet : GiscardPunk77
- Objectif stable : construire progressivement une démo FPS solo GiscardPunk 1977 sous Unity 6, sans élargissement silencieux du périmètre.
- Scène d'intégration PNJ confirmée : `Assets/Scenes/SampleScene.unity` ; elle est l'unique scène active dans `ProjectSettings/EditorBuildSettings.asset`. `Train.unity` ne contient qu'une caméra et une lumière.

## Sources canoniques

- `FPS_DEMO_BACKBONE.md` : vision et décisions approuvées de la première démo.
- `ROADMAP.md` : ordre d'implémentation et portes de sortie.
- `NPC_ETAT_DE_L_ART.md` : exploration et annotations utilisateur concernant les PNJ.
- `NPC_DEVIS_DE_PROMPTS.md` : devis modulaire P00–P19, dépendances et critères.
- `NPC_IMPLEMENTATION_CONTRACT.md` : synthèse compacte du baseline, règles et dépendances PNJ à lire avant les lots P01+.
- `NPC_IMPLEMENTATION_STATUS.md` : état compact du lot PNJ actif.
- `.agents/ingeesclave/FEEDBACK.md` : espace local de discussion ; les retours utilisateur y sont conservés avec leur statut et leur protocole de vérification.
- `.agents/ingeesclave/REPORTS/INDEX.md` : tableau de bord durable ; chaque lot possède un rapport tâche par tâche sous `REPORTS/<LOT_ID>.md`.
- `Docs/INGEESCLAVE/INDEX.md` : parcours pédagogique visible ; chaque lot dispose d'un guide compréhensible sous `Docs/INGEESCLAVE/<LOT_ID>.md`.

## Invariants architecturaux

- Les asmdefs runtime suivent `GiscardPunk77.Core <- GiscardPunk77.Gameplay <- GiscardPunk77.AI` sans dépendance circulaire ; Core ne référence aucun autre module projet.
- Unity Behavior doit orchestrer des composants C# testables, pas contenir navigation, raycasts ou dégâts.
- Un seul lot principal doit être implémenté et validé à la fois.
- La visibilité du joueur traverse `GiscardPunk77.Core.IVisibilityTarget` (`VisibilityPoint`, `IsCrouching`) ; `PlayerController` implémente ce contrat sans dépendance de Core vers le joueur.
- Le contrat P02 est `DamageInfo` -> `IDamageable` -> `Health`; `DamageableHitbox` délègue à la santé racine sans multiplicateur. Unity a validé 7/7 tests Gameplay le 2026-08-14.
- Le contrat P03 sépare `PlayerHitscanWeaponInput` -> `PlayerHitscanWeapon` -> `HitscanResolver` -> `IDamageable`. Le résolveur ignore la racine du tireur et ses enfants, trie les impacts par distance et arrête toujours le rayon au premier obstacle non ignoré.
- Les règles de chargeur, réserve, cadence et recharge résident dans `SemiAutomaticWeaponState`, classe C# sans entrée ni présentation. La configuration joueur P03 par défaut est 8 coups et 1,6 seconde de recharge.
- Unity a validé P03 avec 18/18 tests Gameplay et 23/23 tests Edit Mode au total le 2026-08-14 ; l’utilisateur a ensuite autorisé P04.
- P04 crée `NpcSandbox.unity` sous la racine isolée `P04 Navigation Sandbox Generated`. Le contrat Humanoid est rayon 0,5 m, hauteur 2 m, vitesse 3,5 m/s, évitement haute qualité et priorité 50 ; Unity a validé 27/27 tests Edit Mode, dont 4/4 AI P04.
- P05 introduit `NpcMotor` comme unique propriétaire runtime de `NavMeshAgent`; `NpcSandboxAgentProbe` ne lui adresse plus que des commandes. L'utilisateur a confirmé le 2026-08-14 que tous les tests P05 sont passés.
- P06 sépare `IDoorPassage`/`DoorReservationQueue` dans Gameplay, `SlidingDoor` comme implémentation compatible joueur et `NpcDoorTraversal` dans AI. Le harness pur a validé vingt passages FIFO et l'expiration. Deux exécutions Unity fournies le 2026-08-14 ont révélé que terminer sur le point d'attente opposé bloquait le propriétaire suivant (`8/10`, `RecalculationLimitReached`). La correction conserve le ticket jusqu'à un dégagement propre à chaque agent ; l'utilisateur a confirmé le 2026-08-15 que tous les tests passent. Le stress visuel H2 et la touche E restent non rapportés séparément.
- P07 sépare `NpcVisionParameters`/`NpcVisionEvaluation` (calcul pur), `NpcVisionObservation` (instantané immutable) et `NpcVisionSensor` (sampling/raycast/gizmos). AI consomme uniquement `IVisibilityTarget`; les réglages sandbox initiaux sont 12 m, 100°, 0,6 s, portée accroupie 65 %, exposition accroupie 175 % et 8 Hz répartis. L'utilisateur a confirmé le 2026-08-15 que les tests P07 passent ; H3 sera réglée avec P08.
- P08 sépare `NpcAwarenessModel` (suspicion/hystérésis), `NpcAwareness` (mémoire et abonnement P07) et `NpcAwarenessIndicator` (présentation billboard). `NpcAwarenessConfig.asset` est la source commune des valeurs : gain 0,9/s, décroissance 0,2/s, seuils 0,25/0,12 et 0,85/0,60. L'utilisateur a confirmé le 2026-08-15 que les tests P08 passent ; le réglage visuel H3 reste un suivi qualitatif historique.
- L'utilisateur a confirmé le 2026-08-15 que les tests P09 passent. P09 ajoute `AlertService`, composant de scène explicitement référencé : il expose `Calm`/`Alerted` et un `AlertSnapshot` immutable (point/heure initiaux), sans `Transform`, `Update` ni singleton statique. `NpcAlertReporter` relaie uniquement l'entrée `Alerted` d'une `NpcAwareness` assignée et ne propage pas une position vivante.
- P10 utilise `com.unity.behavior` 1.0.13, résolu et confirmé par l'utilisateur le 2026-08-15. Son assembly runtime exacte est `Unity.Behavior` (GUID `73907d139b13f8a43b7e3e95c329d30a`) ; les assemblies `Unity.Behavior.Authoring` et `Unity.Behavior.Editor` sont Editor-only et ne doivent pas entrer dans `GiscardPunk77.AI`. Le package est embarqué pour dix adaptations Unity 6000.5 réparties dans sept fichiers ; la chaîne Roslyn générée par Unity compile Behavior, AI, tests et Editor sans erreur, mais H4 doit encore confirmer l'import et le graphe dans l'interface.

## Conventions durables

- Préserver les modifications et assets utilisateur déjà présents dans le worktree.
- Utiliser `apply_patch` pour les éditions manuelles.
- Valider les comportements sur capsules avant l'art et les animations finales.
- Chaque livraison doit dire comment voir le changement, le résultat attendu, le signe d'échec pertinent et l'état de vérification.
- Chaque lot doit conserver son historique détaillé dans un rapport distinct ; `WORK_STATE.md` reste limité à l'état courant.
- Chaque lot doit aussi posséder un guide pédagogique visible : il explique les responsabilités, le vocabulaire, le cheminement, une expérience sans risque, les limites et le niveau réel de vérification sans promettre le contenu des lots futurs.

## Commandes validées

- `python <skill-creator>/scripts/quick_validate.py <skill>` valide la structure d'un skill ; PyYAML 6.0.3 est installé dans le profil Python utilisateur.
- `scripts/init_project_memory.ps1 -ProjectRoot <path>` initialise la mémoire Ingéesclave sans écraser les fichiers existants.
- Unity 6000.5.5f1 a compilé les assemblies P01 dans `Library/ScriptAssemblies/` le 2026-08-13 ; l'exécution Test Runner reste à confirmer dans l'éditeur.

## Risques et pièges connus

- Le worktree contient de nombreuses modifications utilisateur suivies et non suivies : toujours inspecter `git status` avant une édition.
- `SampleScene.unity` est volumineuse et modifiée ; aucune intégration PNJ avant P16.
- Le prototype `CapsuleWeapon` et son projectile physique sont conservés à côté du hitscan P03 ; ne pas les supprimer sans décision explicite ultérieure.
- `SampleScene` contient 11 composants `SlidingDoor` et des modifications utilisateur ; l'intégration PNJ est réservée à P16.
- L'exposition P07 est une visibilité continue et revient à zéro dès que la cible sort des conditions ; ne pas y ajouter une mémoire de suspicion, réservée à P08.
- La mémoire P08 est mise à jour seulement avec une ligne de vue claire ; après sa perte, dernière position et heure restent figées jusqu'à une nouvelle vision ou un reset.

## Préoccupations stables ouvertes

- P10 doit encore être confirmé à la porte H4 dans l'éditeur : ouverture/débogage du graphe minimal et échec lisible lorsque le moteur est désactivé.


