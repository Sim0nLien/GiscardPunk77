# Ingéesclave — Mémoire stable de GiscardPunk77

> Dernière revue : 2026-08-13
> Contient uniquement des faits stables et vérifiés. Ne jamais stocker de secret.

## Identité du projet

- Projet : GiscardPunk77
- Objectif stable : construire progressivement une démo FPS solo GiscardPunk 1977 sous Unity 6, sans élargissement silencieux du périmètre.
- Scène d'intégration provisoire observée : `Assets/Scenes/SampleScene.unity` ; `Train.unity` ne contient actuellement qu'une caméra et une lumière. Cette autorité doit être confirmée au lot P00.

## Sources canoniques

- `FPS_DEMO_BACKBONE.md` : vision et décisions approuvées de la première démo.
- `ROADMAP.md` : ordre d'implémentation et portes de sortie.
- `NPC_ETAT_DE_L_ART.md` : exploration et annotations utilisateur concernant les PNJ.
- `NPC_DEVIS_DE_PROMPTS.md` : devis modulaire P00–P19, dépendances et critères.

## Invariants architecturaux

- Pour les PNJ, viser `Core <- Gameplay <- AI` sans dépendance circulaire.
- Unity Behavior doit orchestrer des composants C# testables, pas contenir navigation, raycasts ou dégâts.
- Un seul lot principal doit être implémenté et validé à la fois.

## Conventions durables

- Préserver les modifications et assets utilisateur déjà présents dans le worktree.
- Utiliser `apply_patch` pour les éditions manuelles.
- Valider les comportements sur capsules avant l'art et les animations finales.

## Commandes validées

- `python <skill-creator>/scripts/quick_validate.py <skill>` valide la structure d'un skill ; PyYAML 6.0.3 est installé dans le profil Python utilisateur.
- `scripts/init_project_memory.ps1 -ProjectRoot <path>` initialise la mémoire Ingéesclave sans écraser les fichiers existants.

## Risques et pièges connus

- Le worktree contient de nombreuses modifications utilisateur suivies et non suivies : toujours inspecter `git status` avant une édition.
- `SampleScene.unity` est volumineuse et modifiée ; aucune intégration PNJ avant P16.
- Le prototype `CapsuleWeapon` utilise des projectiles physiques alors que le backbone prévoit du hitscan ; divergence à traiter dans P03.

## Préoccupations stables ouvertes

- Confirmer formellement la scène d'intégration au lot P00.
- Unity Behavior n'est pas encore installé ; installation réservée au lot P10.


