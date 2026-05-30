# Refactor Backlog

This folder contains architecture and refactor work that should not be mixed with product/GDD direction documents.

## Plans

- [MVVM StaticEcs context handoff](mvvm-static-ecs/00_context_handoff.md): fresh-context handoff for the corrected Aspid.StaticEcs + Aspid.MVVM windows migration.
- [MVVM StaticEcs architecture decision](mvvm-static-ecs/01_architecture_decision.md): target package boundaries and required ECS-to-ViewModel window linking API.
- [MVVM StaticEcs migration plan](mvvm-static-ecs/02_migration_plan.md): phased migration order for feature views and ViewModels.
- [06.02 Presentation State Problem Fixing](settlement/06.02_problem_fixing.md): executable plan for fixing `06_problem_presentation_state_flattening.md`.

## Audits

- [Settlement refactor audit](settlement/01_settlement_ux_overview.md): current settlement UX and architecture map.
- [Resource hardcoding](settlement/02_problem_resource_hardcoding.md): resource fields and labels that should become catalog-driven.
- [Interaction nonsystemic](settlement/03_problem_interaction_nonsystemic.md): hardcoded interaction priority/actions.
- [Label duplication](settlement/04_problem_label_duplication.md): duplicated labels across presentation paths.
- [Flow objective hardcoding](settlement/05_problem_flow_objective_hardcoding.md): objective/hint mapping debt.
- [Presentation state flattening](settlement/06_problem_presentation_state_flattening.md): monolithic HUD state and cross-feature presentation reads.
