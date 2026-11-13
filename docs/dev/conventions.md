# Соглашения разработки MoonDiary

## Слои

| Проект | Что в нём | Зависимости |
|---|---|---|
| `src/MoonDiary.Core` | домен, правила, сервисы, интерфейсы хранилищ (`Abstractions/`), ресурсы JSON | только BCL |
| `src/MoonDiary.Data` | SQLite: `SqliteConnectionFactory`, миграции `Migrations/NNN_name.sql`, хранилища `Repositories/` | Core, Microsoft.Data.Sqlite |
| `src/MoonDiary.Presentation` | ViewModel, интерфейсы платформенных сервисов, форматирование, строки интерфейса | Core, CommunityToolkit.Mvvm; без MAUI |
| `MoonDiary.csproj` (корень) | страницы `Views/`, компоненты `Controls/`, дизайн-система `Resources/Styles/`, реализации сервисов `Services/`, Android `Platforms/Android/` | всё выше, MAUI |
| `tests/MoonDiary.Tests` | xUnit для Core, Data, Presentation | Core, Data, Presentation |
| `tests/MoonDiary.UiTests` | Appium, чёрный ящик по APK | без ссылок на проекты приложения |

## Код

- Комментарии в коде не писать. Исключение — неочевидный частный случай, тогда одна строка о нём.
- namespace совпадает с путём папки: `src/MoonDiary.Core/Calendar/DayMood.cs` → `MoonDiary.Core.Calendar`.
- Один публичный тип на файл, имя файла равно имени типа.
- Сущности домена — `sealed class` со свойствами `get; set;`. Результаты расчётов — `sealed record`.
- Асинхронные методы возвращают `Task`/`Task<T>` и называются с суффиксом `Async`.
- Ошибка, которую нужно показать пользователю, — `DomainException` с сообщением на русском.
- Предупреждения компилятора в библиотеках и тестах — ошибки сборки.
- Новый пакет добавляется версией в `Directory.Packages.props`, в проекте — `PackageReference` без версии.

## Время

- Текущее время — только через `TimeProvider` и `DayClock`. `DateTime.Now`, `DateTime.UtcNow`, `DateTimeOffset.Now`, `DateTimeOffset.UtcNow` запрещены везде, кроме `Platforms/`.
- День (dayKey) — `DateOnly`, длится с 04:00 до 04:00. Моменты времени — `DateTime` с `Kind = Utc`.

## Хранение

| Тип | В SQLite |
|---|---|
| `Guid` | TEXT, `ToString("D")` |
| `DateOnly` | TEXT `yyyy-MM-dd` |
| `DateTime` UTC | TEXT `ToString("O")` |
| `TimeOnly` | TEXT `HH:mm` |
| перечисление | TEXT, имя в camelCase (`evening`, `notRelevant`) |
| `bool` | INTEGER 0/1 |
| списки | TEXT, JSON-массив |

- Каждый метод хранилища открывает своё соединение `using var c = factory.Open();`.
- SQL только с параметрами (`$id`), без склейки строк.
- Изменение схемы — новый файл миграции. Уже влитые миграции не менять.

## Интерфейс

- Страница наследует `ViewModelPage`, логика — только во ViewModel из Presentation.
- Сервисы, хранилища, страницы и ViewModel регистрируются явно в `MauiProgram.cs`.
- Четыре вкладки (`CalendarTab`, `AnalyticsTab`, `LettersTab`, `UserTab`) наследуют `TabView` и живут внутри одной страницы `HomePage` вместе с нижним меню. Переключение идёт через `INavigationService.GoToTabAsync`.
- Системные окна Android не используются. Подтверждения, меню и выбор времени показывает `IDialogService` нижней панелью (`Controls/SheetViews.cs`). `DatePicker`, `TimePicker`, `Picker`, `DisplayAlert` и `DisplayActionSheet` на страницах запрещены.
- Кнопку «Назад» страница обрабатывает в `OnBack`, `OnBackButtonPressed` занят закрытием панели.
- Цвета для рисуемых компонентов (`CalendarView`, `MoodScaleView`, `DynamicsChartView`) берутся через `Services/Palette.cs`, он читает те же ресурсы `Resources/Styles/Colors.xaml`.
- Цвета, размеры шрифтов, отступы и радиусы — только ресурсы дизайн-системы из `Resources/Styles/`. HEX-цвета и числовые `FontSize`, `Padding`, `Margin`, `Spacing` в XAML страниц запрещены.
- Экран собирается из компонентов `Controls/` (`ScreenHeader`, `Card`, `ListRow`, `EmptyState`, `Chip`, `MoodScaleView` и т. д.). Новый общий элемент сначала добавляется в компоненты и на витрину компонентов.
- Тексты интерфейса — константы класса `src/MoonDiary.Presentation/Strings.cs`, имя `<Экран>_<Элемент>` (`Wizard_Save`). В XAML — через `x:Static`.
- `AutomationId` есть у каждого элемента, который нажимает или проверяет тест: PascalCase на английском, с префиксом экрана (`WizardSaveButton`). Опубликованный `AutomationId` не переименовывать.
- Нажимаемая область не меньше 44×44.
- Элементам с текстом не задавать фиксированную высоту, чтобы работал системный масштаб шрифта.
- Цвет не единственный признак состояния: выбор и уровень дублируются формой, заливкой или длиной.

## Тесты

- Тест класса лежит по тому же пути: `src/MoonDiary.Core/Calendar/CalendarService.cs` → `tests/MoonDiary.Tests/Core/Calendar/CalendarServiceTests.cs`.
- Общие помощники — только из `tests/MoonDiary.Tests/Support/` (`TestTime`, `TestDatabase`, построители, заглушки). Свои копии помощников в тестах не заводить, недостающее добавлять в `Support/`.
- Имя теста: `Метод_Условие_Результат`.
- UI-тесты: объекты страниц в `tests/MoonDiary.UiTests/Pages/`, сценарии — через `RunScenario`, элементы — только по `AutomationId`.

## Проверка

- Каждая задача: `dotnet test tests/MoonDiary.Tests` проходит без предупреждений.
- На машине исполнителя может не быть Android SDK. Если `dotnet build MoonDiary.csproj -f net10.0-android` падает из-за этого, а не из-за кода, записать это в «Результат» задачи.

## Git

- Одна задача — одна ветка и один PR.
- Сообщение коммита — коротко по факту, с номером задачи: `012: недели в DayClock`. Без упоминаний ИИ и строк `Co-Authored-By`.
