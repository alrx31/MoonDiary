#!/usr/bin/env bash
set -euo pipefail

usage() {
  cat <<'USAGE'
Сборка APK MoonDiary.

  scripts/build-apk.sh [debug|release]

debug    отладочный APK со встроенными сборками, подходит для UI-тестов (по умолчанию)
release  релизный APK

Переменные окружения:
  ANDROID_HOME               путь к Android SDK (по умолчанию ~/Library/Android/sdk)
  JAVA_HOME                  путь к JDK (по умолчанию ~/Library/Java/microsoft-jdk)
  MOONDIARY_KEYSTORE         файл хранилища ключей для подписи релиза
  MOONDIARY_KEY_ALIAS        имя ключа
  MOONDIARY_KEYSTORE_PASS    пароль хранилища
  MOONDIARY_KEY_PASS         пароль ключа (по умолчанию как у хранилища)

Без MOONDIARY_KEYSTORE релиз подписывается отладочным ключом: его можно ставить на своё
устройство, но нельзя публиковать.
USAGE
}

mode="${1:-debug}"
root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
sdk="${ANDROID_HOME:-$HOME/Library/Android/sdk}"
jdk="${JAVA_HOME:-$HOME/Library/Java/microsoft-jdk}"
out="$root/artifacts"

case "$mode" in
  debug) configuration=Debug ;;
  release) configuration=Release ;;
  -h|--help) usage; exit 0 ;;
  *) usage >&2; exit 2 ;;
esac

if [ ! -d "$sdk/platforms" ]; then
  echo "Android SDK не найден в '$sdk'. Задайте ANDROID_HOME." >&2
  exit 1
fi

if [ ! -x "$jdk/bin/java" ]; then
  echo "JDK не найден в '$jdk'. Задайте JAVA_HOME." >&2
  exit 1
fi

args=(
  "$root/MoonDiary.csproj"
  -f net10.0-android
  -c "$configuration"
  -p:AndroidSdkDirectory="$sdk"
  -p:JavaSdkDirectory="$jdk"
  -p:AndroidPackageFormat=apk
  -p:EmbedAssembliesIntoApk=true
  --nologo
  -v minimal
)

if [ "$mode" = release ] && [ -n "${MOONDIARY_KEYSTORE:-}" ]; then
  : "${MOONDIARY_KEY_ALIAS:?задайте MOONDIARY_KEY_ALIAS}"
  : "${MOONDIARY_KEYSTORE_PASS:?задайте MOONDIARY_KEYSTORE_PASS}"
  key_pass_var=MOONDIARY_KEYSTORE_PASS
  [ -n "${MOONDIARY_KEY_PASS:-}" ] && key_pass_var=MOONDIARY_KEY_PASS
  args+=(
    -p:AndroidKeyStore=true
    -p:AndroidSigningKeyStore="$MOONDIARY_KEYSTORE"
    -p:AndroidSigningKeyAlias="$MOONDIARY_KEY_ALIAS"
    -p:AndroidSigningStorePass="env:MOONDIARY_KEYSTORE_PASS"
    -p:AndroidSigningKeyPass="env:$key_pass_var"
  )
fi

dotnet build "${args[@]}"

apk="$root/bin/$configuration/net10.0-android/com.moondiary.app-Signed.apk"

if [ ! -f "$apk" ]; then
  echo "APK не найден: $apk" >&2
  exit 1
fi

version="$(sed -n 's:.*<ApplicationDisplayVersion>\(.*\)</ApplicationDisplayVersion>.*:\1:p' "$root/MoonDiary.csproj")"
target="$out/moondiary-$version-$mode.apk"

mkdir -p "$out"
cp "$apk" "$target"
echo "APK: $target ($(du -h "$target" | cut -f1))"
