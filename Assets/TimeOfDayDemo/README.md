# Enviro 3 — управление временем суток

Готовая демонстрационная сцена: `Assets/Scenes/Test/Time of Day Demo.unity`.

## Что подключено

- кнопка «Утро» — 08:00;
- кнопка «День» — 13:00;
- кнопка «Вечер» — 19:00;
- кнопка «Ночь» — 00:00;
- ползунок позволяет выбрать любое время от 0 до 24 часов;
- текущее выбранное время выводится над ползунком;
- переключатель «Динамическое время» включает и останавливает автоматическое течение времени;
- после ручного выбора часа автоматическое течение времени останавливается;
- демонстрационная сцена запускается с погодой `Cloudy 1`, чтобы объёмные облака были видны.

## UI и URP

Интерфейс собран на элементах исходного проекта заказчика:

- `Assets/UI/Prefabs/Buttons/_Base Button.prefab`;
- `Assets/UI/Prefabs/Test/Test Slider.prefab`;
- `Assets/Common/Fonts/GothaProReg SDF.asset`;
- `Assets/UI/Textures/Icons/Circle Solid.psd`.

Для облаков и тумана включён define `ENVIRO_URP`, а в
`Assets/Settings/Rendering/PC_Renderer.asset` добавлен `Enviro 3 Render Feature`.

## Подключение из кода

Главный вызов Enviro 3:

```csharp
Enviro.EnviroManager.instance.Time.SetTimeOfDay(13f);
```

Либо можно получить компонент `EnviroTimeOfDayUI` и вызвать:

```csharp
timeOfDayUI.SetHour(15.5f); // 15:30
timeOfDayUI.SetMorning();
timeOfDayUI.SetDay();
timeOfDayUI.SetEvening();
timeOfDayUI.SetNight();
timeOfDayUI.SetDynamicTime(true);
```

Компонент находится в `Assets/TimeOfDayDemo/Scripts/EnviroTimeOfDayUI.cs`.

Для повторного создания тестовой сцены доступно меню:
`Tools > Tractor Simulator > Build Time of Day Demo`.
