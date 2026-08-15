# État d’implémentation PNJ

- Dernière mise à jour : 2026-08-15
- Dernier lot PNJ : `P10`
- Statut : `waiting_human — porte H4`
- Porte active : `H4 — refresh Unity, graphe lisible et fallback moteur à confirmer`
- Compte rendu : `.agents/ingeesclave/REPORTS/P10.md`

## Lots validés

- P01 : architecture Core, Gameplay et AI — 5/5 tests.
- P02 : dégâts, santé, mort et hitbox — 7/7 tests Gameplay.
- P03 : hitscan et arme joueur — 18/18 tests Gameplay ; H1 fermée par l’utilisateur.
- P04 : sandbox NavMesh — 27/27 tests Edit Mode au moment de sa validation.
- P05 : moteur PNJ — tous les tests déclarés passés par l'utilisateur.
- P06 : API/réservation de porte — tous les tests déclarés passés par l'utilisateur ; contrôle visuel H2 encore non rapporté.
- P07 : signature et vision — tests déclarés passés par l'utilisateur.
- P08 : suspicion, mémoire courte et indicateur — tests déclarés passés par l'utilisateur.
- P09 : alerte globale bornée — tests déclarés validés par l'utilisateur.

## P10 livré à H4

- `com.unity.behavior` 1.0.13 est résolu puis embarqué pour conserver dix adaptations d'appels, réparties dans sept fichiers, nécessaires à Unity 6000.5 ; aucune autre version directe du manifeste ne change.
- Les huit asmdefs ont été inspectés avant que `GiscardPunk77.AI` référence uniquement `Unity.Behavior`.
- `ActorIdentityComponent`, `NpcContextRequirement` et `NpcContext` exposent les six références demandées avec diagnostic précis.
- `NpcStateCondition`, `NpcWaitAction`, `NpcChooseDestinationAction` et `NpcMoveToDestinationAction` orchestrent sans raycast, dégâts ou accès direct à `NavMeshAgent`.
- `NpcBehaviorGraphAuthoring` crée ou ouvre le graphe P10 `Start -> Choose -> Move -> Wait` sans écraser un asset existant.
- Cinq tests Edit Mode couvrent contexte et lecture d'état ; leur exécution Unity finale reste demandée à H4.

## Implémenté dans P09

- `AlertService` de scène, niveau `Calm/Alerted`, événement de changement réel et reset idempotent.
- `AlertSnapshot` immutable de la première observation : point et heure, sans référence à une cible vivante.
- `NpcAlertReporter` explicitement relié à une `NpcAwareness` et à un service de scène.
- Banc idempotent `P09 Alert Service Test Rig`, qui relie seulement sa racine à P08.
- Quatre tests Edit Mode : abonnement/désabonnement, diffusion unique, lecture tardive, snapshot et reset.

## Contrat P09

- Le service ne possède ni `Transform` de joueur, ni `Update`, ni singleton statique.
- La première alerte du cycle est conservée; les tentatives suivantes ne republient rien et ne remplacent pas le snapshot.
- Tout nouveau garde disposant de la référence explicite lit immédiatement `Level`.

## Validation effectuée

- P06 : tous les tests corrigés sont confirmés verts par l'utilisateur.
- P07 : tests confirmés verts par l'utilisateur.
- P08 : tests déclarés verts par l'utilisateur.
- P09 : assemblies AI/tests et Editor compilées avec 0 erreur et 0 avertissement.
- P09 : audit statique sans `Transform`, `Update` ou membre `static` dans `AlertService`.
- P10 : version, lockfile et huit asmdefs inspectés ; seul `Unity.Behavior` est référencé par AI.
- P10 : audit statique sans raycast, dégâts, recherche de scène, accès `NavMeshAgent` ou déplacement direct dans les nodes.
- P10 : la chaîne Roslyn générée par Unity compile sans erreur les assemblies Behavior, AI, tests AI Edit/Play Mode et `Assembly-CSharp-Editor`. L'import complet et l'exécution dans l'interface Unity restent à confirmer à H4.

## Vérification P10

1. Laisser Unity terminer le refresh ou rouvrir le projet si la Console contient encore un chemin `Library/PackageCache/com.unity.behavior`.
2. Vérifier Behavior `1.0.13`, source `Embedded`, dans Package Manager.
3. Lancer `GiscardPunk77.AI.Tests` en Edit Mode.
4. Exécuter `Tools > GiscardPunk77 > P10 > Create or Open Minimal Behavior Graph`.
5. Suivre `Docs/INGEESCLAVE/P10.md` pour assigner un Behavior Agent, sélectionner **Debug** et tester le moteur désactivé.

## Résultat attendu

Le graphe est lisible et surligné en Play Mode, l'agent rejoint sa destination par `NpcMotor`, puis un moteur désactivé produit un `Failure` explicite sans exception répétée.

## Signe d’échec

Une erreur de compilation, un test rouge, un graphe impossible à ouvrir, un node absent ou un fallback moteur illisible bloque H4.

## Prochaine action

Exécuter H4 et répondre avec le nombre de tests verts, l'ouverture/debug du graphe et le résultat moteur désactivé. Ne pas commencer P11.
