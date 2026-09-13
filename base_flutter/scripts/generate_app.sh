#!/usr/bin/env bash
# Genera una app Flutter nueva a partir de templates/<provider|riverpod>, sustituyendo
# los placeholders __PackageName__ / __BundleId__ / __AppDisplayName__ (y la carpeta
# __BundleSuffix__ de Android) por los valores concretos de la app nueva.
#
# Uso:
#   generate_app.sh <provider|riverpod> <app_name_raw> <package_name> <bundle_suffix> <target_dir>
#
# package_name y bundle_suffix ya deben venir derivados y validados por quien invoca
# el script (ver SKILL.md) - este script solo revalida con regex como defensa en profundidad.
set -euo pipefail

VARIANT="${1:?Uso: generate_app.sh <provider|riverpod> <app_name_raw> <package_name> <bundle_suffix> <target_dir>}"
APP_NAME_RAW="${2:?Falta app_name_raw}"
PACKAGE_NAME="${3:?Falta package_name}"
BUNDLE_SUFFIX="${4:?Falta bundle_suffix}"
TARGET_DIR="${5:?Falta target_dir}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
SKILL_DIR="$(dirname "$SCRIPT_DIR")"

if [[ "$VARIANT" != "provider" && "$VARIANT" != "riverpod" ]]; then
  echo "ERROR: variante debe ser 'provider' o 'riverpod', recibido: $VARIANT" >&2
  exit 1
fi

TEMPLATE_DIR="$SKILL_DIR/templates/$VARIANT"
if [ ! -d "$TEMPLATE_DIR" ]; then
  echo "ERROR: no se encontró la plantilla en $TEMPLATE_DIR" >&2
  exit 1
fi

if [[ ! "$PACKAGE_NAME" =~ ^[a-z][a-z0-9_]*$ ]]; then
  echo "ERROR: package_name inválido: '$PACKAGE_NAME' (debe cumplir ^[a-z][a-z0-9_]*\$)" >&2
  exit 1
fi

if [[ ! "$BUNDLE_SUFFIX" =~ ^[a-z][a-z0-9]*$ ]]; then
  echo "ERROR: bundle_suffix inválido: '$BUNDLE_SUFFIX' (debe cumplir ^[a-z][a-z0-9]*\$)" >&2
  exit 1
fi

if [ -e "$TARGET_DIR" ]; then
  echo "ERROR: '$TARGET_DIR' ya existe. Elige otra ruta/nombre o bórralo primero." >&2
  exit 2
fi

BUNDLE_ID="com.hmedinas.flutterbase.${BUNDLE_SUFFIX}"

echo "==> Copiando plantilla '$VARIANT' a $TARGET_DIR"
mkdir -p "$(dirname "$TARGET_DIR")"
cp -R "$TEMPLATE_DIR" "$TARGET_DIR"

echo "==> Renombrando carpeta de paquete Kotlin (__BundleSuffix__ -> ${BUNDLE_SUFFIX})"
OLD_KOTLIN_DIR="$TARGET_DIR/android/app/src/main/kotlin/com/hmedinas/flutterbase/__BundleSuffix__"
NEW_KOTLIN_DIR="$TARGET_DIR/android/app/src/main/kotlin/com/hmedinas/flutterbase/${BUNDLE_SUFFIX}"
if [ -d "$OLD_KOTLIN_DIR" ]; then
  mkdir -p "$(dirname "$NEW_KOTLIN_DIR")"
  mv "$OLD_KOTLIN_DIR" "$NEW_KOTLIN_DIR"
fi

echo "==> Renombrando archivos con __PackageName__ en su nombre (p. ej. *.iml)"
find "$TARGET_DIR" -depth -iname "*__PackageName__*" -print0 | while IFS= read -r -d '' f; do
  newname="$(dirname "$f")/$(basename "$f" | sed "s/__PackageName__/${PACKAGE_NAME}/g")"
  mv "$f" "$newname"
done

echo "==> Sustituyendo __PackageName__ / __BundleId__ / __AppDisplayName__"
# Delegado a Python (str.replace literal) en vez de sed/perl: un valor ya escapado
# para JSON/Dart (que contiene backslashes, p. ej. "é" o "\'") se corrompe si
# se inyecta como texto de reemplazo de sed/perl, porque esos motores reinterpretan
# las secuencias de escape del propio lenguaje de sustitución (bug real detectado
# al probar con nombres de app con tildes/eñe).
python3 "$SCRIPT_DIR/apply_placeholders.py" "$TARGET_DIR" "$PACKAGE_NAME" "$BUNDLE_ID" "$APP_NAME_RAW"

if command -v flutter >/dev/null 2>&1; then
  echo "==> Ejecutando flutter pub get"
  ( cd "$TARGET_DIR" && flutter pub get ) && echo "OK: flutter pub get" \
    || echo "AVISO: flutter pub get falló, revisa manualmente en $TARGET_DIR"
else
  echo "AVISO: 'flutter' no está en el PATH. Ejecuta manualmente: cd '$TARGET_DIR' && flutter pub get"
fi

echo ""
echo "==> Proyecto generado en: $TARGET_DIR"
echo "==> package_name: $PACKAGE_NAME"
echo "==> bundle id: $BUNDLE_ID"
echo "==> nombre visible: $APP_NAME_RAW"
