# AiTaskExecution Feature Guide

Short operational rules for `StaticMlp.Features.AiTaskExecution`.

## Purpose

- `AiTaskExecution` owns server-side execution of already selected bot tasks.
- It is intentionally downstream from AI decision-making and upstream from navigation/state mirroring.

## Runtime Shape

- Execution runs as a single server pipeline step after utility selection and before navigation.
- `ServerAiTaskExecutionSystem` is a facade only: query entities, resolve executor, run executor.
- Task-specific behavior lives in `../AiActions/Runtime/Actions/*`; shared task transition helpers live in `../AiBots/Runtime/Actions/AiTaskExecutionTransitions.cs`.

## Boundaries

- Do not move behavior selection, perception, blackboard filling, navigation backend work, or replicated presentation state into this feature.
- Do not add Unity Transport, packet, or raw network inbox logic here.
- Keep public gameplay contracts in `StaticMlp.Features.AiBots`; this feature should stay a thin orchestration facade.

## Where To Start

- Read `Runtime/AiTaskExecutionGameplayFeature.cs`.
- Then read `Runtime/Systems/Server/ServerAiTaskExecutionSystem.cs`.
- Then inspect `../AiActions/Runtime/Actions/*`.
