# Ingéesclave — Feedback de GiscardPunk77

> Discussion durable entre l'utilisateur et l'agent. Ne pas stocker de secret.
> L'agent ne réécrit jamais le texte de `Retour utilisateur` : il complète seulement les champs de suivi.

## Règles

- Créer un identifiant unique `FB-XXX` par retour.
- Statuts autorisés : `new`, `acknowledged`, `planned`, `implemented`, `needs_decision`, `declined`.
- Tu peux écrire directement dans ce fichier ou transmettre ton retour en conversation ; l'agent le consignera au début du prochain lot.
- Un retour traité conserve son texte, sa date et son identifiant pour garder une trace vérifiable.

## Nouveaux retours

### FB-001 — Validation H0 de la scène d'intégration

- Date : `2026-08-13`
- Lot concerné : `P00`
- Statut : `implemented`
- Retour utilisateur :
  > En attente : confirmer ou infirmer que `Assets/Scenes/SampleScene.unity` est la scène jouable actuelle et sera la scène d'intégration PNJ au P16.
- Réponse agent : confirmation utilisateur reçue le 2026-08-13 : « confirmé ».
- Décision ou action : `SampleScene.unity` est acceptée comme scène jouable et future scène d'intégration PNJ au P16 ; P01 peut commencer.
- Comment vérifier : ouvrir `ProjectSettings/EditorBuildSettings.asset` et vérifier la scène active ; le résultat attendu après confirmation est que P01 pourra démarrer sur demande explicite, sans modifier de scène.

### FB-002 — Validation des tests P01

- Date : `2026-08-14`
- Lot concerné : `P01`
- Statut : `implemented`
- Retour utilisateur :
  > 5 test bon
- Réponse agent : les cinq tests Edit Mode sont enregistrés comme validés humainement et P01 passe à `complete`.
- Décision ou action : fermer P01 sans démarrer P02.
- Comment vérifier : ouvrir `REPORTS/P01.md` et `NPC_IMPLEMENTATION_STATUS.md`; les deux indiquent P01 `complete` et 5/5 tests validés.

### FB-003 — Compte rendu tâche par tâche

- Date : `2026-08-14`
- Lot concerné : `transversal`
- Statut : `implemented`
- Retour utilisateur :
  > je veux aussi que ingésclave réalise compte rendu tache par tache un systèema afin de voir ce qu'il a fait sur tel ou tel tâche
- Réponse agent : création d'un index global et d'un rapport détaillé séparé pour chaque lot.
- Décision ou action : rendre le rapport obligatoire à l'ouverture, aux checkpoints et à la passation de chaque lot.
- Comment vérifier : ouvrir `.agents/ingeesclave/REPORTS/INDEX.md`, puis les rapports P00, P01 et META-001.

### FB-004 — Fondation dégâts, santé et mort

- Date : `2026-08-14`
- Lot concerné : `P02`
- Statut : `implemented`
- Retour utilisateur :
  > Dans Gameplay/Combat, implémente un `DamageInfo` immutable contenant montant, point, direction, source et catégorie ; un contrat `IDamageable` ; un composant `Health` configurable ; et des événements de dommage/mort. La santé doit ignorer les dommages après la mort, borner ses valeurs et déclencher la mort exactement une fois. Ajoute un `DamageableHitbox` qui délègue vers une santé racine sans coder de multiplicateurs complexes. Crée une cible capsule de test dans le sandbox par un outil d'authoring idempotent ou un protocole manuel minimal. Ajoute des tests Edit Mode couvrant dégâts négatifs/nuls, sur-dégâts, mort unique, heal borné et reset.
  >
  > **Hors périmètre :** armure, membres, ragdoll, factions avancées.
  >
  > **Acceptation :** aucun consommateur ne calcule directement la santé ; 34+34+34 tue une cible à 100 ; la mort n'est publiée qu'une fois.
- Réponse agent : code P02 ajouté dans Gameplay/Combat ; l'outil Editor attend le sandbox P04 et ne modifie pas `SampleScene`. La confirmation de compilation et du Test Runner Unity reste attendue.
- Décision ou action : sept tests Edit Mode sont prêts ; attendre leur résultat avant de clôturer P02 et sans commencer P03.
- Comment vérifier : ouvrir `REPORTS/P02.md`, puis lancer l'assembly Edit Mode `GiscardPunk77.Gameplay.Tests` dans Unity.

### FB-005 — Documentation pédagogique visible

- Date : `2026-08-14`
- Lot concerné : `transversal`, `P02`
- Statut : `implemented`
- Retour utilisateur :
  > je souhaite que Ingésclave, fasse aussi des fichiers facilement ouver tou il explique son travail a des personne n'ayant pas forcèment connaissance afin que quelq'un lise, il apprenne comment ça marche et un coté scolaire et pédagogique avec des informations, des lecons sur des procédés
- Réponse agent : ajout d'un index visible et d'un guide par lot sous `Docs/INGEESCLAVE/`, plus des règles et un générateur qui les rendent obligatoires pour les futurs lots.
- Décision ou action : P02 sert de premier exemple complet ; son guide relie les responsabilités à de vrais fichiers, explique les procédés et distingue les validations des fonctionnalités futures.
- Comment vérifier : ouvrir `Docs/INGEESCLAVE/INDEX.md`, puis `P02.md`; vérifier les rubriques « En une phrase », « Vocabulaire utile », « La méthode employée », « Essayer sans risque » et « État de vérification ».

### FB-006 — Résolution hitscan et arme joueur

- Date : `2026-08-14`
- Lot concerné : `P03`
- Statut : `implemented`
- Retour utilisateur :
  > Crée dans Gameplay/Weapons un résolveur hitscan réutilisable qui raycaste avec LayerMask, ignore explicitement la hiérarchie du tireur, s'arrête au premier obstacle valide et transmet `DamageInfo` à `IDamageable`. Implémente une arme joueur semi-automatique séparée du prototype `CapsuleWeapon`, avec 8 coups, réserve limitée configurable, cadence, rechargement de 1,6 s et événements de présentation sans effets finaux. N'efface pas l'ancien prototype. Dans `NpcSandbox`, configure la nouvelle arme sur un joueur de test et plusieurs cibles derrière/devant une paroi. Ajoute les tests possibles et un protocole Play Mode pour murs, portes et hitbox.
- Réponse agent : P03 est implémenté, compilé sans erreur C# et arrêté à la porte H1 ; le prototype physique reste intact.
- Décision ou action : séparer résolveur, état de l’arme, orchestration, entrée et diagnostic ; fournir onze tests et un outil Editor idempotent limité au banc P03.
- Comment vérifier : ouvrir `REPORTS/P03.md` et `Docs/INGEESCLAVE/P03.md`, lancer les tests Edit Mode puis suivre le protocole mur/porte/hitbox/munitions. Reporter les résultats dans la conversation pour fermer H1.

### FB-007 — Validation des tests P03

- Date : `2026-08-14`
- Lot concerné : `P03`
- Statut : `implemented`
- Retour utilisateur :
  > Les tests sont validées.
- Réponse agent : le résultat Unity confirme 23/23 tests Edit Mode, dont 18/18 Gameplay ; P03 est clos.
- Décision ou action : fermer H1 puisque l’utilisateur demande explicitement l’ouverture de P04.
- Comment vérifier : ouvrir `REPORTS/P03.md` et le dernier `TestResults.xml` Unity.

### FB-008 — Sandbox wagon et NavMesh stable

- Date : `2026-08-14`
- Lot concerné : `P04`
- Statut : `implemented`
- Retour utilisateur :
  > Crée `NpcSandbox.unity` avec une géométrie simple représentant deux sections de wagon, un couloir étroit, une baie de croisement, une porte et deux positions de tir. Utilise AI Navigation déjà installé. Configure des surfaces stables et des liens explicites aux seuils si la géométrie les exige. Ajoute des gizmos pour zones marchables, liens, points d'attente et destinations. Préfère un outil Editor idempotent si la création automatisée de scène est nécessaire ; il ne doit modifier que sa racine générée. Documente les valeurs initiales de rayon, hauteur, vitesse et évitement d'un agent capsule.
- Réponse agent : scène, outil idempotent, réglages, probe, gizmos et tests sont livrés ; 27/27 tests passent et P04 s’arrête à H2.
- Décision ou action : générer uniquement une racine P04, conserver toute autre racine, séparer réglages, probe et gizmos, puis laisser le bake visible à la validation humaine.
- Comment vérifier : suivre `Docs/INGEESCLAVE/P04.md`, baker le `NavMeshSurface`, mesurer les largeurs et observer l’aller-retour de la capsule.

### FB-009 — Validation signalée et moteur de navigation P05

- Date : `2026-08-14`
- Lot concerné : `P04`, `P05`
- Statut : `implemented`
- Retour utilisateur :
  > Tout les tests sont bons 
  >
  > [$ingeesclave](C:\\Users\\simon\\.codex\\skills\\ingeesclave\\SKILL.md) > Implémente `NpcMotor` comme propriétaire unique du `NavMeshAgent`. Expose des commandes bornées : destination, arrêt, rotation vers cible, état de chemin, arrivée, annulation et tentative de recalcul. Ajoute un statut explicite `Idle/Moving/Waiting/Arrived/Failed` et des raisons d'échec. Aucun Behavior node ne doit manipuler directement `NavMeshAgent`. Ajoute une détection de stagnation à basse fréquence et un recalcul limité sans téléportation active par défaut. Teste destination valide, invalide, annulation, désactivation et reprise.
- Réponse agent : P04 est autorisé à laisser place à P05. `NpcMotor`, la migration du probe et quatre tests Play Mode ont été ajoutés ; l'exécution automatique des tests reste à faire dans un éditeur Unity disposant d'une licence active.
- Décision ou action : ne pas commencer P06 ; traiter l'accès `NavMeshAgent` comme une responsabilité exclusive de `NpcMotor`.
- Comment vérifier : suivre le protocole de `Docs/INGEESCLAVE/P05.md`, puis répondre avec le nombre de tests verts ou le texte exact d'une erreur.

### FB-010 — Validation P05 et réservation de porte P06

- Date : `2026-08-14`
- Lot concerné : `P05`, `P06`
- Statut : `implemented`
- Retour utilisateur :
  > **tOUS LES TESTS SONT PASSS2S** 
  >
  > [$ingeesclave](C:\\Users\\simon\\.codex\\skills\\ingeesclave\\SKILL.md) 
  > **Objectif :** empêcher plusieurs agents de se coincer et découpler `SlidingDoor` des PNJ. 
  >
  > **Prompt :**
  >
  > > Extrais de `SlidingDoor` une API de passage commune sans casser l'interaction joueur : `CanUse`, `RequestOpen`, `IsPassable`, `TryReserve`, `Release` et événements d'état. Implémente une réservation FIFO simple avec propriétaire, expiration et deux points d'attente configurables. Crée `NpcDoorTraversal` dans AI : réserver, attendre, demander ouverture, traverser, libérer. Une mort, un disable, un reset ou un timeout doit toujours libérer la ressource. Teste deux puis quatre capsules venant des deux côtés. Les portes restent sans verrouillage ni droits avancés.
- Réponse agent : P05 est clos par la validation utilisateur. P06 implémente le contrat de porte, la file FIFO expirante, la traversée AI, quatre capsules de stress et s'arrête à la porte humaine H2.
- Décision ou action : conserver l'interaction joueur de `SlidingDoor`; placer le contrat et la réservation dans Gameplay ; faire dépendre AI uniquement de `IDoorPassage`.
- Comment vérifier : lancer les tests Edit/Play Mode puis suivre `Docs/INGEESCLAVE/P06.md` jusqu'au message Console confirmant 20 traversées.

### FB-011 — Deux échecs Play Mode et protocole H2 à clarifier

- Date : `2026-08-14`
- Lot concerné : `P06`
- Statut : `implemented`
- Retour utilisateur :
  > Voici les erreirs dans le test run
  >
  > Je ne comprend pas cela :
  >
  > Lance GiscardPunk77.Gameplay.Tests en Edit Mode.
  > Lance GiscardPunk77.AI.PlayMode.Tests en Play Mode.
  > Exécute Tools > GiscardPunk77 > P06 > Create or Update Door Stress Sandbox.
  > Ouvre NpcSandbox.unity.
  > Sélectionne P04 Navigation Sandbox Generated et clique sur Bake.
  > Active Gizmos : les points jaunes sont les attentes et le trait magenta représente le seuil.
  > Passe en Play Mode et observe P06 Door Traversal Stress.
  > Dans SampleScene, vérifie aussi qu’une porte visée répond toujours à E.
- Réponse agent : les deux pièces jointes sont des copies de rapports XML sans feuille de style, pas des pages d'erreur. Elles montrent deux exécutions Play Mode identiques à `8/10` : les scénarios à deux et quatre capsules échouent avec `RecalculationLimitReached`, puis `MotorRejectedPath`. Les huit autres tests passent.
- Décision ou action : la cause source est confirmée et corrigée par des dégagements propres à chaque agent sans contourner `NpcMotor`; les deux tests rouges doivent maintenant être rejoués. Ne pas commencer P07 et ne pas demander le Bake tant que le Play Mode n'est pas vert.
- Comment vérifier : dans `Window > General > Test Runner`, ouvrir l'onglet PlayMode, sélectionner `GiscardPunk77.AI.PlayMode.Tests`, puis `Run Selected`; le lot ne retourne à H2 que lorsque les 10 tests sont verts.

### FB-012 — Reproduction textuelle des deux échecs opposés

- Date : `2026-08-14`
- Lot concerné : `P06`
- Statut : `implemented`
- Retour utilisateur :
  > /
  >
  > FourCapsulesFromOppositeSidesAllComplete (20,025s)
  > ---
  >   Expected: True
  >   But was:  False
  >
  > ---
  > at GiscardPunk77.AI.PlayMode.Tests.NpcDoorTraversalPlayModeTests+<RunOpposedScenario>d__11.MoveNext () [0x00126] in Assets\_Project\Tests\PlayMode\AI\NpcDoorTraversalPlayModeTests.cs:163
  > at UnityEngine.TestTools.TestEnumerator+<Execute>d__7.MoveNext () [0x0003a] in Library\PackageCache\com.unity.test-framework@1405238725ab\UnityEngine.TestRunner\NUnitExtensions\Attributes\TestEnumerator.cs:44
  >
  > ---
  > NpcMotor failed: RecalculationLimitReached.
  > NpcDoorTraversal failed: MotorRejectedPath.
  > NpcMotor failed: RecalculationLimitReached.
  > NpcDoorTraversal failed: MotorRejectedPath.
  >
  > TwoCapsulesFromOppositeSidesCompleteInOrder (12,009s)
  > ---
  >   Expected: True
  >   But was:  False
  >
  > ---
  > at GiscardPunk77.AI.PlayMode.Tests.NpcDoorTraversalPlayModeTests+<RunOpposedScenario>d__11.MoveNext () [0x00126] in Assets\_Project\Tests\PlayMode\AI\NpcDoorTraversalPlayModeTests.cs:163
  > at UnityEngine.TestTools.TestEnumerator+<Execute>d__7.MoveNext () [0x0003a] in Library\PackageCache\com.unity.test-framework@1405238725ab\UnityEngine.TestRunner\NUnitExtensions\Attributes\TestEnumerator.cs:44
  >
  > ---
  > NpcMotor failed: RecalculationLimitReached.
  > NpcDoorTraversal failed: MotorRejectedPath.
- Réponse agent : la destination finale partagée a été remplacée dans le stress par deux dégagements propres à chaque agent. L'assertion finale reste stricte et fournit désormais l'état, l'échec, la position et la destination de chaque capsule si le timeout revient.
- Décision ou action : conserver le ticket jusqu'au dégagement, vérifier explicitement l'ordre FIFO et faire rejouer les deux tests nommés ; ne pas commencer P07.
- Comment vérifier : relancer d'abord les deux tests nommés dans PlayMode ; ils doivent être verts sans `RecalculationLimitReached` ni `MotorRejectedPath`.

### FB-013 — Validation des tests P06 et demande de perception visuelle P07

- Date : `2026-08-15`
- Lot concerné : `P06`, `P07`
- Statut : `implemented`
- Retour utilisateur :
  > Tous les tests passent,
  >
  > Implémente une signature de visibilité sur le joueur à partir de `IsCrouching`, sans coupler AI à `PlayerController`. Implémente `NpcVisionSensor` : distance, angle lié au regard, raycast d'occultation, temps d'exposition et coefficient de posture. La vision produit une observation structurée ; elle ne change pas directement l'état du garde. Un joueur accroupi doit exiger une distance plus courte ou une exposition plus longue. Échantillonne à fréquence configurable, répartie entre agents. Ajoute gizmos pour cône, cible testée, rayon bloqué et dernière observation. Ajoute des tests de fonctions de score et un protocole Play Mode pour face/dos, debout/accroupi et mur.
- Réponse agent : la correction P06 est enregistrée comme validée par tous les tests Unity. Le lot P07 est ouvert sur la branche perception ; la vérification visuelle H2 du stress de porte et de la touche E reste distinguée des tests puisqu'elle n'a pas été rapportée explicitement.
- Décision ou action : implémenter uniquement P07 avec un calcul pur testable, un capteur AI consommant `IVisibilityTarget`, une observation immutable, un échantillonnage réparti et un banc visuel isolé dans `NpcSandbox`.
- Comment vérifier : lancer les tests Edit Mode `GiscardPunk77.AI.Tests`, générer le banc P07, puis suivre le protocole face/dos, debout/accroupi et mur décrit dans `Docs/INGEESCLAVE/P07.md`.

### FB-014 — Validation P07 et demande P08

- Date : `2026-08-15`
- Lot concerné : `P07`, `P08`
- Statut : `implemented`
- Retour utilisateur :
  > Les tests passent 
  >
  > Implémente `NpcAwareness` séparé du capteur. Il accumule/décroît une suspicion normalisée, passe par `Unaware/Suspicious/Alerted`, conserve la dernière position et l'heure de dernière vision. Ajoute une hystérésis pour éviter les oscillations. Implémente `NpcAwarenessIndicator` comme présentation indépendante : caché au calme, signal de suspicion facultatif, puis `!` à l'alerte. Il doit suivre la caméra sans contenir de logique de détection. Toutes les valeurs viennent d'une configuration partagée. Teste seuils, décroissance, perte de vue et reset.
- Réponse agent : la validation des tests P07 est enregistrée. P08 est ouvert ; la vision reste productrice d'observations, tandis que la suspicion et sa présentation résident dans des composants distincts.
- Décision ou action : implémenter exclusivement la configuration partagée, le modèle d'hystérésis, l'awareness, l'indicateur, les tests et un protocole sandbox isolé.
- Comment vérifier : lancer les tests AI Edit Mode, générer le banc P08, puis observer le passage unique vers `!`, la mémoire figée après perte de vue et le retour calme sans oscillation.

### FB-015 — Validation P08 et demande de service d'alerte P09

- Date : `2026-08-15`
- Lot concerné : `P08`, `P09`
- Statut : `implemented`
- Retour utilisateur :
  > Les tests passent 
  >
  > Implémente un `AlertService` de scène explicitement référencé. Lorsqu'un garde devient alerté, le service diffuse `Calm/Alerted` et un instantané facultatif de l'observation initiale. Il ne suit jamais le Transform du joueur et ne met jamais à jour sa position automatiquement. Les nouveaux gardes peuvent lire le niveau courant. Le service se réinitialise proprement et ne doit pas être un singleton statique. Ajoute tests d'abonnement/désabonnement, broadcast unique et reset.
- Réponse agent : la validation P08 est enregistrée. P09 fournit un composant de scène et un adaptateur explicite depuis l'awareness, sans recherche globale, singleton ou suivi de Transform.
- Décision ou action : `AlertLevel`, `AlertSnapshot`, `AlertService`, `NpcAlertReporter`, quatre tests Edit Mode et un outil sandbox P09 ont été ajoutés.
- Comment vérifier : lancer les tests AI Edit Mode puis créer P08 et P09 dans le sandbox. Le service doit passer une seule fois à `Alerted`, conserver son premier snapshot et revenir proprement à `Calm` avec `P09/Reset Alert`.

### FB-016 — Validation P09 et demande du socle Unity Behavior P10

- Date : `2026-08-15`
- Lot concerné : `P09`, `P10`
- Statut : `implemented`
- Retour utilisateur :
  > Les tests sont validées 
  >
  > Installe la version stable de com.unity.behavior compatible avec l'éditeur actuel via Package Manager, sans modifier les autres versions de packages. Inspecte les asmdefs exacts du package avant de référencer l'assemblage AI. Crée NpcContext, qui référence explicitement identité, santé, moteur, perception, awareness et alert service. Crée seulement les conditions/actions Behavior génériques nécessaires pour lire un état, attendre, choisir une destination, se déplacer et interrompre sur mort/alerte. Les nodes délèguent aux composants et ne font ni raycast ni dommage. Crée un graphe minimal de test dans l'éditeur et documente comment l'ouvrir/déboguer. Ajoute un fallback d'erreur clair si une référence manque.
- Réponse agent : la validation P09 est enregistrée. P10 est livré seul avec audit des assemblies, contexte explicite, nodes minces, outil de graphe, tests et guide ; H4 reste à confirmer.
- Décision ou action : conserver `NpcMotor`, perception, awareness, alerte et santé comme propriétaires de leur logique ; Behavior ne fait que lire, commander et attendre.
- Comment vérifier : confirmer 1.0.13 Embedded dans Package Manager, lancer les tests, ouvrir le graphe P10, observer son exécution et vérifier qu'un moteur désactivé provoque un échec lisible.

### FB-017 — Résolution de Behavior 1.0.13 et reprise P10

- Date : `2026-08-15`
- Lot concerné : `P10`
- Statut : `implemented`
- Retour utilisateur :
  > On a bien Behavioral 1.0.13
  >
  > Reprends P10 après résolution du package.
- Réponse agent : la version résolue est enregistrée. Les asmdefs ont été audités, la couche Behavior minimale et son outil de graphe sont livrés, puis P10 s'arrête à H4 sans ouvrir P11.
- Décision ou action : référencer seulement `Unity.Behavior` depuis AI ; embarquer la même version 1.0.13 avec les adaptations `EntityId`/drag-and-drop reproductibles pour Unity 6000.5 ; fournir contexte, nodes, cinq tests et protocole de debug.
- Comment vérifier : suivre `Docs/INGEESCLAVE/P10.md`; Package Manager doit afficher 1.0.13 Embedded, les tests doivent être verts, le graphe doit s'ouvrir et le moteur désactivé doit produire un `Failure` explicite.

### FB-018 — Revue de démo et guide daté

- Date : `2026-08-15`
- Lot concerné : `transversal`, `P03`–`P10`
- Statut : `implemented`
- Retour utilisateur :
  > dans le présent, je souhaite que tu fasses une petite revue de ce que je peux déja utilsie afin de faire sous démo car je dois attendre token recharge, ce qui est déja tilisable.... Je souahite que tu écrives un guide pour la date d'aujjourd'hui
- Réponse agent : le guide daté distingue la scène jouable, les bancs techniques déjà disponibles et les fonctions qui restent en validation.
- Décision ou action : présenter `SampleScene` pour le FPS et les portes, puis `NpcSandbox` pour les bancs P03 à P09 ; ne pas présenter P10 comme validé avant H4.
- Comment vérifier : ouvrir `Docs/INGEESCLAVE/DEMO-2026-08-15.md` et suivre le parcours choisi.

### FB-019 — Demande de boucle non combattante Guard P11

- Date : `2026-08-20`
- Lot concerné : `P11`
- Statut : `implemented`
- Retour utilisateur :
  > Crée `GuardConfig`, `GuardContext` si une spécialisation est nécessaire, des points de patrouille écrits et des nodes fins pour Idle, Patrol, Suspicious et InvestigateLastKnownPosition. Assemble un graphe Guard réutilisant le socle P10. En cas de suspicion, le garde peut s'orienter ou rejoindre la dernière position ; après un délai court sans contact, il revient à son poste. Une alerte globale interrompt immédiatement la routine. Ajoute un prefab capsule garde avec couleurs d'état temporaires et historique court des transitions.
- Réponse agent : P11 fournit la configuration, le contexte spécialisé, trois points écrits, quatre nodes fins, le graphe, le prefab et les diagnostics demandés. Les validations automatiques passent ; la boucle visuelle de dix minutes reste à confirmer à H4.
- Décision ou action : ancrer les points au poste capturé, déléguer tout mouvement à `NpcMotor`, annuler la route sur événement global et conserver huit transitions récentes sans modifier de scène.
- Comment vérifier : suivre `Docs/INGEESCLAVE/P11.md`; rapporter la boucle bleu/vert, l'interruption jaune, l'investigation/retour orange, l'alerte rouge et l'absence de destination invalide pendant dix minutes.

## Modèle d'entrée

### FB-XXX — Titre court

- Date : `YYYY-MM-DD`
- Lot concerné : `PXX` ou `transversal`
- Statut : `new`
- Retour utilisateur :
  > Texte original du retour.
- Réponse agent : à compléter sans modifier le retour utilisateur.
- Décision ou action : à compléter selon le retour.
- Comment vérifier : à compléter avec le point d'entrée, les actions et le résultat attendu.
