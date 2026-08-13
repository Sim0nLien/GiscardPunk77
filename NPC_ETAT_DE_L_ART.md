# GiscardPunk 1977 — État de l'art des PNJ ennemis et civils

> **Statut :** document d'exploration et d'aide à la décision, pas encore une spécification d'implémentation.
>
> **But :** cartographier ce qui peut être créé, du prototype robuste aux systèmes expérimentaux, puis permettre de sélectionner une ambition cohérente avant de produire le devis de prompts et d'implémenter.
>
> **Contexte étudié :** FPS solo sous Unity 6, trois wagons étroits, train fixe avec extérieur mobile, durée cible actuelle de 2–3 minutes, placements d'affrontements écrits à la main et un premier garde armé déjà retenu dans le backbone.

## 1. Résumé exécutif

Un bon PNJ n'est pas celui qui calcule le plus. C'est celui dont le joueur peut comprendre l'intention, anticiper les réactions et provoquer des situations intéressantes.

Pour GiscardPunk, la meilleure fondation n'est donc ni une IA entièrement scriptée scène par scène, ni un grand système autonome. Le meilleur compromis serait un système **hybride** :

- navigation Unity sur NavMesh, renforcée par des points et couloirs de circulation écrits pour le train ;
- perception événementielle commune : vue, bruit, dégâts, découverte d'un corps, alarme et information transmise par un autre PNJ ;
- machine à états hiérarchique pour garantir les grandes phases lisibles ;
- sélection « utility » légère pour choisir une action à l'intérieur d'une phase ;
- objets intelligents réservables pour les portes, sièges, cachettes, couverts, consoles et sorties ;
- mémoire courte et incertaine plutôt qu'une connaissance parfaite ;
- directeur de rencontre très discret pour empêcher les blocages et maintenir le rythme ;
- comportements ennemis et civils construits au-dessus du même socle, mais avec des objectifs différents.

La proposition la plus intéressante pour une première tranche verticale serait : **un garde systémique + trois civils réactifs**. Le garde peut patrouiller, enquêter, combattre, perdre le joueur et éventuellement se rendre. Les civils peuvent poursuivre une micro-routine, remarquer un événement, hésiter, fuir, se cacher, appeler à l'aide ou transmettre une information. Cette tranche donnerait déjà l'impression d'un monde social sans construire une simulation totale.

Les technologies génératives, les modèles de langage et l'apprentissage par renforcement sont possibles, mais ne devraient pas piloter directement les décisions de combat de la première démo. Ils sont plus prometteurs pour générer hors ligne des profils, souvenirs, dialogues courts, variations de routine ou scénarios de test, puis laisser un système déterministe valider et exécuter le résultat.

## 2. Ce que le projet possède réellement aujourd'hui

### 2.1 Base disponible

- Unity 6 et URP.
- Input System.
- `CharacterController` pour le joueur.
- package `com.unity.ai.navigation` 2.0.14 déjà installé.
- scène de train et système d'illusion de déplacement extérieur.
- portes coulissantes commandées par le joueur.
- prototype d'arme à projectiles physiques.

Le package AI Navigation sait construire des NavMesh en édition ou à l'exécution, gérer des obstacles et connecter des surfaces par des liens. C'est une base adaptée à quelques personnages dans un train, à condition de traiter explicitement les portes et passages étroits. [Documentation Unity — AI Navigation](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.navigation.html)

### 2.2 Fondations encore absentes

- pas de contrat partagé de dégâts ;
- pas de santé, mort ou réaction aux impacts ;
- pas de prefab de personnage humanoïde ;
- pas de couche de perception ;
- pas de machine de décision ;
- pas de NavMesh enregistré dans la scène `Train` ;
- pas de protocole permettant aux PNJ de demander l'ouverture d'une porte ;
- pas de gestionnaire de rencontre, factions, alarmes ou relations ;
- pas de jeu d'animations de locomotion, combat et panique identifié.

Le prototype d'arme actuel crée des capsules physiques, tandis que le backbone demande une arme hitscan. Cette divergence doit être résolue avant de juger les réactions ennemies : sinon la logique d'impact, la précision, la ligne de tir et l'équilibrage seraient bâtis sur une base provisoire.

### 2.3 Conséquence de périmètre

Le backbone actuel autorise un seul archétype ennemi et n'inclut pas les civils dans la première démo. Ajouter des civils n'est pas un simple ajout visuel : il faut au minimum définir leur réaction aux menaces, leur collision avec le joueur, leur mortalité éventuelle et la conséquence d'un tir accidentel. Le présent document explore ces possibilités, mais une décision devra modifier explicitement le périmètre avant l'implémentation.

## 3. Anatomie moderne d'un PNJ

Il est utile de séparer huit couches. Elles peuvent ensuite évoluer indépendamment.

| Couche | Question | Exemples dans le train |
|---|---|---|
| Identité | Qui est ce personnage ? | garde, contrôleur, voyageur, technicien, diplomate |
| Besoins et rôle | Que cherche-t-il à préserver ? | survivre, tenir une porte, protéger un VIP, rejoindre une sortie |
| Perception | Que détecte-t-il maintenant ? | joueur visible, coup de feu, corps, vitre brisée, cri |
| Mémoire | Que croit-il savoir ? | dernière position vue, origine probable du bruit, visage suspect |
| Décision | Quelle intention choisit-il ? | enquêter, fuir, avertir, contenir, attaquer |
| Navigation | Comment rejoint-il son but ? | NavMesh, passage réservé, porte, siège contourné |
| Action et animation | Comment rend-il l'intention lisible ? | regarde, pointe, crie, se baisse, court, tire |
| Coordination | Comment affecte-t-il les autres ? | rumeur, alarme, ordre, contagion de panique, priorité de passage |

La perception et la mémoire ne doivent pas se confondre avec la vérité du jeu. Un garde peut connaître la dernière position observée sans connaître la position actuelle. Cette distinction suffit à créer recherche, diversion, surprise et fuite crédibles.

## 4. Grandes familles de systèmes de décision

### 4.1 Script de séquence

Le PNJ exécute une suite préparée : attendre, se tourner, parler, courir vers un point, tirer.

**Forces :** résultat contrôlé, mise en scène précise, débogage simple.

**Faiblesses :** résiste mal aux actions inattendues du joueur, réutilisation faible.

**Bon usage ici :** introduction, première seconde d'une alerte, arrivée en gare, événements uniques. Pas comme cerveau général.

### 4.2 Machine à états finis — FSM/HFSM

Le PNJ passe entre `Calme`, `Suspicion`, `Alerte`, `Combat`, `Fuite`, `Mort`. Une HFSM regroupe des sous-états, par exemple `Combat/SeDéplacer`, `Combat/Viser`, `Combat/Tirer`.

**Forces :** déterministe, lisible, facile à tester et suffisant pour le garde actuel.

**Faiblesses :** les transitions deviennent nombreuses si chaque détail est un état.

**Pertinence :** excellente comme colonne vertébrale commune.

### 4.3 Arbre de comportement

L'agent réévalue une hiérarchie de conditions et d'actions. Unity propose désormais le package Behavior, un outil officiel de behavior trees avec sous-graphes, nœuds C#, événements et débogage en Play mode. Il n'est pas installé dans le projet. [Documentation Unity — Behavior](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.behavior.html)

**Forces :** composition visuelle, réutilisation de branches, bon débogage pour une équipe orientée design.

**Faiblesses :** un arbre trop grand devient difficile à raisonner ; l'ordre des branches peut créer des priorités invisibles.

**Pertinence :** intéressante si l'on prévoit plusieurs archétypes et que l'édition visuelle apporte une vraie valeur. Pour un seul garde, du C# explicite reste plus léger.

### 4.4 Utility AI

Chaque action reçoit un score fondé sur la distance, le danger, les munitions, le courage, la présence de civils ou la disponibilité d'un couvert. L'action au meilleur score gagne, avec hystérésis pour éviter les changements incessants.

**Forces :** variation naturelle, comportements sensibles au contexte, ajout de considérations sans explosion de transitions.

**Faiblesses :** plus difficile à équilibrer ; sans outil de diagnostic, le choix paraît arbitraire.

**Pertinence :** très bonne à petite dose. Exemple : dans l'état `Combat`, choisir entre rester, changer de position, se replier ou se rendre.

### 4.5 GOAP, planification et HTN

Le PNJ compose des actions pour transformer un état du monde : `obtenir une arme → déverrouiller la porte → atteindre la radio → donner l'alerte`.

**Forces :** solutions émergentes, excellent pour infiltration, espionnage et objectifs systémiques.

**Faiblesses :** coût de modélisation élevé, plans absurdes si préconditions et coûts sont incomplets, débogage plus complexe.

**Pertinence :** faible pour la démo linéaire actuelle, forte pour le futur projet de type immersive sim. Une version limitée à quelques buts serait un prototype R&D pertinent.

### 4.6 Directeur narratif ou de rencontre

Un système extérieur aux PNJ suit tension, nombre de menaces actives, progression et blocages. Il peut retarder une alerte, libérer une porte ou interdire à tous les ennemis d'attaquer simultanément.

**Forces :** protège le rythme et la difficulté.

**Faiblesses :** s'il triche visiblement, il détruit la confiance du joueur.

**Pertinence :** utile sous forme minimale : jetons d'attaque, anti-blocage de porte, plafond de civils en panique active.

### 4.7 Apprentissage par renforcement

Unity ML-Agents permet d'entraîner un comportement puis d'exécuter le réseau entraîné dans le jeu. Les agents peuvent partager un même comportement et apprendre dans des scénarios adversariaux ou coopératifs. [Documentation Unity — ML-Agents](https://unity-technologies.github.io/ml-agents/ML-Agents-Overview/)

**Forces :** peut découvrir des tactiques, produire des adversaires d'entraînement et explorer automatiquement des paramètres.

**Faiblesses :** récompenses difficiles à concevoir, comportements difficiles à garantir, longue boucle d'entraînement et moindre explicabilité.

**Pertinence :** meilleure pour tester le niveau ou apprendre une sous-tâche isolée que pour remplacer tout le cerveau. Exemples : agent automatisé cherchant les zones bloquantes ; sélection de déplacements tactiques dans une arène abstraite.

### 4.8 Modèles génératifs et LLM

Les architectures de « generative agents » associent observations, souvenirs, réflexion et planification en langage naturel pour produire des comportements sociaux plausibles. Le papier fondateur montre notamment la propagation d'informations et la coordination spontanée dans une petite ville simulée. [Park et al., *Generative Agents*, UIST 2023](https://arxiv.org/abs/2304.03442)

Unity dispose par ailleurs d'un moteur d'inférence locale, aujourd'hui nommé AI Inference, capable d'exécuter des réseaux dans l'application, avec optimisation, quantification et découpage de l'inférence entre plusieurs images. [Documentation Unity — AI Inference](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.inference.html)

**Forces :** profils et souvenirs riches, conversations variées, propagation sociale émergente.

**Faiblesses :** latence, coût éventuel d'un service, non-déterminisme, contrôle éditorial, localisation, sécurité des contenus, sauvegarde et reproductibilité des bugs.

**Pertinence :** prometteuse en périphérie, risquée au cœur du gameplay temps réel.

### 4.9 Comparaison synthétique

| Famille | Contrôle | Variété | Débogage | Coût de production | Place conseillée |
|---|---:|---:|---:|---:|---|
| Séquence écrite | très fort | faible | facile | faible à moyen | moments uniques |
| HFSM | fort | moyen | facile | faible | socle recommandé |
|(choix début) Behavior tree | fort | moyen/fort | moyen | moyen | plusieurs archétypes |
| Utility AI | moyen/fort | fort | moyen | moyen | choix locaux recommandés |
|(choix version avancé mais pas maintenant) GOAP/HTN | moyen | très fort | difficile | fort | future immersive sim |
| Directeur | fort | moyen | moyen | moyen | garde-fous globaux |
| ML-Agents | faible/moyen | forte | difficile | très fort | R&D et tests |
| LLM génératif | faible | très forte | très difficile | très fort | social hors combat/offline |

## 5. Navigation dans un train : le vrai problème difficile

(je ne connais pas asse, je veux un truc qui tienne la route pour la démo sans trop de bug et sans trop poussé le danger, tu expliquera ton choix)

Dans un espace ouvert, atteindre une destination suffit souvent. Dans un wagon, deux agents qui se croisent, une porte fermée ou un corps dans le couloir peuvent bloquer toute la simulation.

### 5.1 NavMesh global

Le NavMesh calcule le chemin général. Il convient au projet parce que le train reste fixe. Une surface par wagon ou une surface globale sont possibles ; des `NavMeshLink` peuvent relier proprement les soufflets et seuils difficiles.

Limite : un chemin valide ne garantit pas que plusieurs personnages se croiseront élégamment dans un passage étroit.

### 5.2 Graphe de circulation écrit

On ajoute au NavMesh des zones sémantiques :

- couloir à sens privilégié ;
- seuil de porte ;
- baie de croisement ;
- emplacement debout ;
- siège ;
- couvert ;
- cachette ;
- sortie de secours ;
- console d'alarme.

Le chemin reste calculé, mais les interactions délicates utilisent des points prévus. C'est moins « magique », beaucoup plus robuste et très adapté aux trois wagons écrits à la main.

### 5.3 Réservation et priorité

Chaque passage étroit devient une ressource réservable. Un agent obtient un bail court sur la porte ; les autres attendent sur des points latéraux. Les priorités peuvent être :

1. agent déjà engagé dans le passage ;
2. civil en fuite immédiate ;
3. garde poursuivant le joueur ;
4. routine calme.

Un délai maximal et une téléportation de sécurité hors caméra peuvent servir d'ultime anti-blocage, mais seulement après avoir essayé une vraie sortie.

### 5.4 Évitement local et foule

L'évitement réciproque ajuste les vitesses de plusieurs agents pour éviter les collisions. Les méthodes de type ORCA formalisent ce partage de responsabilité entre agents. [van den Berg et al., *Optimal Reciprocal Collision Avoidance*](https://gamma.cs.unc.edu/ORCA/publications/ORCA.pdf)

Dans le train, augmenter simplement la qualité d'évitement ne résout pas tout : des agents symétriques peuvent hésiter dans un couloir trop étroit. Il faut garder les réservations et les baies de croisement comme règle de niveau supérieure.

### 5.5 Portes intelligentes

Une porte ne devrait pas être une animation connue individuellement par chaque cerveau. Elle devrait exposer un contrat :

- `CanUse(agent)` ;
- `RequestOpen(agent)` ;
- `IsPassable` ;
- `ReservePassage(agent)` ;
- événements `Opened`, `Closed`, `Jammed`, `Locked`.

Ainsi, un garde, un civil ou le joueur emploient le même objet selon leurs droits. Une porte verrouillée peut devenir une information de planification au lieu d'un mur incompréhensible.

## 6. Perception, connaissance et communication

### 6.1 Vision

(En fonction regard garde et un certain périmétre si personnage debout et plus proche accroupi + réaction comme dans skyrim avec pint exclamation, on poussera le reste des idées pour la suite)

Une perception visuelle crédible combine :

- distance maximale ;
- angle horizontal et éventuellement vertical ;
- raycast d'occultation vers plusieurs points du corps ;
- temps minimal d'exposition ;
- modulation par posture, vitesse et éclairage ;
- identification séparée de la simple détection d'un mouvement.

Pour la première démo, l'éclairage peut rester purement visuel et la vision utiliser des valeurs stables. Pour une future infiltration, une mesure de visibilité doit être partagée entre lumière, posture et camouflage.

### 6.2 Audition événementielle

Les sons de gameplay publient un stimulus avec position, intensité, catégorie et émetteur : pas, tir, impact, porte forcée, cri, objet lancé. Le PNJ ne « raycaste » pas continuellement le monde sonore ; il reçoit l'événement, applique distance, occlusion simplifiée et sensibilité.

Cela permet les diversions et évite de lier l'IA au volume réel du mixage audio.

### 6.3 Mémoire et incertitude

Une mémoire utile contient :

- fait ou hypothèse ;
- position associée ;
- instant d'acquisition ;
- confiance ;
- source : vu, entendu, transmis, ordre reçu ;
- durée ou règle d'oubli.

Exemple : « coup de feu entendu dans le wagon 2, confiance 0,7 » n'équivaut pas à « joueur exactement en X/Y/Z ». Cette incertitude produit naturellement enquête et recherches imparfaites.

### 6.4 Communication sociale

Les agents peuvent transmettre des faits à proximité, par cri, radio, interphone ou alarme filaire. La transmission doit avoir une portée, une latence et un contenu précis.

Un garde ne devrait pas télépathiquement synchroniser tous les autres. Il peut dire : « intrus vu près du bar il y a cinq secondes ». Le destinataire reçoit ce souvenir avec une confiance réduite.

## 7. État de l'art des ennemis possibles

### Niveau E0 — Cible ou tourelle

- détecte ;
- prévient visuellement ;
- tire ;
- prend des dégâts ;
- meurt ou se désactive.

Très bon banc de test, identité rétro-tech forte, mais faible impression humaine.

### Niveau E1 — Garde de combat lisible

- repos ou patrouille ;
- détection directe ;
- alerte ;
- déplacement vers une position de tir écrite ;
- rafale anticipée ;
- récupération ;
- réaction aux impacts ;
- mort.

C'est le niveau déjà prévu par le backbone. Il constitue le minimum publiable.

(niveau attendu première démo E1)

### Niveau E2 — Garde conscient de l'incertitude

Ajouts :

- suspicion graduelle ;
- réaction aux bruits ;
- dernière position connue ;
- recherche locale ;
- découverte d'un collègue neutralisé ;
- perte du joueur et retour contrôlé ;
- appel d'alarme non instantané.

Ce niveau donne déjà une vraie saveur d'infiltration, même sans furtivité complète.

### Niveau E3 — Garde tactique individuel

Ajouts :

- choix utility entre avancer, tenir, contourner localement, changer de couvert et se replier ;
- gestion de la distance préférée ;
- tir de suppression simplifié ;
- utilisation de portes et consoles ;
- moral, douleur et munitions ;
- abandon, reddition ou fuite.

Attention : dans un train linéaire, « flanquer » se réduit souvent à changer de côté d'un meuble. Il vaut mieux produire des changements de pression lisibles qu'une prétendue grande tactique.

### Niveau E4 — Escouade coordonnée

Ajouts :

- rôles temporaires : fixer, avancer, protéger l'alarme ;
- jetons d'attaque pour éviter les rafales simultanées injustes ;
- partage limité d'informations ;
- ordre de repli ;
- priorité aux portes et couverts.

Impression forte avec trois ou quatre gardes, mais elle exige une arène principale suffisamment large et beaucoup de tests.

### Niveau E5 — Adversaire systémique d'immersive sim

Ajouts :

- buts concurrents ;
- routines hors combat ;
- accès et identité sociale ;
- planification de plusieurs actions ;
- preuves, témoignages et soupçons ;
- réponse aux systèmes du monde : électricité, radio, verrouillage, incendie, sabotage.

C'est une direction de long terme, pas un prérequis pour cette démo.

### Comportements ennemis à forte valeur de jeu

| Comportement | Valeur pour le joueur | Coût relatif |
|---|---|---:|
| anticipation visible avant le tir | équité et lisibilité | faible |
| dernière position connue | permet diversion et rupture de contact | moyen |
| appel d'alarme interrompable | crée un micro-objectif | moyen |
| changement de poste écrit | mouvement sans navigation tactique complexe | faible/moyen |
| rechargement audible | fenêtre d'action lisible | faible |
| moral et reddition | variété et ton politique | moyen |
| investigation d'un corps | cohérence furtive | moyen |
| tir ami évité | crédibilité et équité | moyen |
| communication non télépathique | monde systémique | moyen |
| couverture dynamique découverte | gain limité dans le train | fort |

### Archétypes ennemis envisageables

La profondeur du cerveau et le rôle de combat sont deux choix différents. Plusieurs silhouettes peuvent partager presque tout leur code si seules l'arme, la distance préférée et une capacité changent.

| Archétype | Promesse de jeu | Adaptation au train | Remarque de périmètre |
|---|---|---:|---|
| garde armé standard | pression à distance, lecture immédiate | excellente | premier choix déjà retenu |
| agent au pistolet-mitrailleur | rafales courtes, mobilité | excellente | variante de données possible |
| contrôleur au bâton électrique | pousse le joueur à reculer | moyenne | risque de collision dans les couloirs |
| garde anti-émeute au bouclier | change les zones à viser | bonne | exige dégâts localisés et animation dédiée |
| officier/radiotéléphoniste | coordonne et appelle l'alarme | excellente | micro-objectif très lisible |
| tireur derrière une meurtrière | verrouille une ligne | bonne | proche d'une tourelle, peu coûteux |
| automate de sécurité | identité rétro-futuriste forte | excellente | animations simplifiables, perception différente |
| drone plafonnier sur rail | menace mobile sans encombrer le sol | excellente | navigation spécialisée mais bornée |
| chien ou robot pisteur | suit odeur/trace et débusque | moyenne | animation et pathfinding coûteux |
| lourd en exosquelette administratif | mini-boss, avance implacable | moyenne | peut devenir injuste sans espace de contournement |
| infiltré habillé en civil | doute et identification sociale | excellente pour le futur | demande factions et signaux visuels équitables |

Pour la démo, le radiotéléphoniste est probablement la meilleure « nouveauté » après le garde standard : il peut utiliser le même corps et le même cerveau, mais transforme l'alarme en action interrompable. L'automate ou le drone sur rail serait la meilleure seconde famille véritablement différente.

## 8. État de l'art des civils possibles

### Niveau C0 — Décor animé

Le civil est assis, lit, fume ou regarde par la fenêtre. Il joue une animation en boucle et n'entre pas dans la simulation.

**Usage :** ambiance uniquement. Toute attaque révèle immédiatement l'illusion.

### Niveau C1 — Figurant réactif

- tourne la tête vers un événement ;
- joue peur ou surprise ;
- se baisse ou reste figé ;
- désactive ensuite sa simulation complexe.

**Usage :** faible coût, bon pour un plan bref ou un wagon non traversé.

(niveau attendu pour la démo)

### Niveau C2 — Civil de crise

- micro-routine calme ;
- perception des tirs, cris et gardes ;
- choix parmi sidération, fuite, cachette et appel ;
- réservation des passages ;
- sortie ou cachette persistante ;
- évitement du joueur et des lignes de feu.

**Usage :** niveau recommandé pour une petite tranche verticale.

### Niveau C3 — Témoin social

Ajouts :

- distingue menace, protecteur et inconnu ;
- mémorise une action grave ;
- transmet une description ou déclenche une alarme ;
- peut obéir à un ordre simple ;
- réactions différentes selon tempérament et faction.

**Usage :** très intéressant pour espionnage, identité et conséquences non létales.

### Niveau C4 — Civil à routine systémique

Ajouts :

- besoins et emploi du temps ;
- sièges, bar, toilettes, compartiment, travail ;
- relations entre PNJ ;
- adaptation du programme aux retards et incidents ;
- mémoire d'événements persistants.

**Usage :** immersive sim longue, trop coûteuse pour un trajet de trois minutes sauf prototype très ciblé.

### Niveau C5 — Persona générative

Ajouts possibles :

- biographie et opinions générées ;
- résumé de souvenirs ;
- conversation libre ;
- rumeurs reformulées ;
- planification sociale.

**Usage :** laboratoire narratif. La sortie du modèle doit être contrainte par des actions autorisées et ne jamais commander directement locomotion, dégâts ou progression.

### Répertoire de réactions civiles

Un civil ne devrait pas toujours fuir. Une distribution de réponses courtes est plus crédible :

- **sidération :** reste immobile quelques secondes, puis réévalue ;
- **accroupissement :** se protège derrière le meuble le plus proche ;
- **fuite :** rejoint une sortie sûre sans traverser la menace connue ;
- **cachette :** réserve un compartiment, sous-table ou renfoncement ;
- **appel :** utilise interphone ou bouton d'alarme ;
- **conformité :** lève les mains et suit un ordre ;
- **entraide :** aide un proche blessé si le danger baisse ;
- **opportunisme :** vole un objet, change de camp ou profite d'une porte ouverte ;
- **témoignage :** retient l'apparence et les actes observés ;
- **résistance :** très rare, dépend du rôle, jamais tirée au hasard sans signal visuel.

### Distribution civile possible dans GiscardPunk

Une foule crédible vient davantage de rôles bien choisis que de visages tous uniques.

| Rôle | Routine visible | Réaction distinctive possible |
|---|---|---|
| voyageur ordinaire | lit, fume, regarde le paysage | fuit ou se fige |
| employé du rail | inspecte une porte ou un panneau | connaît une sortie technique |
| serveur du wagon-bar | sert, range, essuie | se cache derrière le comptoir |
| cadre ministériel | téléphone, consulte un dossier | exige, négocie ou appelle la sécurité |
| journaliste | prend des notes ou photographie | devient témoin particulièrement dangereux |
| technicien | répare une armoire électrique | peut couper ou rétablir un système |
| militant clandestin | observe, échange un paquet | aide ou trompe selon les actes du joueur |
| diplomate ou VIP | accompagné, peu mobile | objectif secondaire de protection/extraction |
| médecin/infirmier | voyage ou travaille | secourt un blessé lorsque la zone est sûre |
| enfant/famille | interaction sociale forte | déconseillé au premier prototype pour le ton et les implications |

Pour deux à quatre civils, le trio `voyageur + employé du rail + cadre/journaliste` donne déjà des silhouettes, routines et réactions différentes sans simuler une foule complète.

### Question indispensable : les civils sont-ils vulnérables ?

Quatre modèles existent :

1. **Intangibles/invulnérables :** simple mais artificiel.
2. **Blessables sans mort :** ils tombent et sortent du combat ; bon compromis de ton.
3. **Mortels avec conséquence immédiate :** échec, score ou hostilité ; très lisible mais punitif.
4. **Mortels avec conséquence systémique :** témoins, réputation, renforts, narration ; riche mais coûteux.(objectif long long terme)

Ce choix affecte dégâts, sauvegarde, animation, audio, classement du jeu et perception morale. Il doit être décidé avant le prefab civil.

### Dialogue et voix : cinq niveaux

1. **Barks d'état écrits :** alertes, peur, reddition, recherche. Solution la plus fiable et nécessaire même sans conversation.
2. **Barks combinatoires :** fragments contrôlés associant sujet, lieu et urgence, par exemple « Intrus — wagon-bar — armé ». Bonne variété avec un vocabulaire fermé.
3. **Conversation à choix courts :** ordres ou réponses contextuelles, sans arbre narratif étendu.
4. **Texte généré hors ligne puis validé :** production accélérée de variantes, toujours relues et enregistrées dans le build.
5. **Conversation générative en direct :** grande liberté, mais problèmes de latence, cohérence, voix, contenu et progression.

Le niveau 2 est particulièrement adapté au système de mémoire proposé : le PNJ verbalise exactement les faits qu'il transmet. Des sous-titres restent nécessaires, y compris pour les cris en combat.

### Production visuelle des personnages

| Approche | Avantages | Limites | Usage conseillé |
|---|---|---|---|
| capsules/couleurs | instantané, parfait pour l'IA et les tests | aucune identité finale | première étape obligatoire | (on va aller la pour l'insatant)
| kit humanoïde modulaire sur un squelette partagé | variantes de têtes, tenues et accessoires rentables | préparation des matériaux et compatibilité des meshes | civils et gardes standards |
| personnages achetés puis adaptés | accélère le prototype visuel | cohérence artistique et licences à vérifier | placeholders de qualité |
| personnages sur mesure | direction GiscardPunk précise | coût de modélisation, rig, skin et textures | héros, officier, automate signature |
| génération procédurale de foule | beaucoup de combinaisons | contrôle qualité et répétitions visibles | projet plus vaste, pas la démo |

La stratégie la plus rationnelle est un **squelette humanoïde partagé**, une base de corps, quelques têtes et des vêtements/accessoires modulaires. Les gardes obtiennent silhouette et palette cohérentes ; les civils réutilisent locomotion et réactions tout en variant par manteau, coiffure, lunettes, sac et couleur.

Les éléments réellement coûteux ne sont pas seulement les meshes : rigging, retargeting, transitions, poses assises, prise d'arme, mains sur les accessoires, réactions aux impacts, voix et validation de toutes les combinaisons peuvent dépasser le coût du cerveau C#.

### Niveau de détail et foule

- LOD de mesh et matériaux pour les personnages éloignés ;
- fréquence de perception et décision réduite hors wagon actif ;
- animation simplifiée ou mise en pause hors champ ;
- pooling seulement si les personnages entrent et sortent souvent ;
- ragdoll limité aux personnages proches, puis pose figée ou disparition contrôlée ;
- jamais de réapparition visible d'un civil qui représenterait une personne persistante.

Dans trois wagons, l'optimisation la plus importante restera la maîtrise du nombre d'agents actifs et des capteurs, pas une architecture de foule massive.

## 9. Systèmes partagés qui créent un monde crédible

(Ici pas nécéssaire pour la démo, puet être juste s'assoir et le fait de fumée et c'est tout)

### 9.1 Factions et attitude

Un agent appartient à une faction et calcule une attitude envers une entité : alliée, autorisée, suspecte, hostile, protégée. Cela évite de coder « le joueur » comme ennemi universel et prépare déguisements, papiers et retournements.

### 9.2 Smart Objects

Un objet intelligent décrit les actions possibles et leurs conditions :

- siège : s'asseoir, se relever, se cacher derrière ;
- téléphone : appeler, écouter, couper la ligne ;
- radio : transmettre ou brouiller ;
- porte : ouvrir, verrouiller, forcer, tenir ;
- bar : servir, se couvrir, récupérer un objet ;
- fenêtre : regarder, casser, signaler ;
- armoire : fouiller ou se cacher.

Ce système concentre l'intelligence dans le monde. Ajouter une nouvelle console devient plus rentable que modifier tous les cerveaux.

### 9.3 Zones sémantiques

Chaque volume peut déclarer : wagon, pièce, public/interdit, danger, sortie, silence, ligne de tir, abri. Les agents raisonnent alors sur des lieux compréhensibles plutôt que sur des coordonnées brutes.

### 9.4 Blackboard local et état mondial

- **local :** perception, mémoire, cible, intention et réservation d'un agent ;
- **escouade/faction :** niveau d'alerte et rapports connus ;
- **monde :** courant, portes, progression, statut de l'alarme.

Le partage doit être explicite. Écrire directement la position exacte du joueur dans un blackboard mondial détruirait tout gameplay de recherche.

### 9.5 Niveaux de simulation

Les PNJ visibles utilisent navigation et animation complètes. Dans les wagons éloignés, ils peuvent être réduits à un état logique : lieu, activité, destination et heure d'arrivée estimée. À l'approche du joueur, le système les matérialise sur un point valide.

Cette « simulation fantôme » est plus utile qu'une foule constamment active et ouvre la voie à un train plus long dans le futur.

## 10. Animation, regard et expressivité

L'intelligence perçue vient souvent davantage de la présentation que de la décision.

### Minimum ennemi

- idle ;
- marche/course ;
- visée ;
- tir et récupération ;
- impact léger/fort ;
- mort claire ;
- regard ou rotation de tête ;
- animation d'alerte avant attaque.

### Minimum civil

- deux ou trois idles assis/debout ;
- marche/course ;
- surprise ;
- peur accroupie ;
- fuite ;
- mains levées ;
- entrée et maintien en cachette ;
- blessure ou mort selon la décision de design.

### Approches techniques

- locomotion pilotée par le NavMesh avec blend tree de vitesse ;
- rotation du corps limitée et regard indépendant ;
- IK de pieds pour le sol et IK de mains/visée pour les armes ;
- couches additives pour peur, blessure et regard ;
- root motion réservé aux actions courtes bien contrôlées, comme s'asseoir ou franchir un obstacle.

L'Animation Rigging ajoute des contraintes procédurales, par exemple `Two Bone IK` et `Multi-Aim`, au-dessus de l'Animator. Ce package n'est pas installé actuellement et devrait être ajouté uniquement lorsqu'un personnage riggé est disponible. [Documentation Unity — Animation Rigging](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.2/manual/RiggingWorkflow.html)

## 11. Idées expérimentales propres à GiscardPunk

### X1 — Obéissance administrative

Les gardes ne détectent pas seulement une silhouette hostile. Ils vérifient un niveau d'habilitation : uniforme, badge, document, réponse verbale et zone autorisée. Le joueur peut être techniquement visible sans être encore identifié comme intrus.

**Intérêt :** transforme la perception en tension politique et sociale.

### X2 — Réseau d'alarme analogique

L'information circule physiquement par téléphone, voyant, câble et interphone. Couper le courant, arracher un câble ou occuper une console modifie réellement la propagation de l'alerte.

**Intérêt :** l'IA devient compréhensible et manipulable dans le décor.

### X3 — Rumeur imparfaite

Chaque transmission dégrade ou déforme un attribut : lieu, tenue, arme, culpabilité. Deux civils peuvent rapporter des versions différentes ; les gardes fouillent alors des zones différentes.

**Intérêt :** émergence sociale sans dialogue génératif obligatoire.

### X4 — Courage contextuel

La peur dépend du nombre d'alliés visibles, du statut social, du bruit, d'un proche blessé et de l'état du train. Un garde isolé peut se rendre ; le même garde entouré reste agressif. Un civil normalement passif peut secourir quelqu'un lorsque les tirs cessent.

**Intérêt :** personnalité exprimée par le gameplay.

### X5 — Mémoire visuelle limitée

Un témoin retient quelques traits saillants : manteau orange, arme longue, badge ministériel. Changer un élément réduit la confiance sans effacer magiquement l'incident.

**Intérêt :** déguisement et enquête de long terme.

### X6 — Chorégraphie de panique

Le comportement reste systémique, mais un « chef de foule » invisible attribue des sorties et cachettes pour produire une image lisible : une personne fuit, une autre se fige, une troisième tire quelqu'un derrière une table.

**Intérêt :** évite la masse uniforme de civils courant tous vers la même porte.

### X7 — Minitel prédictif diégétique

Un terminal affiche une estimation statistique de la situation : niveau d'alerte, témoignages possibles, probabilité qu'une porte soit gardée. Il ne révèle pas la vérité exacte, mais synthétise ce que le réseau administratif « croit ».

**Intérêt :** rend les blackboards et la propagation d'information visibles dans l'univers.

### X8 — Mise en scène adaptative sans triche

Le directeur ne téléporte pas les ennemis devant le joueur. Il choisit parmi des intentions plausibles déjà disponibles : retarder un départ de patrouille, faire hésiter un civil, autoriser un garde à appeler plutôt qu'attaquer.

**Intérêt :** maintient une tension cinématographique tout en conservant la causalité.

### X9 — LLM hors ligne, simulation déterministe en jeu

Un outil d'éditeur génère des fiches de personnages, relations, répliques et réactions probables. Un validateur les convertit en données fermées : traits numériques, lignes de dialogue approuvées, routine et seuils. Le build n'exécute aucun LLM.

**Intérêt :** grande variété narrative, coût d'exécution nul et bugs reproductibles.

### X10 — IA comme outil de test

Un agent ML ou une suite de bots tente des milliers de traversées : portes fermées, civils paniqués, cadavres dans le couloir, différents rayons de capsule. Il cherche blocages, positions impossibles et combats statistiquement injustes.

**Intérêt :** l'apprentissage améliore la qualité du jeu sans rendre les PNJ de production opaques.

## 12. Quatre ambitions cohérentes

### Option A — Démo de combat stricte

**Contenu :** un garde E1, aucun civil simulé ou seulement des silhouettes hors zone jouable.

**Technique :** HFSM, NavMesh, positions de tir écrites, vue directe, aucune recherche complexe.

**Avantages :** protège entièrement la roadmap actuelle ; voie la plus rapide vers une boucle jouable.

**Limites :** train peu vivant et faible préparation à l'infiltration.

### Option B — Tranche verticale vivante — recommandée

**Contenu :** un garde E2/E3 limité et deux à quatre civils C2.

**Technique :** socle partagé, stimuli vue/bruit, mémoire courte, HFSM + utility locale, smart objects essentiels, réservation des portes et cachettes.

**Avantages :** prouve combat, ambiance sociale et potentiel d'infiltration avec un périmètre encore maîtrisable.

**Limites :** nécessite animations civiles, politique de dégâts et tests de circulation supplémentaires.

### Option C — Prototype d'immersive sim

**Contenu :** ennemis E3/E4, civils C3, factions, témoins, alarme physique, quelques habilitations ou déguisements.

**Technique :** systèmes partagés plus riches, mémoire propagée, smart objects, directeur et peut-être GOAP limité.

**Avantages :** identité très forte, base réutilisable à long terme.

**Limites :** dépasse clairement la démo 2–3 minutes et doit remplacer plusieurs objectifs actuels plutôt que s'y ajouter.

### Option D — Laboratoire génératif

**Contenu :** un petit wagon expérimental séparé, trois à cinq agents sociaux, souvenirs et dialogues générés.

**Technique :** LLM local ou distant, actions fermées, cache, journal de décisions, simulation à basse fréquence.

**Avantages :** recherche originale et démonstrateur distinctif.

**Limites :** ne constitue pas une fondation fiable pour le combat principal ; projet R&D séparé.

## 13. Architecture recommandée si l'option B est choisie

```text
StimulusBus ───────────────┐
                          v
World/Faction State -> Perception -> Memory -> HFSM intention
       ^                                      |
       |                                      v
Smart Objects <- Reservation/Navigation <- Utility choice
       |                                      |
       └──────── events <- Action/Animation <-┘

Encounter Director observe seulement les métriques globales
et applique des garde-fous explicites.
```

### Composants probables

- `ActorIdentity` : faction, rôle, tags sociaux.
- `ActorVitals` : santé, blessure, mort, événements idempotents.
- `DamageReceiver` et contrat `DamageInfo`.
- `NpcPerception` : capteurs et réception des stimuli.
- `NpcMemory` : faits datés avec confiance.
- `NpcBrain` : intention HFSM.
- `NpcUtilitySelector` : scores locaux et hystérésis.
- `NpcMotor` : interface vers `NavMeshAgent`, rotation et blocages.
- `NpcAnimationDriver` : paramètres Animator et IK.
- `SmartObject` / `SmartObjectSlot` : interactions et réservations.
- `DoorAccess` : usage commun des portes.
- `AlertNetwork` : transmission explicite par faction et médium.
- `EncounterDirector` : jetons d'attaque et anti-impasse.
- assets de configuration séparés pour garde, civil et tempérament uniquement si plusieurs variantes les réutilisent.

### Principes de sécurité architecturale

- aucune décision importante ne dépend d'un `Update()` dans dix composants différents ;
- la mort est déclenchée une seule fois ;
- les sens publient des données, ils ne commandent pas directement l'animation ;
- l'Animator présente l'état, il ne devient pas le cerveau ;
- les agents ne connaissent jamais implicitement la position exacte du joueur ;
- toute réservation expire ;
- chaque choix utility peut expliquer son score dans un panneau de debug ;
- chaque transition importante est journalisable sans spammer la console ;
- une version capsule sans art doit permettre de tester tout le comportement.

## 14. Ordre de prototypage rationnel

Cet ordre ne constitue pas encore le devis de prompts ; il montre seulement les dépendances.

1. **Fondation de dégâts :** contrat, santé, mort idempotente et cible de test.
2. **Navigation capsule :** NavMesh du train, portes, liens, blocages et points d'attente.
3. **Perception testable :** vue, stimulus sonore, gizmos et dernière position connue.
4. **Garde gris :** HFSM complète sans modèle final.
5. **Lisibilité du combat :** anticipation, rafale, récupération, impact, mort.
6. **Smart objects minimaux :** poste de tir, porte, alarme, cachette.
7. **Civil gris :** routine, sidération, fuite, cachette et sortie.
8. **Réservation et crise collective :** plusieurs agents dans les portes.
9. **Mémoire et transmission :** rapports locaux, pas de télépathie.
10. **Animation et audio :** seulement après validation des transitions sur capsules.
11. **Directeur et variation :** jetons d'attaque, tempéraments et anti-impasses.
12. **Expériences isolées :** moral, témoin, rumeur, GOAP limité ou génération hors ligne.

## 15. Outils de débogage indispensables

Sans visualisation, l'IA paraît aléatoire même quand elle ne l'est pas.

- état et intention au-dessus de la tête ;
- cône de vue et rayons d'occultation ;
- stimuli sonores sous forme de sphères temporaires ;
- dernière position connue et niveau de confiance ;
- destination et chemin NavMesh ;
- smart object réservé, propriétaire et expiration ;
- scores utility classés ;
- niveau d'alerte par faction ;
- historique court des cinq dernières transitions ;
- compteur de blocage et motif de recalcul ;
- mode simulation accélérée sans rendu final.

## 16. Critères d'évaluation

### Navigation

- aucun PNJ bloqué définitivement dans une porte sur 100 simulations ;
- reprise automatique après fermeture d'une porte ;
- pas de poussée permanente entre deux agents ;
- destination invalide détectée et solution de repli visible dans les logs de debug.

### Combat

- le joueur peut identifier l'attaque avant de recevoir la rafale ;
- aucune balle ne traverse une paroi ou une porte fermée ;
- aucun tir allié intentionnel sans règle explicite ;
- garde capable de perdre le joueur ;
- mort déclenchée exactement une fois ;
- décision compréhensible depuis la vue debug.

### Civils

- pas de course collective vers une unique cachette ;
- un civil ne choisit pas une sortie qui traverse la menace connue sauf absence d'alternative ;
- réaction initiale en moins d'une fenêtre définie, sans synchronisation parfaite de tous les civils ;
- conséquence d'un dommage accidentel cohérente avec la règle choisie ;
- cachettes et sorties libérées lors d'un restart.

### Performance et reproductibilité

- capteurs répartis dans le temps, sans allocation évitable à chaque image ;
- fréquence de décision inférieure à la fréquence d'affichage quand possible ;
- seed optionnelle pour reproduire une variation ;
- test avec et sans rendu ;
- profilage dans le wagon principal avec le nombre maximal de PNJ prévu.

## 17. Décisions à prendre avant le devis de prompts

Le fichier peut être renvoyé avec les cases choisies ou avec des commentaires libres.

### D1 — Ambition immédiate

- [X] **D1-A :** option A, combat strict.
- [ ] **D1-B :** option B, tranche verticale vivante — recommandée.
- [ ] **D1-C :** option C, immersive sim.
- [ ] **D1-D :** option D, laboratoire séparé.

### D2 — Quantité de PNJ dans la tranche

- [ ] **D2-A :** 1 garde, aucun civil actif.
- [X] **D2-B :** jusqu'à 4 gardes, 2–4 civils actifs — recommandée pour B.
- [ ] **D2-C :** foule de plus de 8 personnages actifs.

### D3 — Profondeur ennemie

- [ ] **D3-A :** E1, combat lisible.
- [X] **D3-B :** E2, suspicion et recherche — recommandée.
- [ ] **D3-C :** E3, tactique individuelle et moral.
- [ ] **D3-D :** E4+, escouade ou planification systémique.

### D4 — Profondeur civile

- [X] **D4-A :** C0/C1, ambiance.
- [ ] **D4-B :** C2, crise et cachettes — recommandée.
- [ ] **D4-C :** C3, témoins et ordres.
- [ ] **D4-D :** C4/C5, routine persistante ou persona générative.

### D5 — Vulnérabilité civile

- [ ] **D5-A :** invulnérables.
- [ ] **D5-B :** blessables mais non mortels — compromis recommandé pour prototype.
- [ ] **D5-C :** mortels avec conséquence immédiate.
- [X] **D5-D :** mortels avec conséquences systémiques.

### D6 — Cerveau principal

- [] **D6-A :** HFSM C# — recommandée pour la première démo.
- [X] **D6-B :** Unity Behavior et arbres visuels.
- [ ] **D6-C :** HFSM + utility locale — recommandée pour l'option B.
- [ ] **D6-D :** GOAP/HTN limité.

### D7 — Furtivité

- [ ] **D7-A :** aucune, acquisition directe du joueur.
- [X] **D7-B :** vue, bruit, suspicion, dernière position — recommandée.
- [ ] **D7-C :** lumière, posture, déguisement et habilitation.

### D8 — Information entre PNJ

- [X] **D8-A :** état d'alerte global instantané.
- [ ] **D8-B :** cris/radio avec faits localisés — recommandée.
- [ ] **D8-C :** rumeurs dégradées et témoignages persistants.

### D9 — Portes et circulation

- [ ] **D9-A :** portes toujours ouvertes pendant les rencontres.
- [X] **D9-B :** interface de porte + réservation des seuils — recommandée.
- [ ] **D9-C :** verrouillage, forçage, panne et droits d'accès complets.

### D10 — Animation initiale

- [X] **D10-A :** capsules et feedback procédural pour valider le cerveau — recommandée d'abord.
- [ ] **D10-B :** humanoïdes et animations génériques dès le début.
- [ ] **D10-C :** rig et animations finales sur mesure.

### D11 — Expérience avancée éventuelle

- [ ] **D11-A :** aucune avant la boucle jouable — recommandée.
- [ ] **D11-B :** moral et reddition.
- [ ] **D11-C :** réseau d'alarme analogique.
- [ ] **D11-D :** témoins/rumeurs.
- [X] **D11-E :** génération LLM hors ligne.
- [ ] **D11-F :** ML-Agents pour tests automatisés.

### D12 — Priorité de production

- [X] **D12-A :** comportement d'abord, art ensuite — recommandée.
- [ ] **D12-B :** personnage visuel et animations d'abord.
- [ ] **D12-C :** prototype parallèle art/comportement, plus coûteux en intégration.

### D13 — Variété des personnages

- [X] **D13-A :** un seul modèle garde et civils .
- [ ] **D13-B :** squelette partagé et kit modulaire — recommandée.
- [ ] **D13-C :** plusieurs personnages entièrement sur mesure.
- [ ] **D13-D :** génération procédurale de foule.

### D14 — Parole des PNJ

- [ ] **D14-A :** barks fixes uniquement.
- [X] **D14-B :** barks combinatoires factuels — recommandée.
- [ ] **D14-C :** interactions à choix courts.
- [ ] **D14-D :** texte généré hors ligne et validé.
- [ ] **D14-E :** conversation générative en direct.

### D15 — Première variation ennemie après le garde

- [X] **D15-A :** aucune avant la stabilisation — recommandée.
- [ ] **D15-B :** même garde avec rôle de radiotéléphoniste.
- [ ] **D15-C :** automate ou drone de sécurité.
- [ ] **D15-D :** mêlée, bouclier ou lourd.

## 18. Ce que contiendra le futur devis de prompts

Après retour de ce document, le devis ne sera pas une liste vague de demandes. Chaque prompt proposé précisera :

- objectif et valeur de jeu ;
- fichiers et scène concernés ;
- préconditions ;
- éléments explicitement hors périmètre ;
- données et interfaces à créer ;
- modifications autorisées ;
- critères d'acceptation observables dans Unity ;
- tests Edit Mode/Play Mode ou protocole manuel ;
- outils de debug exigés ;
- dépendances avec les prompts précédents ;
- risque et stratégie de retour arrière ;
- estimation relative : petit, moyen, grand ou R&D ;
- point de décision nécessitant une validation humaine.

Les prompts seront découpés verticalement. Un prompt devra produire un résultat testable, par exemple « une capsule garde perçoit un bruit et rejoint la dernière position connue », plutôt que créer dix abstractions sans comportement visible.

## 19. Recommandation provisoire

Sous réserve des décisions, la direction la plus équilibrée est :

- option B ;
- garde E2 avec une petite sélection utility en combat ;
- civils C2, blessables mais non mortels dans le premier prototype ;
- HFSM C# explicite ;
- perception vue/bruit, mémoire courte et communication locale ;
- NavMesh + smart objects + réservation obligatoire des portes ;
- comportement validé sur capsules avant acquisition ou création des personnages ;
- aucune IA générative dans le build de la première démo ;
- une seule expérience avancée après stabilisation, idéalement le réseau d'alarme analogique ou le moral/reddition.

Cette combinaison est assez nouvelle pour donner une identité au projet, mais suffisamment contrôlable pour rester compatible avec un premier jeu Unity en développement progressif.

## 20. Références principales

- [Unity AI Navigation](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.navigation.html) — NavMesh, obstacles dynamiques et liens.
- [Unity Behavior](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.behavior.html) — behavior trees officiels et débogage visuel.
- [Unity ML-Agents](https://unity-technologies.github.io/ml-agents/ML-Agents-Overview/) — entraînement et inférence de comportements.
- [Unity AI Inference](https://docs.unity3d.com/6000.0/Documentation/Manual/com.unity.ai.inference.html) — exécution locale de réseaux neuronaux.
- [Unity Animation Rigging](https://docs.unity3d.com/Packages/com.unity.animation.rigging@1.2/manual/RiggingWorkflow.html) — contraintes procédurales, IK et visée.
- [Park et al., *Generative Agents: Interactive Simulacra of Human Behavior*](https://arxiv.org/abs/2304.03442) — mémoire, réflexion, planification et simulation sociale générative.
- [van den Berg et al., *Optimal Reciprocal Collision Avoidance*](https://gamma.cs.unc.edu/ORCA/publications/ORCA.pdf) — évitement local réciproque multi-agent.
