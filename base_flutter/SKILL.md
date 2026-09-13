---
name: base_flutter
description: Genera una app Flutter nueva a partir de las plantillas base_flutter_provider/base_flutter_riverpod (Clean Architecture feature-first, lib/core + lib/features, Provider o Riverpod a elección), renombrando pubspec, imports, título visible, config JSON y todos los identificadores nativos (Android/iOS/macOS/Linux/Windows/Web) con bundle id com.hmedinas.flutterbase.<derivado>. Usa esta skill siempre que el usuario pida crear una nueva app Flutter, un "starter"/"boilerplate"/plantilla base de Flutter, o clonar/renombrar una de estas dos plantillas, aunque no mencione "base_flutter" por su nombre. Solo hace rename de identificadores, no limpieza de scaffolding ni fixes funcionales.
---

# base_flutter — Generador de apps Flutter (Provider / Riverpod)

Esta skill crea, en un directorio nuevo, una app Flutter completa a partir de una de dos plantillas gemelas (arquitectura feature-first: `lib/core/` + `lib/features/`, cliente Dio con mocks, config por entorno, theming, routing con rutas nombradas, sesión + login funcionando), eligiendo el gestor de estado (Provider o Riverpod) y renombrando todos los identificadores (paquete Dart, bundle id nativo, nombre visible) al nombre que indique el usuario.

**Alcance: solo rename.** No limpia carpetas/archivos de scaffolding vacíos de la plantilla (`lib/core/errors/`, `lib/core/extensions/`, `lib/core/helpers/`, `lib/core/api/base_api_service.dart`, `lib/features/error_page/presentation/screens/unauthorized_screen.dart`), no implementa dark mode, y no conecta `lib/features/initialization/presentation/screens/splash_screen.dart` con la sesión real (sigue con `hasToken = false` simulado). Todo eso queda igual que en la plantilla base — díselo al usuario en el reporte final para que no le sorprenda.

## Paso 1: Recolectar datos

Usa `AskUserQuestion` (o toma lo que el usuario ya haya dado en su mensaje) para conseguir:

- **Nombre de la app** (texto libre, ej. "Ventas Pro"). Obligatorio; si viene vacío o solo espacios, vuelve a pedirlo — no continúes.
- **Gestor de estado**: `Provider` o `Riverpod`. Dale contexto breve si el usuario no sabe cuál elegir:
  - *Provider* → `MultiProvider` + `ChangeNotifierProvider`, estilo más simple/imperativo (`context.watch/read`).
  - *Riverpod* → `ProviderScope` + `NotifierProvider` (uno con `.autoDispose` para el form de login, otro persistente para la sesión), más type-safe y testeable (`ref.watch/read`).
- **Directorio destino**: si el usuario no lo indica, usa `./<package_name>` relativo al directorio de trabajo actual (resuelve y muestra la ruta absoluta).

### Derivar `package_name` desde el nombre de la app

1. Normaliza Unicode (NFD) y quita diacríticos: á→a, é→e, í→i, ó→o, ú→u, ñ→n, ü→u.
2. Minúsculas. Cualquier carácter fuera de `[a-z0-9]` → `_`. Colapsa `_` repetidos. Quita `_` al inicio/fin.
3. Si queda vacío → pide el nombre de nuevo, no continúes.
4. Si empieza con dígito, o es una palabra reservada de Dart (`class`, `import`, `export`, `void`, `final`, `const`, `var`, `in`, `is`, `new`, `this`, `true`, `false`, `null`, `if`, `else`, `for`, `while`, `return`, `extends`, `with`, `on`, `async`, `await`, etc.) → antepón `app_`.
5. Ejemplos: `"Ventas Pro"` → `ventas_pro`. `"Café con Ñ"` → `cafe_con_n`. `"Class"` → `app_class`.

### Derivar `bundle_suffix` desde `package_name`

1. Parte de `package_name` ya derivado y quita todos los `_`.
2. Si queda vacío, empieza con dígito, o es palabra reservada de Java/Kotlin (`class`, `interface`, `package`, `import`, etc.) → antepón `app`.
3. Ejemplo: `ventas_pro` → `ventaspro` → bundle id final `com.hmedinas.flutterbase.ventaspro`.

Muéstrale al usuario un resumen de confirmación (nombre de app, variante elegida, `package_name`, bundle id completo, ruta destino resuelta) antes de ejecutar el script. Si `package_name` o `bundle_suffix` recibieron el prefijo `app_`/`app` por colisión con palabra reservada, menciónalo explícitamente para que no le extrañe.

## Paso 2: Generar el proyecto

```bash
bash ~/.claude/skills/base_flutter/scripts/generate_app.sh "<provider|riverpod>" "<NombreAppTalCual>" "<package_name>" "<bundle_suffix>" "<TargetDir>"
```

El script:
1. Copia `templates/<variant>/` a `<TargetDir>`.
2. Renombra la carpeta Kotlin `__BundleSuffix__` al `bundle_suffix` real.
3. Sustituye `__PackageName__` → `package_name` y `__BundleId__` → `com.hmedinas.flutterbase.<bundle_suffix>` en todos los archivos de texto (pubspec, imports Dart, `build.gradle.kts`, `MainActivity.kt`, `project.pbxproj`, `Info.plist`, `AppInfo.xcconfig` de macOS, `CMakeLists.txt` de Linux/Windows, `.iml`).
4. Sustituye `__AppDisplayName__` → el nombre de app tal cual lo escribió el usuario, con el escapado correcto según el tipo de archivo (Dart, JSON, XML/plist/HTML) — aparece en el `title` de `MaterialApp`, en `nameApplication` de los 4 JSON de `assets/config/`, en `android:label`, en `CFBundleDisplayName`/`CFBundleName` de iOS, y en el `manifest.json`/`index.html` de Web.
5. Corre `flutter pub get` si `flutter` está en el PATH.

Falla (sin tocar nada) si `<TargetDir>` ya existe (código de salida 2) — si eso pasa, pregunta al usuario por otra ruta o su confirmación explícita para borrar el directorio existente (nunca lo borres por tu cuenta).

Si el script reporta placeholders sin resolver (línea "AVISO: quedaron placeholders sin resolver"), es un bug en la plantilla — revisa manualmente esos archivos antes de dar el trabajo por terminado.

## Paso 3: Reportar al usuario

- Ruta absoluta del proyecto generado.
- `package_name` y bundle id resultante (mismo bundle id en Android, iOS, macOS y Linux).
- Cómo correrlo: `cd <TargetDir> && flutter run --dart-define=ENVIRONMENT=dev` (si `flutter pub get` no corrió solo o falló, recuérdalo primero).
- Recordatorio explícito: esto fue **solo un rename**. Los huecos conocidos de la plantilla base siguen intactos — sin dark mode, `splash_screen.dart` con sesión simulada, y los archivos/carpetas vacíos de scaffolding (`core/errors/`, `core/extensions/`, `core/helpers/`, `base_api_service.dart`, `unauthorized_screen.dart`) sin implementar.
- `README.md`, `Read_plan_dev.md` y `LICENSE` del proyecto generado siguen mencionando el nombre original de la plantilla (no se editan) — coméntaselo al usuario si le interesa actualizarlos a mano.

## Notas de implementación

- Las plantillas viven como árboles de archivos reales en `~/.claude/skills/base_flutter/templates/{provider,riverpod}/`, preprocesadas con placeholders (`__PackageName__`, `__BundleId__`, `__AppDisplayName__`, y la carpeta `__BundleSuffix__` bajo el paquete Kotlin) en vez del texto real de las plantillas originales — evita que un reemplazo pise a otro (ej. reemplazar `base_provider` por el nombre de paquete y luego no poder poner ahí el nombre visible).
- Si en el futuro hay que actualizar las plantillas (porque `base_flutter_provider`/`base_flutter_riverpod` cambiaron), hay que repetir el preprocesado de placeholders sobre la copia nueva — no sobrescribas `templates/` con una copia cruda sin placeholders.
- `ios/Flutter/Generated.xcconfig` y `ios/Flutter/flutter_export_environment.sh` no están en la plantilla (se eliminaron a propósito porque contenían rutas absolutas de la máquina donde se creó la plantilla original) — `flutter pub get`/`flutter build` los regenera automáticamente en el proyecto nuevo.
- El script usa `perl -pi -e` para todas las sustituciones (portable entre BSD/macOS y GNU sin diferencias de flags, a diferencia de `sed -i`).
