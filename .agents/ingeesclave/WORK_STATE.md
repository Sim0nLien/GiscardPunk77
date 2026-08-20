# Ingéesclave — État de travail de GiscardPunk77

- Statut : `waiting_human`
- Dernière mise à jour : 2026-08-20
- Lot/prompt : P11 — garde : repos, patrouille et investigation
- Rapport actif : `.agents/ingeesclave/REPORTS/P11.md`
- Objectif : livrer une boucle non combattante réutilisable repos/patrouille/suspicion/investigation sur le socle P10
- Périmètre autorisé : configuration et contexte Guard, points écrits, nodes fins, graphe P11, prefab capsule, présentation temporaire, tests, mémoire et guide ; aucun combat, P12/P13, recherche multi-points ou modification de scène

## Critères d'acceptation

- Le garde alterne repos et points de patrouille écrits sans contourner `NpcMotor`.
- La suspicion interrompt la patrouille et utilise seulement la dernière position mémorisée par P08.
- Après une investigation et un délai bornés sans contact, le garde revient à son poste.
- Une alerte globale annule immédiatement la routine.
- Le prefab capsule expose des couleurs temporaires et un historique court.
- Le graphe fonctionne dix minutes sans destination invalide dans le sandbox.

## Fichiers touchés

- `.agents/ingeesclave/WORK_STATE.md`
- `.agents/ingeesclave/FEEDBACK.md`
- `.agents/ingeesclave/MEMORY.md`
- `.agents/ingeesclave/DECISIONS.md`
- `.agents/ingeesclave/REPORTS/INDEX.md`
- `.agents/ingeesclave/REPORTS/P11.md`
- `Docs/INGEESCLAVE/INDEX.md`
- `Docs/INGEESCLAVE/P11.md`
- `NPC_IMPLEMENTATION_CONTRACT.md`
- `NPC_IMPLEMENTATION_STATUS.md`
- `Assets/_Project/Scripts/AI/Behavior/Guard/`
- `Assets/_Project/Scripts/Editor/NpcBehaviorGraphAuthoring.cs`
- `Assets/_Project/Scripts/Editor/GuardPrefabAuthoring.cs`
- `Assets/_Project/Tests/EditMode/AI/GuardContextTests.cs`
- `Assets/_Project/Tests/EditMode/AI/NpcStateReaderTests.cs`
- `Assets/_Project/Config/AI/GuardConfig.asset`
- `Assets/_Project/Config/AI/P11 Guard Routine.asset`
- `Assets/_Project/Art/Debug/Guard Capsule Debug.mat`
- `Assets/_Project/Prefabs/AI/Guard Capsule.prefab`

## Travail réalisé

- `GuardConfig` sépare durées, mémoire, capacité d'historique et couleurs.
- `GuardPatrolRoute` contient trois offsets écrits ancrés au poste capturé au démarrage.
- `GuardContext` spécialise `NpcContext`, borne huit transitions et s'abonne à l'alerte de scène.
- Le graphe P11 contient `GuardRoutineComposite` puis, dans l'ordre, Idle, Patrol, Suspicious et InvestigateLastKnownPosition.
- L'investigation rejoint une dernière position, attend deux secondes et retourne au poste ; l'alerte annule le moteur et termine la routine en échec explicite.
- `GuardStatePresenter` colore la capsule sans logique de décision ni instanciation répétée de matériau.
- L'outil P11 a généré la configuration, le graphe et le prefab sans modifier de scène.
- Le harness P10 initialise désormais explicitement `Health` en Edit Mode avant le test létal.

## Validations

- Validé automatiquement : compilation Roslyn ciblée de AI, tests AI et `Assembly-CSharp-Editor`, zéro erreur.
- Validé automatiquement : Unity importe et génère les assets P11 avec code de sortie 0.
- Validé automatiquement : `Logs/P11-tests-rerun.xml`, 28/28 tests AI Edit Mode passés.
- Validé par inspection : quatre enfants ordonnés dans le runtime graph ; références internes et override `Guard Context` du prefab présents.
- Validé statiquement : aucun `NavMeshAgent`, raycast ou dégât dans les nodes Guard.
- Non vérifié : boucle Play Mode de dix minutes, rendu des couleurs, retour au poste et Debug visuel du graphe.

## Blocages ou validation humaine

- Porte H4 active : placer le prefab dans `NpcSandbox`, assigner `AlertService` et la cible de vision, puis observer le protocole complet de `Docs/INGEESCLAVE/P11.md`.

## Prochaine action exacte

- Exécuter le protocole H4 P11 pendant dix minutes et rapporter la boucle, la suspicion, le retour au poste, les couleurs, l'historique et l'interruption globale.

## Passation

- P11 est implémenté et validé automatiquement ; il attend uniquement la validation humaine H4. P12 n'est pas commencé.
