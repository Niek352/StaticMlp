# Aspid.MVVM Guide

This reference provides a concise overview of the **Aspid.MVVM** framework for
Unity. Use it when you need deeper context while building MVVM components
through the skill.

## Основные концепции

* **Без рефлексии**. Привязки работают без отражения (reflection) и
  boxing/unboxing, обеспечивая высокую производительность. Поддерживаются
  четыре режима: `OneWay` (View обновляется при изменении ViewModel),
  `TwoWay` (двусторонняя синхронизация), `OneTime` (значение устанавливается
  один раз) и `OneWayToSource` (ViewModel обновляется при изменении View)【24777377164018†L282-L289】【24777377164018†L296-L299】.
* **Гибкие ViewModel**. Любой класс можно превратить в ViewModel, пометив
  его атрибутом `[ViewModel]` и сделав `partial`. Не требуется наследование
  от специальных базовых классов【48126929028137†L24-L31】. Специальные варианты `MonoViewModel` и
  `ScriptableViewModel` обеспечивают сериализацию и уведомления об изменениях
  в редакторе【48126929028137†L49-L76】.
* **Команды**. Механизм команд основан на интерфейсе `IRelayCommand`. Команды
  создаются через класс `RelayCommand` или атрибут `[RelayCommand]`
  для методов. Используйте `NotifyCanExecuteChanged()` для обновления
  условия выполнения【774945376988087†L33-L69】【932601311377628†L198-L225】.
* **Биндеры**. Интерфейсы `IBinder<T>` и `IAnyBinder` позволяют создавать
  пользовательские биндеры для UI‑компонентов. Наследуйтесь от `Binder`
  или `MonoBinder` и реализуйте метод `SetValue(T value)` для каждого
  поддерживаемого типа【571141673724075†L19-L36】【571141673724075†L72-L111】.

## ViewModel и привязки

Внутри ViewModel данные для привязки объявляются обычными полями и
помечаются следующими атрибутами:

| Атрибут               | Назначение                                 |
|-----------------------|--------------------------------------------|
| `[Bind]`              | Автоматически определяет режим привязки    |
| `[OneWayBind]`        | Обновляет View при изменении ViewModel     |
| `[TwoWayBind]`        | Синхронизирует View и ViewModel            |
| `[OneTimeBind]`       | Устанавливает значение один раз            |
| `[OneWayToSourceBind]`| Обновляет ViewModel при изменении View     |

По этим атрибутам Source Generator генерирует свойства, методы установки
значений и события изменения значения【932601311377628†L168-L207】.

Команды в ViewModel создаются двумя способами:

1. **Вручную**: создайте `RelayCommand` в конструкторе и передайте
   делегаты `Execute` и `CanExecute`. При изменении условий вызовите
   `NotifyCanExecuteChanged()`【774945376988087†L60-L69】.
2. **Автоматически**: пометьте метод `[RelayCommand]`, и генератор создаст
   свойство `<MethodName>Command` типа `IRelayCommand`【932601311377628†L198-L225】.

Для сокращения кода существуют методы расширения: `GetSelfOrEmpty()`,
`CreateCommand()`, `CreateCommandOrEmpty()` и `CreateCommandWithoutParameters()`.

## View и инициализация

View — это класс, наследуемый от `MonoView` или `ScriptableView`, помеченный
атрибутом `[View]`. Для каждого связуемого элемента в View нужно объявить
сериализованное поле типа `MonoBinder` или `MonoBinder[]` и пометить его
`[RequireBinder(typeof(T))]`, где `T` — тип данных или `IRelayCommand` для
команд. Имена полей должны соответствовать именам полей в ViewModel【932601311377628†L41-L58】【932601311377628†L104-L113】.

Свяжите View и ViewModel через вызов `Initialize(IViewModel viewModel)` на
экземпляре View. При повторной инициализации сначала вызовите
`Deinitialize()` и затем `Initialize()`; методы расширения `DeinitializeView()`
и `DisposeViewModel()` помогают освободить ресурсы【578480834522145†L86-L133】【578480834522145†L137-L154】.

## Собственные биндеры

Чтобы создать свой биндер:

1. Наследуйтесь от `Binder` или `MonoBinder`.
2. Реализуйте интерфейс `IBinder<T>` для каждого поддерживаемого типа, либо
   `IAnyBinder` для универсальной привязки.
3. В методе `SetValue(T value)` обновляйте компонент UI. Для поддержки
   нескольких типов реализуйте несколько `SetValue` с разными параметрами【571141673724075†L19-L36】【571141673724075†L72-L111】.

Пример комбинированного биндера для отображения строк и чисел в `TMP_Text`:

```csharp
public class TextMonoBinder : MonoBinder, IBinder<string>, IBinder<float>
{
    [SerializeField] private TMP_Text _text;

    public void SetValue(string value) => _text.text = value;
    public void SetValue(float value) => _text.text = value.ToString(CultureInfo.InvariantCulture);
}
```

## Рекомендации

* Разделяйте логику View и ViewModel: UI‑код остаётся в View, бизнес‑логика — в
  ViewModel.
* Не выполняйте длительные операции в UI‑потоке; используйте команды для
  взаимодействия с сервисами.
* Проверяйте корректность биндера в редакторе Unity. `[BinderLog]` поможет
  отлаживать изменения значений【24777377164018†L344-L349】.
* Используйте `ObservableList`, `ObservableDictionary`, `ObservableStack` и
  другие коллекции для удобной работы с динамическими списками【24777377164018†L310-L323】.

Этот документ содержит основные сведения о библиотеке; дополнительные
разделы примеров и руководств находятся в официальной документации
(https://vpd-inc.gitbook.io/aspid.mvvm/).