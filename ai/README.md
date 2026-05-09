# AI Docs Index

Curated documentation for agents and contributors working in this repository.

## Core networking and ECS

- `static_ecs_multiplayer_architecture.md`: high-level architecture for StaticEcs, replication, and Unity Transport.
- `networked_feature_recipes.md`: patterns for implementing replicated gameplay features.
- `static_ecs_reference.md`: quick rules and reminders for day-to-day StaticEcs usage.
- `replication_codegen_notes.md`: current behavior and constraints of replicated component code generation.

## Gameplay and presentation

- `gameplay_systems_memo.md`: gameplay-system boundaries and design reminders.
- `input_feature.md`: input feature structure and intent flow.
- `static_ecs_view_feature.md`: client-side view/presentation feature guidance.
- `ai_combat_feature_research_2026-05-09.md`: research on the new `Ai` and `Combat` features, including architectural issues, undocumented semantics, and system-by-system simplification notes.

## UI and MVC

- `mvc_usage_guidelines.md`: how to use the MVC package as a UX adapter over ECS.
- `mvc_package_review.md`: review notes about the package's strengths, risks, and fit.

## Additional references

- `ECS_Feature_Architecture_Layout_StaticEcs.md`: broader feature/module architecture notes.
- `static-ecs FULL.txt`: deeper StaticEcs API/reference material.
- `prefabxml/`: PrefabXML skill and templates.

## Repository cleanup note

Obsolete implementation plans were removed from `ai/` to keep this folder focused on current reference material instead of archived execution plans.
