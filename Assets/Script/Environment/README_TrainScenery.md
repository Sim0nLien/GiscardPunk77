# Décor procédural du train

La scène `Assets/Scenes/SampleScene.unity` contient l'objet **Procedural Exterior Scenery**. Le train reste immobile et cet objet fait défiler des éléments extérieurs recyclés en boucle.

Le composant `LoopingGround` crée aussi un sol sans collider, composé de segments recyclés. Il lit directement `Train Speed` et `Movement Direction` depuis `TrainScenerySystem` : le sol reste donc exactement synchronisé avec le décor proche. L'objet **Sun** utilise `SunLight`, une classe qui configure une `UnityEngine.Light` directionnelle et l'enregistre comme soleil de la scène.

## Test immédiat

Lancer `SampleScene` en Play mode. Sans aucun prefab supplémentaire, trois couches sont générées :

- poteaux proches ;
- arbres à moyenne distance ;
- bâtiments lointains plus lents pour créer la parallaxe.

## Réglages principaux

Sélectionner **Procedural Exterior Scenery** dans la Hierarchy :

- `Train Speed` : vitesse globale du décor ;
- `Movement Direction` : direction locale du défilement, normalement `(0, 0, -1)` ;
- `Random Seed` : conserver la même valeur pour reproduire exactement la même distribution ;
- `Randomize Seed At Runtime` : obtenir une nouvelle distribution à chaque partie ;
- `Fill Visible Area At Start` : éviter que l'extérieur soit vide au lancement.

Chaque élément de `Layers` est une famille de décor indépendante.

- `Prefabs` : glisser un ou plusieurs prefabs. Si la liste est vide, la forme simple `Generated Shape` est utilisée ;
- `Side` : gauche, droite, ou les deux côtés du train ;
- `X Distance` : distance horizontale minimale et maximale depuis le centre du train ;
- `Y Position` : hauteur minimale et maximale ;
- `Interval Seconds` : delta t aléatoire entre deux apparitions ;
- `Start Delay` : décalage temporel propre à cette famille ;
- `Speed Multiplier` : `1` pour le premier plan, environ `0.4` à `0.8` pour les plans lointains ;
- `Spawn/Despawn Distance` : longueur de la boucle devant et derrière le train ;
- `Minimum/Maximum Scale` et `Y Rotation` : variations visuelles aléatoires ;
- `Pool Size` : nombre maximum d'objets recyclés pour la couche.

### Orientation indépendante par côté

`Y Rotation` reste la variation aléatoire commune. Les champs `Left Side Y Rotation` et `Right Side Y Rotation` s'y ajoutent respectivement pour les objets à gauche (X négatif) et à droite (X positif) du train.

Dans `SampleScene`, les pylônes proches utilisent maintenant le prefab `Prefabs/Train/pilone_electrique`. Les bâtiments ont comme réglage initial `+90°` à gauche et `-90°` à droite : leur façade est donc tournée vers le train sur les deux côtés. Modifiez ces deux valeurs dans la couche concernée si l'orientation native d'un nouveau prefab est différente.

La distance moyenne entre deux objets suit cette relation :

`espacement = Train Speed × Speed Multiplier × Interval Seconds`

Exemple : à une vitesse de `18`, un multiplicateur de `1` et un delta t de `0.8 s`, les objets sont espacés d'environ `14.4 m`.

## Ajouter un nouveau type de décor

1. Ajouter un élément à `Layers`.
2. Lui donner un nom, par exemple `Rochers proches`.
3. Glisser les prefabs possibles dans `Prefabs`.
4. Régler `X Distance`, `Y Position`, `Interval Seconds` et `Start Delay`.
5. Garder `Disable Colliders` activé pour que le paysage extérieur n'interagisse jamais avec le joueur.

Les lignes colorées visibles quand l'objet est sélectionné représentent les limites de spawn et de recyclage de chaque couche.
