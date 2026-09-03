#!/usr/bin/env bash
# Genera un backend nuevo a partir de la plantilla reference/, sustituyendo placeholders
# y compilando el resultado. Uso:
#   scaffold.sh <Empresa> <Aplicacion> <Database> <TargetDir>
set -euo pipefail

EMPRESA="${1:?Uso: scaffold.sh <Empresa> <Aplicacion> <Database> <TargetDir>}"
APLICACION="${2:?Falta Aplicacion}"
DATABASE="${3:?Falta nombre de base de datos}"
TARGET_DIR="${4:?Falta directorio destino}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REFERENCE_DIR="$SCRIPT_DIR/reference"
ROOT_NS="${EMPRESA}.${APLICACION}"
YEAR="$(date +%Y)"

if [ ! -d "$REFERENCE_DIR" ]; then
  echo "ERROR: no se encontró la plantilla en $REFERENCE_DIR" >&2
  exit 1
fi

if [ -e "$TARGET_DIR" ]; then
  echo "ERROR: el directorio destino '$TARGET_DIR' ya existe. Elige otro nombre o bórralo primero." >&2
  exit 1
fi

sed_inplace() {
  if sed --version >/dev/null 2>&1; then
    sed -i "$@"      # GNU sed
  else
    sed -i '' "$@"   # BSD/macOS sed
  fi
}

echo "==> Copiando plantilla a $TARGET_DIR"
mkdir -p "$TARGET_DIR"
cp -R "$REFERENCE_DIR"/. "$TARGET_DIR"/

echo "==> Renombrando archivos/carpetas con __RootNamespace__ -> ${ROOT_NS}"
find "$TARGET_DIR" -depth -name '*__RootNamespace__*' | while IFS= read -r path; do
  newpath="$(dirname "$path")/$(basename "$path" | sed "s/__RootNamespace__/${ROOT_NS}/g")"
  mv "$path" "$newpath"
done

echo "==> Sustituyendo placeholders en el contenido de los archivos"
find "$TARGET_DIR" -type f | while IFS= read -r file; do
  sed_inplace \
    -e "s/__RootNamespace__/${ROOT_NS}/g" \
    -e "s/__Empresa__/${EMPRESA}/g" \
    -e "s/__Aplicacion__/${APLICACION}/g" \
    -e "s/__Database__/${DATABASE}/g" \
    -e "s/__Year__/${YEAR}/g" \
    "$file"
done

echo "==> Proyecto generado en: $TARGET_DIR (namespace raíz: ${ROOT_NS})"

DOTNET_BIN="$HOME/.dotnet/dotnet"
if [ ! -x "$DOTNET_BIN" ]; then
  DOTNET_BIN="dotnet"
fi

HOST_CSPROJ="$TARGET_DIR/src/${ROOT_NS}.Host/${ROOT_NS}.Host.csproj"

echo "==> Inicializando User Secrets en el Host"
"$DOTNET_BIN" user-secrets init --project "$HOST_CSPROJ"

echo "==> Compilando la solución"
"$DOTNET_BIN" build "$TARGET_DIR/${ROOT_NS}.slnx"

echo "==> Listo."
