# État d’implémentation PNJ

- Dernière mise à jour : 2026-08-20
- Dernier lot PNJ : `P11`
- Statut : `waiting_human — porte H4`
- Porte active : `H4 — debug du graphe et boucle Guard de dix minutes à confirmer`
- Compte rendu : `.agents/ingeesclave/REPORTS/P11.md`

## Lots validés

- P01 : architecture Core, Gameplay et AI — 5/5 tests.
- P02 : dégâts, santé, mort et hitbox — 7/7 tests Gameplay.
- P03 : hitscan et arme joueur — 18/18 tests Gameplay ; H1 fermée par l’utilisateur.
- P04 : sandbox NavMesh — 27/27 tests Edit Mode au moment de sa validation.
- P05 : moteur PNJ — tous les tests déclarés passés par l’utilisateur.
- P06 : API/réservation de porte — tous les tests déclarés passés ; contrôle visuel H2 encore non rapporté.
- P07 : signature et vision — tests déclarés passés.
- P08 : suspicion, mémoire courte et indicateur — tests déclarés passés.
- P09 : alerte globale bornée — tests déclarés validés.

## P10–P11 livrés à H4

- Behavior 1.0.13 reste embarqué ; AI ne référence que son assembly runtime `Unity.Behavior`.
- P10 fournit `NpcContext`, les conditions/actions génériques et le banc minimal.
- P11 ajoute `GuardConfig`, `GuardContext`, trois offsets de patrouille écrits et un historique de huit transitions.
- Le graphe `P11 Guard Routine` contient, dans l’ordre, Idle, Patrol, Suspicious et InvestigateLastKnownPosition sous un composite réactif.
- La suspicion annule la patrouille, oriente le garde, visite une dernière position, attend brièvement puis retourne au poste.
- L’événement global `Alerted` annule immédiatement `NpcMotor`, passe le diagnostic en rouge et termine la routine non combattante en `Failure`.
- `Guard Capsule.prefab` réunit identité, santé, navigation, vision, awareness, contexte, présentation et graph agent ; ses références de scène restent explicitement vides.

## Validation effectuée

- Unity 6000.5.5f1 a compilé les assemblies AI, tests et Editor sans erreur.
- Unity a généré/importé `GuardConfig.asset`, `P11 Guard Routine.asset`, le matériau de debug et `Guard Capsule.prefab` avec code de sortie 0.
- `Logs/P11-tests-rerun.xml` : 28/28 tests AI Edit Mode passés, 0 échec.
- Le runtime graph sérialisé contient les quatre enfants dans l’ordre attendu.
- Le prefab sérialise l’override `Guard Context` et toutes les références internes ; `AlertService` et la cible de vision restent des références de scène à assigner.
- Audit statique : aucun accès `NavMeshAgent`, raycast ou dégât dans les nodes Guard.

## Vérification H4 P11

1. Ouvrir `NpcSandbox` et baker/vérifier son NavMesh.
2. Déposer `Assets/_Project/Prefabs/AI/Guard Capsule.prefab` sur la zone marchable.
3. Assigner le même `AlertService` à `NpcContext` et `NpcAlertReporter`, puis la cible à `NpcVisionSensor`.
4. Entrer en Play Mode et ouvrir `Assets/_Project/Config/AI/P11 Guard Routine.asset` en mode **Debug**.
5. Suivre `Docs/INGEESCLAVE/P11.md`, notamment la boucle de dix minutes.

## Résultat attendu

La capsule alterne bleu/vert sans destination invalide ; la suspicion produit jaune puis orange et un retour bleu au poste ; l’alerte globale arrête immédiatement la route et produit le rouge. L’historique reste limité à huit transitions.

## Signe d’échec

Une destination invalide, une route qui continue après l’alerte, l’absence de retour au poste, plus de huit transitions, un node absent ou un graphe non débogable bloque H4.

## Prochaine action

Exécuter H4 P11 pendant dix minutes et répondre avec les observations sur boucle, suspicion, retour au poste, couleurs, historique et alerte globale. Ne pas commencer P12.
