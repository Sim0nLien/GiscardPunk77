# GiscardPunk 1977 — Devis modulaire de prompts PNJ

> **Source de décision :** `NPC_ETAT_DE_L_ART.md`, relu avec les choix et annotations utilisateur.
>
> **Nature du devis :** découpage précis des futures interventions Codex. Ce document ne lance pas encore l'implémentation.
>
> **Objectif :** obtenir une première démo stable avec gardes et civils simples, tout en empêchant la logique de perception, navigation, combat et présentation de se mélanger dans un script monolithique.

## 1. Périmètre retenu

### 1.1 Interprétation des décisions

Certaines cases cochées décrivent une ambition de long terme, tandis que les annotations dans le texte réduisent explicitement la première démo. Le devis applique donc deux horizons.

| Décision | Interprétation pour la démo | Extension conservée pour plus tard |
|---|---|---|
| D1-A | démo de combat stricte | tranche systémique après stabilisation |
| D2-B | jusqu'à 4 gardes et 2–4 civils présents | foule supérieure à 8 exclue |
| D3-B + annotation E1 | **E1+** : combat E1, suspicion visuelle et dernière position limitée | recherche E2 complète, corps et appel d'alarme |
| D4-A + annotation C1 | civils C0/C1 : assis/fument, regardent, réagissent et peuvent mourir | fuite, cachettes et témoins C2/C3 |
| D5-D + « long long terme » | contrat de mort civile et événement d'incident seulement | réputation, renforts, narration et conséquences persistantes |
| D6-B | Unity Behavior pour orchestrer les décisions | GOAP réservé à une version avancée |
| D7-B + annotation vision | vue, posture, suspicion, dernière position ; bruit de tir seulement | lumière, diversions sonores riches, camouflage et déguisement |
| D8-A | niveau d'alerte global instantané | aucun partage permanent de la position exacte du joueur |
| D9-B | porte commune aux joueurs/PNJ et réservation des seuils | verrouillage, panne et droits complets |
| D10-A | capsules et feedback procédural | humanoïdes, IK et animations plus tard |
| D11-E | LLM hors ligne comme lot R&D indépendant | aucune dépendance du build de la démo envers un LLM |
| D12-A | comportement avant art | passe artistique après validation |
| D13-A | une représentation capsule pour garde et civil | kit modulaire non compris |
| D14 non choisi | aucun dialogue produit ; hooks audio seulement | barks ou dialogue à décider |
| D15 non choisi | aucun second archétype | radiotéléphoniste/automate à décider après stabilisation |

### 1.2 Résultat jouable attendu

La tranche terminée devra permettre ceci :

1. Le joueur entre dans un wagon contenant jusqu'à quatre gardes capsules et quelques civils simples.
2. Un garde détecte mieux un joueur debout et proche de son regard qu'un joueur accroupi.
3. Une jauge discrète ou un symbole de suspicion apparaît, puis un `!` signale l'alerte.
4. À l'alerte, tous les gardes connaissent l'état d'alerte global, mais pas une position mise à jour télépathiquement.
5. Chaque garde rejoint une position de tir écrite, anticipe sa rafale, tire sans traverser les murs, récupère, peut perdre le contact et enquêter brièvement sur la dernière position connue.
6. Les gardes ouvrent et franchissent les portes sans se bloquer durablement entre eux.
7. Un civil assis ou en train de fumer regarde l'événement, joue une réaction simple et peut recevoir des dégâts/mourir.
8. La mort d'un civil publie un incident exploitable plus tard, sans construire maintenant réputation ou narration.
9. Mort, pause et restart ne laissent aucun agent, réservation ou état global fantôme.

### 1.3 Hors périmètre immédiat

- GOAP, HTN, Utility AI générale et apprentissage par renforcement ;
- simulation sociale ou besoins quotidiens ;
- témoins, rumeurs, réputation et renforts narratifs ;
- couverture découverte dynamiquement ;
- flanking d'escouade et rôles tactiques ;
- perception réelle de la lumière ;
- déguisements et habilitations ;
- foule massive, LOD avancé et simulation hors écran ;
- personnage humanoïde final, ragdoll, IK et motion capture ;
- dialogue, voix et sous-titres ;
- LLM dans le build ou appel réseau en jeu ;
- deuxième archétype ennemi.

## 2. Décisions techniques structurantes

### 2.1 Unity Behavior orchestre, il ne contient pas le jeu

Le graphe de comportement choisira une intention et appellera des nœuds courts. Il ne devra pas :

- lancer directement tous les raycasts de vision ;
- déplacer directement le `Transform` ;
- calculer les dégâts ;
- ouvrir une porte en modifiant son booléen privé ;
- gérer lui-même les effets visuels ;
- contenir des délais dispersés non configurables.

Ces responsabilités appartiennent à des composants C# testables. Le graphe lira leur état et appellera leurs méthodes. Un remplacement futur du Behavior Tree par GOAP ne forcera donc pas à réécrire navigation, perception ou combat.

### 2.2 Navigation choisie

**Choix :** AI Navigation Unity déjà installé, surfaces stables par zone de train, `NavMeshLink` explicites aux transitions délicates, `NavMeshAgent` pour le mouvement, évitement natif et réservation écrite des seuils.

**Pourquoi ce choix tient la route :**

- le train jouable reste immobile, donc aucun recalcul permanent n'est nécessaire ;
- Unity gère le chemin global et les obstacles ordinaires ;
- les seuils et portes, source principale de bugs, reçoivent une règle explicite ;
- le système est visible avec des gizmos et reproductible ;
- quatre gardes ne justifient ni ORCA personnalisé ni système de foule ;
- les positions de tir et points d'attente écrits sont plus fiables dans trois wagons étroits.

**Règle de porte :** l'agent réserve un passage, rejoint son point d'attente, demande l'ouverture, attend `IsPassable`, traverse, puis libère la réservation. Toute réservation expire. L'évitement NavMesh reste un complément, pas le mécanisme de priorité.

**Récupération :** après plusieurs échecs, l'agent annule et recalcule. Une éventuelle téléportation hors caméra sera derrière une option désactivée en développement afin de ne pas masquer les bugs.

### 2.3 Assemblages et dépendances

Trois assemblages runtime suffisent. Une multiplication d'assemblages serait une autre forme de complexité.

```text
GiscardPunk77.Core
  └─ données primitives, identité, interfaces sans dépendance gameplay

GiscardPunk77.Gameplay -> Core
  └─ dégâts, santé, hitscan, portes et réservations

GiscardPunk77.AI -> Core + Gameplay + AI Navigation + Unity Behavior
  └─ moteur PNJ, perception, mémoire, graph nodes, gardes, civils, debug

Tests -> assemblage ciblé
```

Le code existant dans `Assembly-CSharp` peut consommer ces assemblages. L'inverse est interdit : les nouveaux modules ne doivent pas dépendre directement d'un script global existant. Un petit adaptateur reliera par exemple `PlayerController.IsCrouching` à l'interface de visibilité du module Core.

### 2.4 Arborescence cible

Les scripts existants ne seront pas déplacés pendant cette tranche afin de préserver leurs GUID et les scènes utilisateur.

```text
Assets/_Project/
  Config/
    AI/
    Combat/
  Prefabs/
    Characters/
    Gameplay/
  Scenes/
    Tests/
      NpcSandbox.unity
  Scripts/
    Core/
      Actors/
      Visibility/
    Gameplay/
      Combat/
      Interaction/
      Weapons/
    AI/
      Shared/
      Navigation/
      Perception/
      Memory/
      Behavior/
        Nodes/
      Guards/
      Civilians/
      Debug/
    Editor/
  Tests/
    EditMode/
    PlayMode/
```

### 2.5 Règles anti-spaghetti obligatoires

- une classe a une responsabilité exprimable en une phrase ;
- aucune classe `NpcManager` universelle ;
- aucun accès direct du garde aux champs privés du joueur ou de la porte ;
- interfaces uniquement aux frontières utiles, pas une interface par classe ;
- événements pour les changements rares, pas pour remplacer tous les appels de méthode ;
- aucune recherche de composant ou d'objet à chaque `Update` ;
- pas de singleton statique contenant l'état mutable des PNJ ;
- les données réglables et partagées vont dans des `ScriptableObject` de configuration ;
- les états runtime restent sur les instances, jamais dans les assets de configuration ;
- les nœuds Behavior restent minces et délèguent aux composants ;
- le cerveau ne pilote jamais l'Animator directement ;
- chaque délai utilise une horloge cohérente et respecte pause/restart ;
- toute réservation et tout abonnement à un événement sont libérés dans `OnDisable`/mort/reset ;
- aucun script de production ne dépasse environ 300 lignes sans justification ;
- aucun prompt ne modifie `SampleScene.unity` avant le lot d'intégration ;
- les assets utilisateur sales ou sans rapport sont conservés.

## 3. Devis global

### 3.1 Échelle

| Taille | Travail attendu | Tours Codex usuels |
|---|---|---:|
| S | composant borné, peu d'intégration | 1 |
| M | plusieurs classes et tests | 1–2 |
| L | système + scène/prefab + validation Play Mode | 2–3 |

Un « tour » inclut inspection, modification et vérification. Il ne garantit pas qu'un réglage visuel dans l'éditeur soit correct sans validation humaine.

### 3.2 Total estimatif

- **20 prompts principaux** numérotés `P00` à `P19`.
- **30 à 47 tours Codex** en incluant compilation, réglages et corrections issues des validations Unity.
- **8 portes de validation humaine** courtes, numérotées `H0` à `H7`.
- **12 à 20 sessions de développement concentrées** pour la tranche complète, hors création d'art, animation et audio.
- **4 lots futurs** séparés et non bloquants.

La marge vient surtout de la navigation dans la géométrie réelle, de l'installation/authoring Unity Behavior et de la sérialisation des scènes/prefabs.

Répartition indicative :

| Phase | Prompts | Tours estimés |
|---|---|---:|
| baseline et combat partagé | P00–P03 | 5–8 |
| navigation, portes et perception | P04–P09 | 9–14 |
| Unity Behavior et garde | P10–P13 | 8–12 |
| civils, intégration et cycle de vie | P14–P17 | 5–8 |
| durcissement et livraison | P18–P19 | 3–5 |
| **Total** | **20 prompts** | **30–47** |

### 3.3 Tableau des prompts

| ID | Module | Taille | Dépend de | Porte humaine |
|---|---|---:|---|---|
| P00 | baseline, scènes et périmètre | S | — | H0 |
| P01 | frontières de modules | M | P00 | — |
| P02 | contrat de dégâts et santé | M | P01 | — |
| P03 | résolution hitscan et arme joueur | L | P02 | H1 |
| P04 | sandbox et NavMesh | L | P01 | H2 |
| P05 | moteur de navigation PNJ | M | P04 | H2 |
| P06 | portes et réservation | L | P05 | H2 |
| P07 | signature de visibilité et vision | L | P01 | H3 |
| P08 | suspicion, mémoire et indicateur | M | P07 | H3 |
| P09 | alerte globale bornée | S | P08 | — |
| P10 | installation et socle Unity Behavior | L | P05, P08, P09 | H4 |
| P11 | garde : repos, patrouille, investigation | L | P10 | H4 |
| P12 | combat à distance modulaire | L | P02, P03, P11 | H5 |
| P13 | garde E1+ complet | L | P06, P09, P12 | H5 |
| P14 | civil C0/C1 | M | P02, P07, P10 | H6 |
| P15 | incident de mort civile | S | P14 | H6 |
| P16 | rencontre et intégration scène réelle | L | P13, P15 | H6 |
| P17 | cycle pause/mort/restart | M | P16 | H7 |
| P18 | debug, tests de stress et performance | L | P17 | H7 |
| P19 | audit final et documentation | S | P18 | H7 |

### 3.4 Risques et provisions

| Risque | Probabilité | Impact | Provision du devis |
|---|---:|---:|---|
| Unity Behavior incompatible ou graph assets difficiles à automatiser | moyenne | fort | P10 dispose de 2–3 tours et d'une porte Editor ; arrêter plutôt que fabriquer un autre cerveau en douce |
| `SampleScene` contient des changements utilisateur non consolidés | forte | fort | aucune modification avant P16 ; audit P00 et intégration sous une racine isolée |
| NavMesh trop étroit autour des sièges/portes | forte | fort | sandbox P04, moteur P05, réservation P06 et porte H2 avant tout cerveau |
| quatre gardes rendent les tirs injustes | moyenne | fort | anticipation, positions écrites et réglage H5 ; pas de tactique supplémentaire |
| les civils C1 paraissent artificiels sous attaque | forte | moyen | périmètre assumé et documenté ; C2 reste un lot futur |
| scènes/prefabs resérialisés massivement par Unity | moyenne | moyen | outils idempotents, diff inspecté et changements de scène réservés aux prompts dédiés |
| tests Play Mode instables | moyenne | moyen | logique pure testée en Edit Mode, harness déterministe et validations humaines séparées |
| dérive vers réputation/LLM/GOAP | moyenne | fort | coutures minimales seulement ; lots F01–F03 interdits avant P19 |

## 4. Préambule commun à tous les prompts

Le texte suivant devra accompagner chaque prompt d'implémentation :

> Lis `NPC_IMPLEMENTATION_CONTRACT.md`, la section du prompt courant dans `NPC_DEVIS_DE_PROMPTS.md` et le dernier état d'avancement, puis inspecte le code et `git status`. Consulte le backbone, la roadmap ou l'état de l'art seulement si le lot rencontre une ambiguïté de produit. Préserve toutes les modifications utilisateur sans rapport. Ne réalise que le lot demandé et ses corrections nécessaires. Utilise les modules et namespaces prévus, sans créer de gestionnaire monolithique, de singleton mutable ou de dépendance circulaire. Ajoute les tests et outils de debug demandés. Compile et exécute les tests disponibles ; si une validation Unity manuelle reste nécessaire, fournis un protocole exact. Ne modifie pas une scène de production avant le prompt qui l'autorise. Termine par la liste des fichiers changés, les vérifications effectuées et les limites restantes, puis actualise l'état d'avancement compact.

## 5. Prompts détaillés

## P00 — Figer le baseline et l'autorité des scènes

**Objectif :** éviter de bâtir l'IA dans la mauvaise scène.

**Constat actuel :** `SampleScene.unity` contient le train et est enregistrée dans les Build Settings ; `Train.unity` ne contient actuellement qu'une caméra et une lumière.

**Prompt :**

> Audite les scènes `SampleScene` et `Train`, les Build Settings, les packages et les composants de gameplay existants. Ne renomme, ne déplace et n'écrase aucune scène. Documente `SampleScene` comme scène d'intégration provisoire de cette tranche et `NpcSandbox` comme future scène de test. Crée `NPC_IMPLEMENTATION_CONTRACT.md`, synthèse courte des décisions figées, règles architecturales, scène autoritaire et critères communs, ainsi qu'un état d'avancement compact. Ces deux fichiers remplaceront la relecture systématique des quatre gros documents aux prompts suivants. Mets à jour uniquement les documents de décision nécessaires avec : scène autoritaire, composants existants à préserver, divergence projectile/hitscan et liste des dépendances PNJ. Ne code encore aucun PNJ.

**Livrables :** décision de scène écrite, inventaire des composants, contrat compact, état d'avancement, aucun changement de gameplay.

**Acceptation :** aucune scène resérialisée ; le document indique sans ambiguïté où tester puis où intégrer.

**Estimation :** S, 1 tour.

## P01 — Créer les frontières de modules

**Objectif :** créer la structure compilable avant les systèmes.

**Prompt :**

> Crée sous `Assets/_Project` l'arborescence Core, Gameplay, AI, Tests et Config prévue par le devis. Ajoute les trois asmdefs runtime avec dépendances unidirectionnelles, les namespaces `GiscardPunk77.Core`, `.Gameplay` et `.AI`, puis les types minimaux `ActorKind`, `TeamId`, `ActorIdentity` et `IVisibilityTarget`. N'ajoute aucun système générique non requis. Modifie `PlayerController` seulement pour exposer proprement sa posture via un petit adaptateur ou l'interface Core, sans changer son mouvement. Ajoute des tests de compilation/valeurs primitives adaptés aux asmdefs.

**Fichiers attendus :** asmdefs, identité acteur, contrat de visibilité, tests associés.

**Acceptation :** compilation sans dépendance circulaire ; AI peut référencer Gameplay/Core ; Core ne référence aucun autre module du projet ; le joueur expose debout/accroupi.

**Estimation :** M, 1–2 tours.

## P02 — Fondation dégâts, santé et mort idempotente

**Objectif :** fournir un contrat unique à l'arme, aux gardes et aux civils.

**Prompt :**

> Dans Gameplay/Combat, implémente un `DamageInfo` immutable contenant montant, point, direction, source et catégorie ; un contrat `IDamageable` ; un composant `Health` configurable ; et des événements de dommage/mort. La santé doit ignorer les dommages après la mort, borner ses valeurs et déclencher la mort exactement une fois. Ajoute un `DamageableHitbox` qui délègue vers une santé racine sans coder de multiplicateurs complexes. Crée une cible capsule de test dans le sandbox par un outil d'authoring idempotent ou un protocole manuel minimal. Ajoute des tests Edit Mode couvrant dégâts négatifs/nuls, sur-dégâts, mort unique, heal borné et reset.

**Hors périmètre :** armure, membres, ragdoll, factions avancées.

**Acceptation :** aucun consommateur ne calcule directement la santé ; 34+34+34 tue une cible à 100 ; la mort n'est publiée qu'une fois.

**Estimation :** M, 1–2 tours.

## P03 — Résolution hitscan et arme du joueur

**Objectif :** remplacer la dépendance aux capsules physiques pour tester correctement les PNJ.

**Prompt :**

> Crée dans Gameplay/Weapons un résolveur hitscan réutilisable qui raycaste avec LayerMask, ignore explicitement la hiérarchie du tireur, s'arrête au premier obstacle valide et transmet `DamageInfo` à `IDamageable`. Implémente une arme joueur semi-automatique séparée du prototype `CapsuleWeapon`, avec 8 coups, réserve limitée configurable, cadence, rechargement de 1,6 s et événements de présentation sans effets finaux. N'efface pas l'ancien prototype. Dans `NpcSandbox`, configure la nouvelle arme sur un joueur de test et plusieurs cibles derrière/devant une paroi. Ajoute les tests possibles et un protocole Play Mode pour murs, portes et hitbox.

**Hors périmètre :** recul final, son, muzzle flash final, inventaire.

**Acceptation :** la cible devant le mur prend les dégâts ; celle derrière non ; aucun tir pendant reload ; les munitions ne deviennent jamais négatives.

**Porte humaine H1 :** vérifier tir, rechargement et occultation dans le sandbox.

**Estimation :** L, 2–3 tours.

## P04 — Construire le sandbox et le NavMesh stable

**Objectif :** valider la navigation sans toucher la scène principale.

**Prompt :**

> Crée `NpcSandbox.unity` avec une géométrie simple représentant deux sections de wagon, un couloir étroit, une baie de croisement, une porte et deux positions de tir. Utilise AI Navigation déjà installé. Configure des surfaces stables et des liens explicites aux seuils si la géométrie les exige. Ajoute des gizmos pour zones marchables, liens, points d'attente et destinations. Préfère un outil Editor idempotent si la création automatisée de scène est nécessaire ; il ne doit modifier que sa racine générée. Documente les valeurs initiales de rayon, hauteur, vitesse et évitement d'un agent capsule.

**Choix interdit :** NavMesh rebâti chaque frame, ORCA personnalisé, déplacement direct du Transform.

**Acceptation :** un agent Unity de test peut atteindre chaque destination de part et d'autre de la porte ouverte ; aucun warning NavMesh récurrent.

**Porte humaine H2 :** bake/visualisation du NavMesh et vérification des largeurs réelles.

**Estimation :** L, 2–3 tours.

## P05 — Moteur de navigation PNJ

**Objectif :** cacher `NavMeshAgent` derrière une API stable.

**Prompt :**

> Implémente `NpcMotor` comme propriétaire unique du `NavMeshAgent`. Expose des commandes bornées : destination, arrêt, rotation vers cible, état de chemin, arrivée, annulation et tentative de recalcul. Ajoute un statut explicite `Idle/Moving/Waiting/Arrived/Failed` et des raisons d'échec. Aucun Behavior node ne doit manipuler directement `NavMeshAgent`. Ajoute une détection de stagnation à basse fréquence et un recalcul limité sans téléportation active par défaut. Teste destination valide, invalide, annulation, désactivation et reprise.

**Acceptation :** une seule classe écrit la destination NavMesh ; aucune allocation évitable à chaque frame ; un échec est observable par debug.

**Estimation :** M, 1–2 tours.

## P06 — API de porte et réservation de seuil

**Objectif :** empêcher plusieurs agents de se coincer et découpler `SlidingDoor` des PNJ.

**Prompt :**

> Extrais de `SlidingDoor` une API de passage commune sans casser l'interaction joueur : `CanUse`, `RequestOpen`, `IsPassable`, `TryReserve`, `Release` et événements d'état. Implémente une réservation FIFO simple avec propriétaire, expiration et deux points d'attente configurables. Crée `NpcDoorTraversal` dans AI : réserver, attendre, demander ouverture, traverser, libérer. Une mort, un disable, un reset ou un timeout doit toujours libérer la ressource. Teste deux puis quatre capsules venant des deux côtés. Les portes restent sans verrouillage ni droits avancés.

**Attention :** pas de `GetComponent` permanent dans Update, pas de référence du script de porte au type `Guard`.

**Acceptation :** 20 traversées alternées sans blocage permanent ; une réservation abandonnée expire ; le joueur peut encore utiliser la porte.

**Porte humaine H2 :** stress test quatre agents aux deux extrémités.

**Estimation :** L, 2–3 tours.

## P07 — Signature du joueur et perception visuelle

**Objectif :** créer la furtivité visuelle minimale demandée.

**Prompt :**

> Implémente une signature de visibilité sur le joueur à partir de `IsCrouching`, sans coupler AI à `PlayerController`. Implémente `NpcVisionSensor` : distance, angle lié au regard, raycast d'occultation, temps d'exposition et coefficient de posture. La vision produit une observation structurée ; elle ne change pas directement l'état du garde. Un joueur accroupi doit exiger une distance plus courte ou une exposition plus longue. Échantillonne à fréquence configurable, répartie entre agents. Ajoute gizmos pour cône, cible testée, rayon bloqué et dernière observation. Ajoute des tests de fonctions de score et un protocole Play Mode pour face/dos, debout/accroupi et mur.

**Hors périmètre :** luminosité, camouflage, vision périphérique complexe, identification sociale.

**Acceptation :** derrière un mur = aucune observation ; face/debout détecté plus vite que face/accroupi ; derrière le garde non détecté hors cône.

**Porte humaine H3 :** réglage des distances dans un couloir du sandbox.

**Estimation :** L, 2–3 tours.

## P08 — Suspicion, mémoire courte et indicateur « ! »

**Objectif :** rendre la détection lisible et permettre une investigation limitée.

**Prompt :**

> Implémente `NpcAwareness` séparé du capteur. Il accumule/décroît une suspicion normalisée, passe par `Unaware/Suspicious/Alerted`, conserve la dernière position et l'heure de dernière vision. Ajoute une hystérésis pour éviter les oscillations. Implémente `NpcAwarenessIndicator` comme présentation indépendante : caché au calme, signal de suspicion facultatif, puis `!` à l'alerte. Il doit suivre la caméra sans contenir de logique de détection. Toutes les valeurs viennent d'une configuration partagée. Teste seuils, décroissance, perte de vue et reset.

**Acceptation :** pas de clignotement entre états ; `!` apparaît une seule fois par acquisition ; dernière position figée après perte de vue.

**Estimation :** M, 1–2 tours.

## P09 — Alerte globale sans télépathie de position

**Objectif :** respecter D8-A sans détruire la dernière position connue.

**Prompt :**

> Implémente un `AlertService` de scène explicitement référencé. Lorsqu'un garde devient alerté, le service diffuse `Calm/Alerted` et un instantané facultatif de l'observation initiale. Il ne suit jamais le Transform du joueur et ne met jamais à jour sa position automatiquement. Les nouveaux gardes peuvent lire le niveau courant. Le service se réinitialise proprement et ne doit pas être un singleton statique. Ajoute tests d'abonnement/désabonnement, broadcast unique et reset.

**Acceptation :** tous les gardes passent en alerte ; seuls ceux qui voient ensuite le joueur actualisent leur propre dernière position.

**Estimation :** S, 1 tour.

## P10 — Installer Unity Behavior et créer le socle des graphes

**Objectif :** adopter le cerveau choisi sans y enfermer les systèmes.

**Prompt :**

> Installe la version stable de `com.unity.behavior` compatible avec l'éditeur actuel via Package Manager, sans modifier les autres versions de packages. Inspecte les asmdefs exacts du package avant de référencer l'assemblage AI. Crée `NpcContext`, qui référence explicitement identité, santé, moteur, perception, awareness et alert service. Crée seulement les conditions/actions Behavior génériques nécessaires pour lire un état, attendre, choisir une destination, se déplacer et interrompre sur mort/alerte. Les nodes délèguent aux composants et ne font ni raycast ni dommage. Crée un graphe minimal de test dans l'éditeur et documente comment l'ouvrir/déboguer. Ajoute un fallback d'erreur clair si une référence manque.

**Hors périmètre :** graphe final du garde, GOAP, package tiers supplémentaire.

**Acceptation :** graphe minimal exécutable ; désactiver le moteur fait échouer proprement l'action ; aucun node ne possède la logique métier.

**Porte humaine H4 :** ouvrir le graphe, observer son exécution et confirmer la version installée.

**Estimation :** L, 2–3 tours.

## P11 — Garde : repos, patrouille et investigation

**Objectif :** produire une première boucle non combattante complète.

**Prompt :**

> Crée `GuardConfig`, `GuardContext` si une spécialisation est nécessaire, des points de patrouille écrits et des nodes fins pour Idle, Patrol, Suspicious et InvestigateLastKnownPosition. Assemble un graphe Guard réutilisant le socle P10. En cas de suspicion, le garde peut s'orienter ou rejoindre la dernière position ; après un délai court sans contact, il revient à son poste. Une alerte globale interrompt immédiatement la routine. Ajoute un prefab capsule garde avec couleurs d'état temporaires et historique court des transitions.

**Hors périmètre :** découverte de corps, recherche multi-points, flanking, couvert dynamique.

**Acceptation :** dix minutes de boucle sans destination invalide ; suspicion interrompt la patrouille ; perte de cible conduit à une investigation bornée puis retour.

**Estimation :** L, 2–3 tours.

## P12 — Combat à distance réutilisable

**Objectif :** isoler le tir du comportement.

**Prompt :**

> Implémente `NpcRangedCombat` comme façade de combat : validation de cible, rotation/visée, contrôle de ligne de tir avec le résolveur hitscan partagé, anticipation, rafale configurable, intervalle entre tirs et récupération. Le composant publie des événements de présentation et ne choisit ni sa destination ni son état Behavior. Ajoute des positions de tir explicites réservables, sans découverte dynamique de couvert. Crée des nodes Behavior `AcquireFiringPosition`, `Aim`, `FireBurst`, `Recover`. Ajoute un garde et une cible joueur factice dans le sandbox.

**Sécurité :** aucun tir si un mur, une porte fermée, un civil ou un allié bloque la ligne ; la politique exacte face au civil est « retenir le tir ».

**Acceptation :** anticipation visible avant dommage ; nombre exact de tirs ; récupération obligatoire ; aucun tir à travers obstacle ; mort du garde interrompt la séquence.

**Porte humaine H5 :** juger lisibilité et équité d'une rafale à courte distance.

**Estimation :** L, 2–3 tours.

## P13 — Assembler le garde E1+

**Objectif :** livrer l'ennemi complet de la démo dans le sandbox.

**Prompt :**

> Assemble le graphe Guard final avec priorités strictes : Dead > porte/traversée en cours sécurisée > Alert/Combat > Suspicious/Investigate > Patrol/Idle. Intègre santé, hitbox, vision, awareness, alerte globale, navigation, portes, positions de tir et combat. Crée un prefab capsule stable et un `GuardConfig` avec valeurs initiales documentées : 100 santé, perception, vitesse, rafale, anticipation et récupération. Place quatre instances dans un scénario de stress. Ajoute debug par agent activable sans allocations permanentes.

**Hors périmètre :** E2 complet, moral, munitions de garde, communication localisée, escouade tactique.

**Acceptation :** quatre gardes partagent l'alerte, ne se mettent pas à jour télépathiquement, franchissent les portes, attaquent de façon lisible et meurent exactement une fois.

**Estimation :** L, 2–3 tours.

## P14 — Civils C0/C1 capsules

**Objectif :** ajouter de la vie sans construire un second cerveau complexe.

**Prompt :**

> Crée `CivilianConfig`, un prefab capsule différencié et un petit graphe Civilian indépendant. États autorisés : IdleSeated/IdleSmoking, NoticeEvent, ReactFear et Dead. Pour le prototype capsule, simule assise/fumée avec ancrages, orientation, couleur ou petit effet temporaire ; ne crée pas d'animation finale. Le civil reçoit l'alerte globale et les dommages mais ne patrouille, ne fuit et ne cherche pas de cachette. Sa présentation réagit sans commander la logique. Place 2–4 civils dans le sandbox.

**Hors périmètre :** navigation civile, portes, témoins, appels, entraide et dialogue.

**Acceptation :** les civils ne synchronisent pas exactement leur délai de réaction ; ils restent à leur emplacement ; pause, mort et reset sont sûrs.

**Estimation :** M, 1–2 tours.

## P15 — Couture pour conséquences des morts civiles

**Objectif :** préparer D5-D sans implémenter le système long terme.

**Prompt :**

> Ajoute un événement immutable `CivilianIncident` contenant victime, cause, source, position et instant. Crée un `CivilianIncidentRegistry` de scène à durée de vie explicite qui collecte les incidents de la session et se réinitialise. Aucun score, échec, réputation, renfort ou sauvegarde ne doit être ajouté. Teste mort unique, attribution de source, plusieurs victimes et reset. Documente l'interface que le futur système de conséquences consommera.

**Acceptation :** une mort civile produit exactement un incident ; aucun module AI ne dépend d'un futur système de réputation.

**Estimation :** S, 1 tour.

## P16 — Intégrer la rencontre dans `SampleScene`

**Objectif :** passer du laboratoire au train réel sans polluer la scène.

**Prompt :**

> Après sauvegarde et inspection de la scène, intègre dans `SampleScene` une racine clairement nommée `AI_Runtime`, les services de scène, surfaces/links NavMesh nécessaires, points de patrouille/tir/attente, jusqu'à quatre gardes et 2–4 civils. Réutilise les prefabs du sandbox ; ne duplique aucune logique dans la scène. Configure les LayerMasks et collisions explicitement. Ne renomme ni ne déplace les objets utilisateur sans nécessité. Vérifie les transitions de wagon, portes, sièges et lignes de tir. Documente chaque objet d'authoring que le level designer peut déplacer.

**Important :** ne pas intégrer dans `Train.unity`, qui n'est pas la scène jouable actuelle.

**Acceptation :** scène sauvegardée sans références manquantes ; navmesh valide ; rencontre jouable ; aucun changement massif de YAML sans explication.

**Porte humaine H6 :** playthrough complet du wagon avec réglage des positions.

**Estimation :** L, 2–3 tours.

## P17 — Cycle de vie, pause et restart

**Objectif :** garantir qu'une partie recommence proprement.

**Prompt :**

> Branche gardes, civils, alert service, incident registry, santé, armes et réservations sur un contrat de cycle de session explicite. Implémente pause et reset sans ajouter un énorme GameManager : un petit orchestrateur de session peut diffuser les transitions, chaque module réinitialisant son propre état. Vérifie que les Behavior graphs s'arrêtent hors état Playing, que les abonnements sont nettoyés, que les portes et réservations reviennent à leur état initial et que les morts ne se rejouent pas.

**Acceptation :** cinq cycles jouer/mourir/restart sans état fantôme, double abonnement ni erreur console.

**Estimation :** M, 1–2 tours.

## P18 — Outils de debug, stress tests et profilage

**Objectif :** rendre les bugs reproductibles avant le polish.

**Prompt :**

> Finalise un overlay de debug activable affichant par PNJ : état Behavior, suspicion, dernière position, destination, statut de chemin, porte réservée et santé. Ajoute gizmos et historique circulaire borné. Crée des tests Edit Mode pour les contrats purs et des scénarios Play Mode ou un harness Editor pour : 100 cycles de portes, quatre gardes alertés, obstacles de tir, morts multiples et restart. Profile allocations et coût des capteurs ; espace les scans entre agents. Aucun log ne doit spammer en fonctionnement normal.

**Seuils :** zéro blocage définitif sur 100 traversées contrôlées ; zéro exception ; zéro allocation récurrente évidente dans les boucles critiques après échauffement.

**Acceptation :** un échec de navigation ou décision peut être expliqué depuis les outils sans ajouter de log ad hoc.

**Estimation :** L, 2–3 tours.

## P19 — Audit final et documentation de livraison

**Objectif :** fermer la tranche proprement.

**Prompt :**

> Audite toute la tranche contre `NPC_DEVIS_DE_PROMPTS.md`. Recherche dépendances circulaires, recherches d'objets en Update, singletons mutables, logique métier dans les Behavior nodes, abonnements non libérés, scripts trop larges et assets orphelins. Exécute compilation/tests et un protocole complet. Corrige uniquement les défauts du périmètre. Mets à jour le backbone, la roadmap et un README PNJ avec architecture, réglages, authoring de garde/civil/porte, debug, limites E1+/C1 et backlog. Ne lance aucun lot futur.

**Acceptation :** tous les critères de section 7 passent ou sont listés avec preuve et blocage précis ; aucun TODO critique caché.

**Estimation :** S/M, 1–2 tours.

## 6. Portes de validation humaine

Ces validations ne sont pas des prompts supplémentaires. Elles empêchent Codex de deviner des sensations visuelles ou des dimensions réelles.

| Porte | Après | Validation |
|---|---|---|
| H0 | P00 | confirmer que `SampleScene` est bien la scène jouable actuelle |
| H1 | P03 | tir, reload et murs dans le sandbox |
| H2 | P04–P06 | rayons, largeur du couloir et traversée de porte |
| H3 | P07–P08 | distance debout/accroupi et vitesse de suspicion |
| H4 | P10–P11 | graphe Unity Behavior lisible dans l'éditeur |
| H5 | P12–P13 | anticipation, rafales et difficulté contre quatre gardes |
| H6 | P14–P16 | lisibilité des civils et placement dans le train |
| H7 | P17–P19 | cinq playthroughs/restarts et validation finale |

Les huit portes regroupent plusieurs prompts quand ils doivent être jugés ensemble, afin d'éviter une validation manuelle après chaque petite classe.

## 7. Critères de livraison consolidés

### Architecture

- trois assemblages runtime sans cycle ;
- aucun gros cerveau universel ;
- Behavior orchestre des composants testables ;
- perception, awareness, navigation, combat, santé et présentation sont séparés ;
- configuration partagée distincte de l'état runtime.

### Ennemi

- jusqu'à quatre gardes ;
- patrol/idle, suspicion, alerte, dernière position courte, position de tir, anticipation, rafale, récupération, mort ;
- posture accroupie moins visible ;
- indicateur `!` lisible ;
- aucune connaissance continue et globale de la position du joueur.

### Civil

- 2–4 civils C0/C1 ;
- assise/fumée simulée, regard/réaction simple ;
- santé et mort ;
- incident de mort enregistré une fois ;
- aucune promesse de fuite, témoin ou conséquence systémique dans la démo.

### Navigation

- NavMesh stable, liens/seuils documentés ;
- porte utilisable par joueur et PNJ ;
- réservation expirante ;
- 100 traversées de stress sans impasse définitive ;
- aucune téléportation cachant les bugs pendant le développement.

### Combat

- hitscan commun ;
- mur et porte fermée bloquent les tirs ;
- garde retient le tir si allié ou civil bloque ;
- mort unique ;
- pause et restart sûrs.

### Qualité

- aucun recurring console error ;
- tests purs pour dégâts, awareness, alerte, incident et reset ;
- outils de debug désactivables ;
- aucune allocation évitable dans les scans fréquents ;
- procédure d'authoring documentée.

## 8. Lots futurs séparés

Ils ne doivent pas être glissés dans un prompt de la tranche principale.

### F01 — Conséquences systémiques civiles

Consomme `CivilianIncident` pour score moral, échec éventuel, témoins, réputation, renforts et narration. Nécessite une décision de design séparée sur la punition et la persistance.

**Estimation :** 5–8 prompts, L/R&D.

### F02 — Génération LLM hors ligne

Outil Editor séparé : entrée structurée, génération de profils/barks, validation humaine, schéma fermé, cache local et aucun appel dans le build. Nécessite un choix de fournisseur/modèle, une politique de coûts et la gestion des secrets.

**Estimation :** 4–7 prompts, R&D. Ne commence qu'après P19.

### F03 — GOAP/HTN avancé

Prototype dans une scène laboratoire, utilisant les composants existants sans remplacer la démo. Comparaison mesurée avec Unity Behavior avant toute migration.

**Estimation :** 6–10 prompts, R&D.

### F04 — Personnages et animations

Squelette partagé, kit modulaire, locomotion, assise, fumée, visée, impacts, mort, puis Animation Rigging si utile. Les graphes de présentation consomment les événements existants.

**Estimation :** très dépendante des assets ; 6–12 prompts d'intégration, hors production 3D/audio.

## 9. Ordre d'exécution recommandé

```text
P00 -> P01
        ├─> P02 -> P03 ─────────────┐
        ├─> P04 -> P05 -> P06 ─────┤
        └─> P07 -> P08 -> P09 ─────┤
                                    v
                           P10 -> P11 -> P12 -> P13
                                  └──────────> P14 -> P15
                                                   |
                                                   v
                                             P16 -> P17 -> P18 -> P19
```

Les branches P02/P03, P04/P06 et P07/P09 sont conceptuellement indépendantes, mais il est préférable de les exécuter séquentiellement dans ce projet afin de limiter les conflits sur asmdefs, sandbox et prefabs. Le parallélisme ne sera utile qu'aux lectures/audits, pas aux modifications simultanées de scènes.

## 10. Règle de lancement

Un prompt ne commence que si :

1. le précédent dont il dépend est compilé ;
2. ses tests passent ;
3. sa porte humaine éventuelle est validée ;
4. les écarts sont ajoutés au devis ou à la roadmap ;
5. aucune modification utilisateur concurrente ne chevauche ses fichiers.

Si un prompt révèle que son contrat de départ est mauvais, on corrige le contrat dans son propre module avant de poursuivre. On ne contourne pas le problème depuis le Behavior Tree ou depuis la scène.

## 11. Modèle, niveau d'intelligence et consommation estimée

### 11.1 Modèles retenus

Ce devis recommande seulement les deux profils disponibles dans l'environnement actuel :

- **`gpt-5.6-terra`** pour les lots bornés, les données, les contrats simples et les tests ;
- **`gpt-5.6-sol`** pour l'architecture sensible, les scènes Unity, la navigation, la concurrence de portes et les intégrations transversales.

La documentation OpenAI décrit Sol comme le modèle frontière pour le travail professionnel complexe et Terra comme le compromis intelligence/coût. Les deux acceptent les niveaux `none`, `low`, `medium`, `high`, `xhigh` et `max`. [Documentation officielle — choix des modèles GPT-5.6](https://developers.openai.com/api/docs/guides/latest-model)

Le niveau de raisonnement n'est pas une nouvelle IA. Il indique combien de travail de vérification le modèle peut consacrer au même lot :

| Profil | Intelligence pratique visée | Usage conseillé |
|---|---|---|
| Terra medium | équilibrée | tâche courte et bien définie |
| Terra high | élevée | composants modulaires et tests |
| Sol high | très élevée | architecture ou intégration délicate |
| Sol xhigh | très élevée + vérification renforcée | scène, navigation, concurrence, cycle de vie |

`max` et le mode Pro ne sont pas prévus : OpenAI recommande de les réserver aux tâches où le gain est mesuré, car ils augmentent latence et consommation. Le projet devrait d'abord comparer `high` et `xhigh` sur ses vrais bugs. [Documentation officielle — effort et mode Pro](https://developers.openai.com/api/docs/guides/latest-model#pro-mode)

### 11.2 Unité d'usage

OpenAI ne publie pas de conversion universelle du type « ce prompt consomme X % de ton quota Codex ». La consommation dépend notamment du plan, du modèle effectivement disponible, du contexte lu, des sorties, des outils, des compilations et des reprises. Je ne peux pas voir le plan, le solde ou la fenêtre de quota du compte.

Le tableau utilise donc une **unité relative `U`**, propre à ce devis :

> `1 U` ≈ un tour borné avec Terra medium, peu de fichiers et une vérification courte.

Ce n'est ni un crédit OpenAI, ni un token, ni un pourcentage garanti. Cela sert seulement à comparer les lots entre eux.

### 11.3 Affectation par prompt

| ID | Modèle conseillé | Raisonnement | Intelligence pratique | Usage estimé |
|---|---|---|---|---:|
| P00 | Terra | low | normale | 0,5–1 U |
| P01 | Sol | high | très élevée | 2–4 U |
| P02 | Terra | high | élevée | 1,5–3 U |
| P03 | Sol | high | très élevée | 4–7 U |
| P04 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P05 | Terra | high | élevée | 1,5–3 U |
| P06 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P07 | Sol | high | très élevée | 4–7 U |
| P08 | Terra | high | élevée | 1,5–3 U |
| P09 | Terra | medium | équilibrée | 0,75–1,5 U |
| P10 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P11 | Sol | high | très élevée | 4–7 U |
| P12 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P13 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P14 | Terra | high | élevée | 1,5–3 U |
| P15 | Terra | medium | équilibrée | 0,75–1,5 U |
| P16 | Sol | xhigh | très élevée renforcée | 7–12 U |
| P17 | Sol | high | très élevée | 4–7 U |
| P18 | Sol | xhigh | très élevée renforcée | 6–10 U |
| P19 | Sol | high | très élevée | 2–4 U |
| **Total** | **8 lots Terra / 12 lots Sol** | — | — | **environ 71–124 U** |

La fourchette basse correspond à une bonne compilation du premier coup et des validations humaines rapides. La haute suppose plusieurs corrections Unity, surtout sur P04, P06, P10, P13, P16 et P18.

### 11.4 Répartition de l'usage

| Phase | Usage relatif |
|---|---:|
| P00–P03 : baseline et combat partagé | 8–15 U |
| P04–P09 : navigation, portes et perception | 20–35 U |
| P10–P13 : Behavior et garde | 22–37 U |
| P14–P17 : civils et intégration | 13–24 U |
| P18–P19 : durcissement final | 8–14 U |

Les sommes sont arrondies ; le total détaillé reste la référence.

### 11.5 Comment réduire la consommation

- exécuter un seul prompt principal par tour ;
- utiliser le contrat compact créé par P00 au lieu de relire tout l'état de l'art ;
- valider dans Unity avant de demander le prompt suivant ;
- utiliser Terra pour P00, P02, P05, P08, P09, P14 et P15 ;
- réserver Sol xhigh aux six zones à fort risque ;
- ne pas utiliser `max`, Pro ou plusieurs agents par défaut ;
- corriger un contrat dans son module au lieu d'accumuler des contournements ;
- commencer un nouveau fil après une grande phase si l'historique n'est plus utile, en joignant seulement le contrat et l'état d'avancement.

### 11.6 Si l'utilisation passe par l'API

À titre de repère uniquement, l'API facture actuellement Sol à 5 $/million de tokens d'entrée et 30 $/million de sortie, contre 2,50 $ et 15 $ pour Terra. Ces tarifs API ne permettent pas de déduire la consommation d'un abonnement Codex dans l'application. [Sol — tarification API](https://developers.openai.com/api/docs/models/gpt-5.6-sol), [Terra — tarification API](https://developers.openai.com/api/docs/models/gpt-5.6-terra)

La sélection du modèle racine peut dépendre des réglages et droits du produit. Le tableau est donc une recommandation de routage ; il ne garantit pas que l'application changera automatiquement de modèle à chaque prompt.
