# Шаблон UX-блока фичи

Добавляй этот блок в feature plans, summaries и refactor notes, если задача меняет UI, input, prompt, окно, панель или player-facing flow.

## UX Status

**Видимые controls**
- `<View/Panel>`: `<Button/control label>` - `<что видит игрок>`.

**Focus condition**
- `<когда объект попадает в focus/selection>` - `<какой state/resource/component это публикует>`.

**Prompt text**
- `<condition>` - `Press <input> to <action>`.

**Opens panel/window**
- `<input/control>` - `<controller/view/session>` - `<когда закрывается>`.

**Скрытые inputs**
- `<InputActionName>` - `<когда активен>` - `<что запускает>`.

**Карта действий**
| Действие игрока | UI/input source | System/request/event | Result |
|---|---|---|---|
| `<action>` | `<button/input>` | `<controller/system/request/event>` | `<effect>` |

**Правила доступности**
| Control | Доступен когда | Disabled reason |
|---|---|---|
| `<control>` | `<state condition>` | `<text or gap>` |

**Текущий UX status**
| Area | Status | Notes |
|---|---|---|
| `<area>` | `Реализовано, нужна Unity-проверка` | `<known gap or verification note>` |

**Unity verification notes**
- [ ] `<ручная проверка в Unity Editor>`
- [ ] `<ручная проверка в Unity Editor>`
