import json

with open(r'C:\Users\pavel\.claude\plugins\cache\claude-plugins-official\skill-creator\205b6e0b3036\skills\skill-creator\assets\eval_review.html', 'r') as f:
    template = f.read()

with open(r'C:\Users\pavel\_UnityProjects\StaticMlp\.claude\skills\aspid-mvvm\trigger_evals.json', 'r') as f:
    eval_data = f.read().strip()

desc = (
    'Use when writing, modifying, or reviewing AspidMVVM code — ViewModels, Views, MonoBinders, '
    'RelayCommands, observable collections, or MVVM binding wiring in Unity. Also trigger when the user '
    'mentions Aspid, AspidMVVM, MVVM binding, MonoViewModel, MonoView, MonoBinder, OneWayBind, TwoWayBind, '
    'RelayCommand, ViewModel source generator, or any Aspid.MVVM namespace usage. Trigger even if the user '
    'does not explicitly say "AspidMVVM" but is working with MVVM patterns, data binding, ViewModel/View pairs, '
    'or UI binder components in this project.'
)

template = template.replace('__EVAL_DATA_PLACEHOLDER__', eval_data)
template = template.replace('__SKILL_NAME_PLACEHOLDER__', 'aspid-mvvm')
template = template.replace('__SKILL_DESCRIPTION_PLACEHOLDER__', desc)

with open(r'C:\Users\pavel\_UnityProjects\StaticMlp\.claude\skills\aspid-mvvm\eval_review.html', 'w') as f:
    f.write(template)

print('Done')
