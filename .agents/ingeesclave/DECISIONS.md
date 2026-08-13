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


