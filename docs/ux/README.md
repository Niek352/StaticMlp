# UX Status Pipeline

Эта папка фиксирует текущий UX в формате, который можно быстро проверить в Unity: что видно игроку, какие кнопки и input-actions существуют, что они делают, почему могут быть недоступны, и какой статус проверки у каждого действия.

## Правила

- `docs/ux/<feature>.md` - canonical UX status для фичи или vertical slice.
- `ai/refactor/**` и planning summaries могут ссылаться сюда, но не должны становиться единственным источником UX-статуса.
- Каждый feature/task summary, который меняет UI, input, prompt или player-facing flow, должен содержать UX-блок по шаблону [`templates/feature-ux-block.md`](templates/feature-ux-block.md).
- Статус `Реализовано, нужна Unity-проверка` ставится для кода, который статически понятен, но еще не проверен в сцене/префабе.
- Статус `Проверено в Unity` ставится только после ручной проверки в Unity Editor.
- Статус `UX gap` используется для поведения, которое технически существует, но игроку плохо объяснено.
- Статус `Hidden input` используется для input-action, который работает через keyboard/mouse/controller state, но не объяснен в UI.

## Текущие документы

- [`ui-roadmap.md`](ui-roadmap.md) - сводный UI roadmap: что есть сейчас, что нужно сверстать и какие legacy UI части не развивать.
- [`settlement-stage1.md`](settlement-stage1.md) - Settlement HUD, interaction prompt, building management panel, building menu, placement и legacy Stage1 notes.
