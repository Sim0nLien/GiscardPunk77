# Ingéesclave — Registre de décisions de GiscardPunk77

> Initialisé le 2026-08-13. Ne pas supprimer une décision remplacée ; la marquer `superseded`.

## Décisions actives

### DEC-001 — Mémoire locale par projet

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `transversal`
- Décideur : utilisateur + conception du skill
- Contexte : l'agent de travail doit conserver une mémoire sans mélanger plusieurs projets.
- Décision : stocker la mémoire sous `.agents/ingeesclave/` dans chaque projet, séparée en `MEMORY.md`, `WORK_STATE.md` et `DECISIONS.md`.
- Conséquences : le skill global reste portable ; la mémoire est versionnable avec le projet et vérifiable par les futurs agents.
- Preuves : demande utilisateur du 2026-08-13 et `ingeesclave/SKILL.md`.
- Remplace : aucune

### DEC-002 — Mémoire subordonnée aux sources réelles

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `transversal`
- Décideur : conception du skill
- Contexte : une mémoire persistante peut devenir obsolète ou erronée.
- Décision : les instructions actuelles, documents canoniques, code et tests priment toujours sur la mémoire ; toute contradiction corrige la mémoire.
- Conséquences : aucune reprise ne doit exécuter aveuglément un ancien état de travail.
- Preuves : `ingeesclave/SKILL.md`, section « Gérer les conflits ».
- Remplace : aucune

### DEC-003 — Passation validée automatiquement

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `transversal`
- Décideur : conception du skill après essai indépendant
- Contexte : une convention Markdown seule ne garantit pas qu'un futur agent laisse un état reprenable.
- Décision : exiger `validate_project_memory.ps1` avant la passation ; le validateur contrôle les fichiers, rubriques, statut, variables non résolues et l'unicité de la prochaine action.
- Conséquences : une passation mal formée échoue explicitement avant la réponse finale.
- Preuves : essai indépendant du skill et tests positif/négatif du validateur le 2026-08-13.
- Remplace : aucune

### DEC-004 — Scène d'intégration PNJ proposée

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `P00`, `P16`
- Décideur : utilisateur, confirmation H0 du 2026-08-13
- Contexte : la seule scène enregistrée dans les Build Settings est `Assets/Scenes/SampleScene.unity`, tandis que `Train.unity` ne contient qu'une caméra et une lumière.
- Décision : utiliser `SampleScene.unity` comme scène d'intégration PNJ provisoire et réserver `NpcSandbox` à une future scène de test isolée.
- Conséquences : aucun lot P01+ ne modifie une scène de production avant H0 ; P16 intégrera les PNJ uniquement dans la scène confirmée.
- Preuves : `ProjectSettings/EditorBuildSettings.asset`, audit YAML P00 des deux scènes, `NPC_IMPLEMENTATION_CONTRACT.md`.
- Remplace : aucune

### DEC-005 — Démonstration obligatoire et feedback local

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `transversal`, `P00`–`P19`
- Décideur : utilisateur
- Contexte : chaque livraison doit permettre de constater précisément le changement et recueillir un retour qui survit aux sessions.
- Décision : imposer dans toute passation « Comment voir le changement », « Résultat attendu », limite de vérification et un espace `.agents/ingeesclave/FEEDBACK.md` versionnable.
- Conséquences : les retours sont identifiés `FB-XXX`, préservés dans leur formulation utilisateur et traités avec un statut ; aucun service externe n'est requis.
- Preuves : demande utilisateur du 2026-08-13 et `ingeesclave/SKILL.md`.
- Remplace : aucune

### DEC-006 — Contrat minimal de visibilité du joueur

- Date : `2026-08-13`
- Statut : `accepted`
- Lots concernés : `P01`, `P07`
- Décideur : devis P01 et implémentation technique vérifiée
- Contexte : l'AI future doit lire la posture et le point visé sans référencer directement le contrôleur joueur.
- Décision : `IVisibilityTarget` expose uniquement `VisibilityPoint` et `IsCrouching`; `PlayerController` implémente directement ce contrat Core avec sa propriété de posture existante et la position de sa caméra.
- Conséquences : aucune modification de scène ni composant adaptateur à attacher ; Core ne connaît pas `PlayerController`; perception, score et éclairage restent réservés à P07.
- Preuves : `Assets/_Project/Scripts/Core/Visibility/IVisibilityTarget.cs`, `Assets/Script/PlayerController.cs`, compilation Unity P01.
- Remplace : aucune

### DEC-007 — Compte rendu distinct par lot

- Date : `2026-08-14`
- Statut : `accepted`
- Lots concernés : `transversal`, `P00`–`P19`
- Décideur : utilisateur
- Contexte : l'utilisateur veut savoir précisément ce que l'agent a réalisé dans chaque tâche de chaque lot, même après plusieurs sessions.
- Décision : conserver un index `.agents/ingeesclave/REPORTS/INDEX.md` et un rapport `<LOT_ID>.md` contenant des tâches stables `TXX`, fichiers, preuves, résultat observable, limites, feedback et suite.
- Conséquences : `WORK_STATE.md` reste compact et pointe vers le rapport actif ; un rapport terminé n'est pas réécrit hors validation tardive, feedback ou addendum daté.
- Preuves : demande utilisateur du 2026-08-14, rapports P00/P01/META-001 et skill Ingéesclave mis à jour.
- Remplace : aucune

### DEC-008 — Guides pédagogiques visibles par lot

- Date : `2026-08-14`
- Statut : `accepted`
- Lots concernés : `transversal`, `P02`–`P19`
- Décideur : utilisateur
- Contexte : les rapports techniques et le code ne suffisent pas à une personne qui découvre le projet et veut apprendre les procédés employés.
- Décision : conserver un guide visible `Docs/INGEESCLAVE/<LOT_ID>.md` et un index associé, séparés des rapports techniques cachés. Chaque guide explique le problème, les responsabilités, le vocabulaire, le cheminement, une expérience sans risque, les limites et l'état réel de vérification.
- Conséquences : chaque prochain lot Ingéesclave crée et met à jour son guide ; la passation finale le lie explicitement. Le guide décrit les faits observables sans recopier le raisonnement interne ni promettre une fonctionnalité future.
- Preuves : demande utilisateur du 2026-08-14, `Docs/INGEESCLAVE/P02.md` et skill personnel `C:/Users/simon/.codex/skills/ingeesclave/`.
- Remplace : aucune

### DEC-009 — Banc de tir minimal dans NpcSandbox au P03

- Date : `2026-08-14`
- Statut : `accepted`
- Lots concernés : `P03`, `P04`
- Décideur : instruction utilisateur actuelle, prioritaire sur le contrat antérieur
- Contexte : le contrat réservait la création de `NpcSandbox` au P04, tandis que le prompt P03 actuel exige d'y configurer l'arme, des cibles et des obstacles pour la porte H1.
- Décision : P03 peut créer ou mettre à jour uniquement une racine idempotente `P03 Hitscan Test Rig` dans `NpcSandbox`. Cette racine contient le banc de tir sans NavMesh ; P04 ajoute ensuite sa propre géométrie/navigation sans supprimer le banc P03.
- Conséquences : aucun changement dans `SampleScene`; P03 ne bake aucun NavMesh et ne construit pas le wagon P04.
- Preuves : demande utilisateur P03 du 2026-08-14 et ordre d'autorité de `NPC_IMPLEMENTATION_CONTRACT.md`.
- Remplace : restriction « créer NpcSandbox au P04, pas avant » uniquement pour le banc de tir P03 ; la responsabilité NavMesh de P04 reste active.

### DEC-010 — Empreinte Humanoid commune au sandbox P04

- Date : `2026-08-14`
- Statut : `accepted`
- Lots concernés : `P04`, `P05`, `P06`
- Décideur : configuration Unity existante et implémentation P04
- Contexte : un rayon d’agent différent entre le bake et le `NavMeshAgent` fausserait les mesures du couloir et du seuil.
- Décision : utiliser le type Humanoid ID 0 existant avec rayon 0,5 m et hauteur 2 m. Le probe commence à 3,5 m/s, accélération 12 m/s², rotation 360°/s, évitement haute qualité et priorité 50.
- Conséquences : le couloir de 1,5 m accepte une capsule mais pas deux côte à côte ; la baie de 4,6 m sert au croisement. P05 peut déplacer ces valeurs vers une configuration réutilisable sans changer silencieusement l’empreinte H2.
- Preuves : `ProjectSettings/NavMeshAreas.asset`, `NpcSandboxTuning.cs` et tests P04.
- Remplace : aucune

### DEC-011 — Contrat de passage dans Gameplay, orchestration dans AI

- Date : `2026-08-14`
- Statut : `accepted`
- Lots concernés : `P06`, futurs consommateurs de porte
- Décideur : devis P06 et implémentation technique vérifiée
- Contexte : `SlidingDoor` appartient à l'assembly par défaut et conserve l'entrée joueur, tandis que l'AI ne doit dépendre ni de cette classe concrète ni d'un type `Guard`.
- Décision : placer `IDoorPassage`, `DoorPassageState` et `DoorReservationQueue` dans `GiscardPunk77.Gameplay.Doors`; faire implémenter ce contrat par `SlidingDoor`; faire consommer seulement l'interface par `NpcDoorTraversal` dans AI.
- Conséquences : la porte conserve E et son Rigidbody ; AI peut utiliser un passage de test ou une autre porte ; un seul propriétaire FIFO occupe le seuil et les tickets abandonnés expirent.
- Preuves : compilations ciblées P06, harness de vingt passages et sources `Gameplay/Doors`, `SlidingDoor.cs`, `NpcDoorTraversal.cs`.
- Remplace : aucune

### DEC-012 — Mesure visuelle séparée de la décision de garde

- Date : `2026-08-15`
- Statut : `accepted`
- Lots concernés : `P07`, `P08`
- Décideur : devis P07 et instruction utilisateur
- Contexte : distance, regard, posture, occultation et exposition doivent être testables sans créer un cerveau de garde monolithique.
- Décision : placer le calcul pur, l'observation immutable et le capteur physique dans des types distincts ; `NpcVisionSensor` publie une observation et ne modifie ni suspicion, ni alerte, ni état de garde.
- Conséquences : l'accroupissement réduit portée et gain tout en augmentant la durée requise ; une rupture de vue remet seulement l'exposition P07 à zéro. La mémoire/décroissance et l'interprétation appartiennent à P08.
- Preuves : sources `AI/Perception`, tests `NpcVisionEvaluationTests`, harness P07 et `FB-013`.
- Remplace : aucune

### DEC-013 — Une configuration d'awareness partagée et une hystérésis explicite

- Date : `2026-08-15`
- Statut : `accepted`
- Lots concernés : `P08`, `P11`, `P13`
- Décideur : instruction utilisateur et devis P08
- Contexte : la suspicion, l'indicateur et les futurs gardes doivent conserver les mêmes valeurs sans que l'UI refasse de logique de détection.
- Décision : utiliser un `NpcAwarenessConfig` ScriptableObject comme source unique des seuils, vitesses et options de présentation ; modéliser les seuils d'entrée/sortie dans `NpcAwarenessModel` et faire publier les changements d'état par `NpcAwareness`.
- Conséquences : l'indicateur ne lit que l'état, l'alerte n'est annoncée qu'à une transition réelle, et les futurs consommateurs peuvent partager le même asset sans dépendre de l'UI.
- Preuves : `NpcAwarenessConfig.cs`, `NpcAwarenessModel.cs`, `NpcAwareness.cs`, `NpcAwarenessIndicator.cs`, `FB-014`.
- Remplace : aucune

### DEC-014 — Alerte de scène par instantané explicitement assigné

- Date : `2026-08-15`
- Statut : `accepted`
- Lots concernés : `P09`, `P11`, `P13`
- Décideur : instruction utilisateur et devis P09
- Contexte : l'alerte doit coordonner les gardes sans leur transmettre la position vivante du joueur ou ajouter un singleton caché.
- Décision : `AlertService` est un `MonoBehaviour` de scène référencé explicitement. Il mémorise seulement le premier `AlertSnapshot` immutable de son cycle Calm→Alerted et publie uniquement les changements réels de niveau. `NpcAlertReporter` écoute une awareness assignée et copie sa dernière vision au moment de l'alerte.
- Conséquences : les nouveaux consommateurs lisent le niveau actuel par leur référence ; aucun consommateur ne reçoit de Transform ou de suivi automatique. Reset remet Calm et vide le snapshot. Les comportements d'investigation restent hors P09.
- Preuves : `AI/Coordination/AlertService.cs`, `NpcAlertReporter.cs`, `AlertServiceTests.cs`, `FB-015` et compilations ciblées P09.
- Remplace : aucune

### DEC-015 — Behavior 1.0.13 embarqué avec compatibilité Unity 6000.5

- Date : `2026-08-15`
- Statut : `accepted`
- Lots concernés : `P10` et futurs graphes Behavior
- Décideur : version confirmée par l'utilisateur et contrainte de compilation observée
- Contexte : le package résolu `com.unity.behavior` 1.0.13 cible Unity 6000.0 et emploie dix appels répartis dans sept fichiers devenus des erreurs d'obsolescence avec Unity 6000.5.5f1. Modifier le cache `Library` ne serait ni durable ni partageable.
- Décision : conserver exactement la version 1.0.13 sous `Packages/com.unity.behavior`, adapter conditionnellement les sept fichiers concernés avec `UNITY_6000_5_OR_NEWER`, et ne référencer que l'assembly runtime `Unity.Behavior` depuis `GiscardPunk77.AI`.
- Conséquences : le correctif est versionnable et reproductible ; l'authoring reste isolé dans un outil Editor ; les avertissements internes non bloquants restent visibles. Une mise à jour officielle compatible devra remplacer l'embedding lors d'un lot explicitement autorisé.
- Preuves : `Packages/com.unity.behavior/package.json`, `Packages/packages-lock.json`, les huit asmdefs audités et `.agents/ingeesclave/REPORTS/P10.md`.
- Remplace : aucune

### DEC-016 — Patrouille ancrée au poste et alerte comme sortie de la routine P11

- Date : `2026-08-20`
- Statut : `accepted`
- Lots concernés : `P11`, `P13`
- Décideur : devis P11 et implémentation technique vérifiée
- Contexte : des points enfants de la capsule se déplaceraient avec elle, tandis que le combat futur doit pouvoir prendre la main sans laisser une route de patrouille active.
- Décision : écrire les points comme offsets relatifs à une position/orientation de poste capturée au démarrage ; laisser quatre nodes fins commander seulement `NpcMotor` ; traiter `AlertService.Alerted` comme une interruption immédiate qui annule le moteur et termine le graphe non combattant en `Failure`.
- Conséquences : chaque instance conserve une petite route réutilisable sans objets de scène cachés ; P13 pourra brancher sa priorité combat après l'échec d'alerte ; le prefab exige toujours une référence explicite au service de scène.
- Preuves : `GuardPatrolRoute.cs`, `GuardContext.cs`, graphe `P11 Guard Routine.asset`, prefab `Guard Capsule.prefab` et 28/28 tests AI Edit Mode.
- Remplace : aucune

## Modèle d'entrée

### DEC-XXX — Titre court

- Date : `YYYY-MM-DD`
- Statut : `proposed | accepted | superseded | rejected`
- Lots concernés : `PXX`, nom de module ou `transversal`
- Décideur : utilisateur, document canonique ou contrainte technique vérifiée
- Contexte : raison du choix
- Décision : formulation précise
- Conséquences : ce que ce choix autorise et interdit
- Preuves : fichiers, tests ou message utilisateur
- Remplace : identifiant éventuel


