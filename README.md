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
export MOONDIARY_APK_PATH="$PWD/bin/Release/net10.0-android/publish/com.companyname.moondiary-Signed.apk"
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
