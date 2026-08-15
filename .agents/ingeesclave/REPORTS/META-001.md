# META-001 — Comptes rendus par tâche

- Lot : `META-001`
- Statut : `complete`
- Début : `2026-08-14`
- Dernière mise à jour : `2026-08-14`
- Demande source : « je veux aussi que ingésclave réalise compte rendu tache par tache un systèema afin de voir ce qu'il a fait sur tel ou tel tâche ».

## Objectif et périmètre

- Objectif : rendre chaque lot consultable tâche par tâche entre les sessions.
- Inclus : règles du skill, modèles, scripts, validateur, index et rapports historiques P00/P01.
- Exclu : tout travail produit P02 et toute modification de gameplay Unity.

## Tâches

### T01 — Définir le format du registre

- Statut : `complete`
- Action réalisée : séparation entre état courant compact, index global et rapport détaillé par lot.
- Fichiers concernés : conception du skill et modèles `REPORTS`.
- Preuve ou validation : format couvre tâches, fichiers, preuves, résultat observable, limites, feedback et suite.
- Résultat observable : un lecteur peut ouvrir un lot précis sans parcourir tout l'historique.

### T02 — Étendre le skill et ses automatismes

- Statut : `complete`
- Action réalisée : règles tâche par tâche, création idempotente des rapports et validation de leur structure ajoutées.
- Fichiers concernés : copie de préparation `.ingeesclave-skill-build/`.
- Preuve ou validation : initialisation, idempotence et validation sur projet neuf réussies ; skill-creator indique `Skill is valid!`.
- Résultat observable : les futurs lots recevront automatiquement un rapport et une entrée d'index.

### T03 — Reconstituer P00 et P01

- Statut : `complete`
- Action réalisée : rapports historiques créés uniquement depuis la mémoire, les documents, les fichiers et les validations existantes.
- Fichiers concernés : `REPORTS/P00.md`, `REPORTS/P01.md` et `REPORTS/INDEX.md`.
- Preuve ou validation : P01 intègre le feedback utilisateur « 5 test bon » et passe à `complete`.
- Résultat observable : P00 et P01 sont consultables séparément, tâche par tâche.

### T04 — Installer et valider le skill personnel

- Statut : `complete`
- Action réalisée : copie validée installée dans le skill personnel et testée sur un projet neuf.
- Fichiers concernés : skill personnel `C:/Users/simon/.codex/skills/ingeesclave/` et mémoire projet.
- Preuve ou validation : création idempotente de P42, validation de la mémoire avec la copie installée et validation officielle `Skill is valid!` réussies.
- Résultat observable : les prochaines invocations `$ingeesclave` appliqueront automatiquement le registre.

## Validations

- Projet neuf : initialisation, création idempotente d'un rapport et validation réussies.
- Validateur officiel skill-creator : réussi sur la copie de préparation.
- Installation personnelle, test sur projet neuf, validation du projet réel et validateur officiel skill-creator : réussis.

## Comment voir le changement

1. Ouvrir `.agents/ingeesclave/REPORTS/INDEX.md`.
2. Cliquer sur P00, P01 ou META-001.
3. Lire les sections T01, T02, etc. du lot choisi.

## Résultat attendu

- Chaque lot possède un compte rendu autonome et l'index montre immédiatement son statut et sa date.

## Limites et non-vérifié

- Les comptes rendus antérieurs à ce système sont reconstruits depuis les preuves disponibles, pas depuis une télémétrie historique exhaustive.

## Feedback lié

- `FB-003` — demande de compte rendu tâche par tâche.

## Suite

- Prochaine action exacte : attendre la prochaine demande ; ne commencer P02 que sur instruction explicite.
