---
name: execute-plan
description: Execute a PLAN.md file directly with intelligent segmentation. Use when user asks to execute a planning PLAN.md by path.
---

# Execute PLAN.md

When invoked, execute the plan at the provided path.

Required argument:
- plan_path: path to PLAN.md

Process:
1. Read plan_path.
2. If file does not exist, stop with an error.
3. Check same directory for SUMMARY.md.
4. If SUMMARY.md exists, report that the plan was already executed and ask before re-running.
5. Parse sections:
   - <objective>
   - <execution_context>
   - <context>
   - <tasks>
   - <verification>
   - <success_criteria>
6. Read execution_context first.
7. Read only files explicitly mentioned in execution_context and context.
8. Detect checkpoints with:
   grep "type=\"checkpoint" "$plan_path"

Routing:
- No checkpoints: execute fully autonomous.
- Only checkpoint:human-verify: execute in autonomous segments between checkpoints.
- checkpoint:decision or checkpoint:human-action: execute sequentially and block for user input.

Rules:
- Do not invoke planning skills.
- Follow execute-phase.md protocol if referenced.
- Track all deviations in SUMMARY.md.
- Never skip checkpoint interaction.
- Run verification before marking complete.
- Create SUMMARY.md in the same directory.
- Update ROADMAP.md if referenced by the plan.
- Commit changes with:
  feat({phase}-{plan}): [summary]

Final response must include:
- tasks completed
- files modified
- verification results
- commit hash
