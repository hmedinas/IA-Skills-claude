#!/usr/bin/env python3
"""Sustituye los placeholders __PackageName__ / __BundleId__ / __AppDisplayName__
en todos los archivos de texto de un proyecto generado por generate_app.sh.

Usa reemplazo literal de cadenas (str.replace) en vez de sed/perl para evitar que
secuencias de escape del valor ya escapado (p. ej. "\\u00e9" al insertar JSON, o
"\\'" al insertar Dart) sean reinterpretadas por el motor de sustitución - un bug
real detectado al probar con nombres de app que llevan tildes/eñe.

Uso: apply_placeholders.py <target_dir> <package_name> <bundle_id> <app_name_raw>
"""
import json
import sys
from pathlib import Path
from xml.sax.saxutils import escape as xml_escape

TARGET_DIR, PACKAGE_NAME, BUNDLE_ID, APP_NAME_RAW = sys.argv[1:5]


def escape_dart(s: str) -> str:
    # s se inserta dentro de comillas simples de Dart: 'title: '<esto>''
    return s.replace("\\", "\\\\").replace("'", "\\'").replace("$", "\\$")


def escape_json_value(s: str) -> str:
    # json.dumps con ensure_ascii=False deja tildes/ñ como UTF-8 literal (válido en
    # JSON) en vez de "é"; solo escapa lo que JSON exige (comillas, backslash,
    # control chars). Quitamos las comillas envolventes que añade dumps.
    return json.dumps(s, ensure_ascii=False)[1:-1]


def escape_xml_value(s: str) -> str:
    # xml.sax.saxutils.escape no cubre comillas por defecto; se añaden aparte.
    return xml_escape(s, {'"': "&quot;", "'": "&apos;"})


APP_NAME_DART = escape_dart(APP_NAME_RAW)
APP_NAME_JSON = escape_json_value(APP_NAME_RAW)
APP_NAME_XML = escape_xml_value(APP_NAME_RAW)

EXT_DISPLAY_NAME = {
    ".dart": APP_NAME_DART,
    ".json": APP_NAME_JSON,
    ".xml": APP_NAME_XML,
    ".plist": APP_NAME_XML,
    ".html": APP_NAME_XML,
}

unresolved = []

for path in Path(TARGET_DIR).rglob("*"):
    if not path.is_file():
        continue
    try:
        text = path.read_text(encoding="utf-8")
    except (UnicodeDecodeError, ValueError):
        continue  # archivo binario, se salta

    original = text
    text = text.replace("__PackageName__", PACKAGE_NAME)
    text = text.replace("__BundleId__", BUNDLE_ID)

    display_value = EXT_DISPLAY_NAME.get(path.suffix)
    if display_value is not None:
        text = text.replace("__AppDisplayName__", display_value)

    if text != original:
        path.write_text(text, encoding="utf-8")

    if "__PackageName__" in text or "__BundleId__" in text or "__AppDisplayName__" in text:
        unresolved.append(str(path))

if unresolved:
    print("AVISO: quedaron placeholders sin resolver en:")
    for p in unresolved:
        print(f"  - {p}")
