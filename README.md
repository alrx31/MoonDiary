# MoonDiary

Личный дневник настроения. Название от mood и moon: записи делаются вечером, когда день закончился.

Правила разработки и слои проекта описаны в [docs/dev/conventions.md](docs/dev/conventions.md).

## Тесты

Модульные тесты запускаются командой:

```bash
dotnet test tests/MoonDiary.Tests
```

### UI-тесты на Appium

UI-тесты работают как чёрный ящик через Appium с драйвером UiAutomator2 и предназначены для запуска на Android-эмуляторе. Перед прогоном нужно подготовить среду:

- .NET 10 с workload `android`
- Android SDK и запущенный эмулятор
- Node.js
- Appium и драйвер UiAutomator2

Установка Appium и драйвера:

```bash
npm install -g appium
appium driver install uiautomator2
```

Публикация release APK:

```bash
dotnet publish MoonDiary.csproj -f net10.0-android -c Release
```

Запуск сервера Appium:

```bash
appium
```

Запуск тестов с TRX-отчётом:

```bash
export MOONDIARY_APK_PATH="$PWD/bin/Release/net10.0-android/publish/com.moondiary.app-Signed.apk"
export MOONDIARY_TEST_RESULTS_DIR="$PWD/TestResults/Appium"
dotnet test tests/MoonDiary.UiTests/MoonDiary.UiTests.csproj -c Release --logger "trx;LogFileName=appium.trx" --results-directory "$MOONDIARY_TEST_RESULTS_DIR"
```

Переменные окружения:

- `MOONDIARY_APK_PATH` — обязательный путь к standalone release APK.
- `MOONDIARY_APPIUM_URL` — адрес сервера Appium, по умолчанию `http://127.0.0.1:4723`.
- `MOONDIARY_DEVICE_NAME` — имя устройства, по умолчанию `Android`.
- `MOONDIARY_TEST_RESULTS_DIR` — каталог для TRX-отчёта и скриншотов, по умолчанию `TestResults/Appium`.

При падении теста в каталог результатов сохраняется PNG-скриншот.

> Внимание: прогон использует `fullReset=true`, который удаляет данные приложения на выбранном устройстве. Запускайте UI-тесты только на эмуляторе, чтобы не потерять данные на реальном устройстве.

## Проверки в CI

Workflow `.github/workflows/unit-tests.yml` запускает модульные тесты при каждом pull request и пуше в `main` и `dev`. Gitea Actions читает файлы из `.github/workflows/`, поэтому отдельный workflow для Gitea не нужен.

Что делает job `unit-tests` на `ubuntu-latest`:

- ставит .NET 10;
- выполняет `dotnet test tests/MoonDiary.Tests` с TRX-отчётом `unit.trx` в каталоге `TestResults/Unit`;
- загружает каталог с отчётом артефактом `unit-tests-trx` — это происходит и при упавших тестах.

Приложение в CI не собирается, Android SDK не нужен: тестовый проект ссылается только на `MoonDiary.Core`. UI-тесты на Appium в CI не запускаются, их прогоняют вручную перед релизом.

Скачать отчёт: вкладка Actions → нужный прогон → блок Artifacts → `unit-tests-trx`.

Проверка `unit-tests` становится обязательной в защите ветки. Это настраивает владелец репозитория вручную, один раз на каждом сервере:

- GitHub: Settings → Branches → правило для `dev` (и `main`) → Require status checks to pass before merging → выбрать `unit-tests`.
- Gitea: Settings → Branches → правило для `dev` (и `main`) → включить Status Check Pattern и указать `unit-tests`, либо добавить `unit-tests` в список обязательных проверок правила защиты.
