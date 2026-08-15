# Ingéesclave — État de travail de GiscardPunk77

- Statut : `waiting_human`
- Dernière mise à jour : 2026-08-15
- Lot/prompt : P10 — installation et socle Unity Behavior
- Rapport actif : `.agents/ingeesclave/REPORTS/P10.md`
- Objectif : installer Unity Behavior et créer une couche d'orchestration générique qui délègue aux composants existants
- Périmètre autorisé : package `com.unity.behavior`, asmdef AI après audit, `AI/Behavior`, outil/asset de test P10, tests, mémoire et guide ; aucun P11, garde final, GOAP, combat, package tiers ou changement de `SampleScene`

## Critères d'acceptation

- `com.unity.behavior` stable est installé sans changement des autres versions directes.
- Les asmdefs exacts du package sont inspectés avant la dépendance de `GiscardPunk77.AI`.
- `NpcContext` référence explicitement identité, santé, moteur, vision, awareness et alert service, avec diagnostic clair si une référence manque.
- Les nodes lisent l'état, attendent, choisissent une destination, commandent le moteur et interrompent sur mort/alerte sans raycast ni dommage.
- Un graphe minimal est ouvrable et débogable dans Unity ; désactiver le moteur fait échouer proprement l'action.

## Fichiers touchés

- `.agents/ingeesclave/WORK_STATE.md`
- `.agents/ingeesclave/FEEDBACK.md`
- `.agents/ingeesclave/MEMORY.md`
- `.agents/ingeesclave/DECISIONS.md`
- `.agents/ingeesclave/REPORTS/INDEX.md`
- `.agents/ingeesclave/REPORTS/P05.md`
- `.agents/ingeesclave/REPORTS/P06.md`
- `.agents/ingeesclave/REPORTS/P07.md`
- `Docs/INGEESCLAVE/INDEX.md`
- `Docs/INGEESCLAVE/P05.md`
- `Docs/INGEESCLAVE/P06.md`
- `Docs/INGEESCLAVE/P07.md`
- `NPC_IMPLEMENTATION_STATUS.md`
- `Assets/Script/SlidingDoor.cs`
- `Assets/_Project/Scripts/Gameplay/Doors/`
- `Assets/_Project/Scripts/AI/Navigation/NpcDoorTraversal.cs`
- `Assets/_Project/Scripts/AI/Navigation/NpcDoorTraversalStressProbe.cs`
- `Assets/_Project/Scripts/Editor/NpcSandboxNavigationAuthoring.cs`
- `Assets/_Project/Tests/EditMode/Gameplay/DoorReservationQueueTests.cs`
- `Assets/_Project/Tests/PlayMode/AI/GiscardPunk77.AI.PlayMode.Tests.asmdef`
- `Assets/_Project/Tests/PlayMode/AI/NpcDoorTraversalPlayModeTests.cs`
- `Assets/_Project/Scripts/AI/Perception/`
- `Assets/_Project/Scripts/AI/Debug/NpcVisionSandboxTarget.cs`
- `Assets/_Project/Scripts/Editor/NpcVisionSandboxAuthoring.cs`
- `Assets/_Project/Tests/EditMode/AI/NpcVisionEvaluationTests.cs`
- `Assets/_Project/Scripts/AI/Perception/NpcAwareness*.cs`
- `Assets/_Project/Scripts/AI/Debug/NpcAwarenessIndicator.cs`
- `Assets/_Project/Scripts/Editor/NpcAwarenessSandboxAuthoring.cs`
- `Assets/_Project/Tests/EditMode/AI/NpcAwarenessTests.cs`
- `Assets/_Project/Config/AI/NpcAwarenessConfig.asset`
- `Packages/manifest.json`
- `Assets/_Project/Scripts/Core/Actors/ActorIdentityComponent.cs`
- `Assets/_Project/Scripts/AI/Behavior/NpcContextRequirement.cs`
- `Assets/_Project/Scripts/AI/Behavior/NpcContext.cs`
- `Assets/_Project/Tests/EditMode/AI/NpcContextTests.cs`
- `Assets/_Project/Scripts/AI/Behavior/NpcStateQuery.cs`
- `Assets/_Project/Scripts/AI/Behavior/NpcStateReader.cs`
- `Assets/_Project/Scripts/AI/Behavior/Nodes/`
- `Assets/_Project/Scripts/AI/GiscardPunk77.AI.asmdef`
- `Assets/_Project/Scripts/Editor/NpcBehaviorGraphAuthoring.cs`
- `Assets/_Project/Tests/EditMode/AI/NpcStateReaderTests.cs`
- `Assets/_Project/Tests/EditMode/AI/GiscardPunk77.AI.Tests.asmdef`
- `Assets/_Project/Tests/EditMode/AssemblyBoundaryTests.cs`
- `Packages/com.unity.behavior/`
- `Packages/packages-lock.json`
- `.agents/ingeesclave/REPORTS/P10.md`
- `Docs/INGEESCLAVE/P10.md`

## Travail réalisé

- L'utilisateur confirme que Unity Behavior 1.0.13 est résolu ; P10 a repris sans ouvrir P11.
- Les huit asmdefs installés ont été audités avant l'ajout de la référence runtime `Unity.Behavior` à AI.
- Behavior 1.0.13 est embarqué avec dix adaptations d'appels conditionnelles, réparties dans sept fichiers, pour les API `EntityId`/drag-and-drop de Unity 6000.5 ; sa version reste 1.0.13 et les autres versions directes sont inchangées.
- `NpcContext` regroupe explicitement identité, santé, moteur, vision, awareness et service d'alerte.
- Les nodes ajoutés lisent un état, attendent, choisissent une destination et délèguent le mouvement à `NpcMotor` ; `DeadOrGloballyAlerted` alimente un `Abort` sans logique métier dans le graphe.
- L'outil Editor P10 crée ou ouvre sans écrasement le graphe minimal `Start -> Choose -> Move -> Wait`.
- Deux tests de contexte et trois tests de lecture d'état sont prêts.

## Validations

- Validé par l'utilisateur : tests P09 et résolution de Behavior 1.0.13.
- Validé automatiquement P10 : le lockfile reconnaît Behavior comme package embarqué ; les commandes Roslyn générées par Unity compilent sans erreur Serialization, Authoring, Editor, Muse, AI, ses tests Edit/Play Mode et `Assembly-CSharp-Editor`.
- Validé par inspection : version 1.0.13, cible Unity 6000.0/16f1 et plateformes des huit asmdefs.
- Validé statiquement : aucun raycast, dégât, accès `NavMeshAgent`, recherche de scène ou déplacement direct dans `AI/Behavior`.
- À vérifier dans l'interface Unity après refresh : absence d'erreur actuelle, exécution des cinq tests P10, création/ouverture du graphe, debug Play Mode et fallback du moteur désactivé.

## Blocages ou validation humaine

- Suivi historique : H3 (réglage visuel P08) et le stress visuel P06 ne sont pas détaillés dans le retour utilisateur, malgré les tests verts.
- Porte active H4 : confirmer le graphe minimal et le fallback moteur selon `Docs/INGEESCLAVE/P10.md`.

## Prochaine action exacte

- Laisser Unity rafraîchir, lancer `GiscardPunk77.AI.Tests`, exécuter l'outil P10, déboguer l'agent puis tester `NpcMotor` désactivé.

## Passation

- P09 est validé. P10 est livré à H4 avec un refresh/validation Unity encore requis ; P11 n'est pas commencé.


