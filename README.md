# MoonDiary

Личный дневник настроения. Название от mood и moon: записи делаются вечером, когда день закончился.

Правила разработки и слои проекта описаны в [docs/dev/conventions.md](docs/dev/conventions.md).

## Состав

| Проект | Содержание |
|---|---|
| `src/MoonDiary.Core` | домен, правила, сервисы, ресурсы JSON |
| `src/MoonDiary.Data` | SQLite, миграции, хранилища |
| `src/MoonDiary.Presentation` | ViewModel, навигация, строки интерфейса |
| `MoonDiary.csproj` | приложение MAUI под Android: страницы, компоненты, сервисы |
| `tests/MoonDiary.Tests` | модульные тесты |
| `tests/MoonDiary.UiTests` | UI-тесты на Appium |

## Сборка под Android

Нужны .NET 10 с workload `maui`, Android SDK и JDK. Если SDK и JDK стоят не в стандартных местах, пути передаются свойствами:

```bash
dotnet build MoonDiary.csproj -f net10.0-android -p:AndroidSdkDirectory="$HOME/Library/Android/sdk" -p:JavaSdkDirectory="$HOME/Library/Java/microsoft-jdk"
```

APK появляется в `bin/Debug/net10.0-android/com.moondiary.app-Signed.apk`.

Установить SDK и JDK можно той же командой с целью `InstallAndroidDependencies` и свойством `-p:AcceptAndroidSdkLicenses=True`.

## Тестовые параметры запуска

Отладочная сборка читает параметры из extras интента, релизная их игнорирует.

| Параметр | Значение |
|---|---|
| `moondiary_now` | текущее время, ISO 8601 со смещением: `2026-11-10T18:00:00+03:00` |
| `moondiary_db` | имя файла базы в каталоге приложения, без `/` и `..` |
| `moondiary_seed` | набор данных: `empty` (первый запуск пройден), `month` (месяц записей), `report` (месяц и непоказанный отчёт) |
| `moondiary_open` | что открыть: `wizard` или `letter:<id>` |

Пример:

```bash
adb shell am start -n com.moondiary.app/com.moondiary.app.MainActivity --es moondiary_now "2026-11-10T19:30:00+03:00" --es moondiary_db demo.db --es moondiary_seed month
```

## Тесты

Модульные тесты запускаются командой:

```bash
dotnet test tests/MoonDiary.Tests
```

### UI-тесты на Appium

UI-тесты работают как чёрный ящик через Appium с драйвером UiAutomator2 и запускаются на Android-эмуляторе. Сценарии подменяют время и базу параметрами запуска, поэтому нужен отладочный APK. Перед прогоном нужны:

- Android SDK и запущенный эмулятор
- Node.js
- Appium и драйвер UiAutomator2

Установка Appium и драйвера:

```bash
npm install -g appium
appium driver install uiautomator2
```

Сборка отладочного APK со встроенными сборками:

```bash
dotnet build MoonDiary.csproj -f net10.0-android -c Debug -p:EmbedAssembliesIntoApk=true
```

Запуск сервера Appium:

```bash
appium
```

Запуск тестов с TRX-отчётом:

```bash
export MOONDIARY_APK_PATH="$PWD/bin/Debug/net10.0-android/com.moondiary.app-Signed.apk"
export MOONDIARY_TEST_RESULTS_DIR="$PWD/TestResults/Appium"
dotnet test tests/MoonDiary.UiTests/MoonDiary.UiTests.csproj --logger "trx;LogFileName=appium.trx" --results-directory "$MOONDIARY_TEST_RESULTS_DIR"
```

Переменные окружения:

- `MOONDIARY_APK_PATH` — обязательный путь к отладочному APK.
- `MOONDIARY_APPIUM_URL` — адрес сервера Appium, по умолчанию `http://127.0.0.1:4723`.
- `MOONDIARY_DEVICE_NAME` — имя устройства, по умолчанию `Android`.
- `MOONDIARY_TEST_RESULTS_DIR` — каталог для TRX-отчёта и скриншотов, по умолчанию `TestResults/Appium`.

Каждый сценарий запускает приложение заново (`mobile: startActivity` с остановкой процесса) и работает со своей базой `ui-<id>.db`, основная база приложения не затрагивается. При падении сценария в каталог результатов сохраняется PNG-скриншот.

## Проверки в CI

Workflow `.github/workflows/unit-tests.yml` запускает модульные тесты при каждом pull request и пуше в `main` и `dev`. Gitea Actions читает файлы из `.github/workflows/`, поэтому отдельный workflow для Gitea не нужен.

Что делает job `unit-tests` на `ubuntu-latest`:

- ставит .NET 10;
- выполняет `dotnet test tests/MoonDiary.Tests` с TRX-отчётом `unit.trx` в каталоге `TestResults/Unit`;
- загружает каталог с отчётом артефактом `unit-tests-trx` — это происходит и при упавших тестах.

Приложение в CI не собирается, Android SDK не нужен: тестовый проект ссылается только на библиотеки `MoonDiary.Core`, `MoonDiary.Data` и `MoonDiary.Presentation`. UI-тесты на Appium в CI не запускаются, их прогоняют вручную перед релизом.

Скачать отчёт: вкладка Actions → нужный прогон → блок Artifacts → `unit-tests-trx`.

Проверка `unit-tests` становится обязательной в защите ветки. Это настраивает владелец репозитория вручную, один раз на каждом сервере:

- GitHub: Settings → Branches → правило для `dev` (и `main`) → Require status checks to pass before merging → выбрать `unit-tests`.
- Gitea: Settings → Branches → правило для `dev` (и `main`) → включить Status Check Pattern и указать `unit-tests`, либо добавить `unit-tests` в список обязательных проверок правила защиты.
