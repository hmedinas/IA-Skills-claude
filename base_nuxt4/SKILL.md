---
name: base_nuxt4
description: Genera un proyecto Nuxt 4 base (SPA) con login, menú lateral colapsable por roles, topbar, panel de personalización de colores/tema y modo oscuro/claro, todo sobre PrimeVue + Tailwind + Pinia y datos mock (sin backend real). Usa esta skill SIEMPRE que el usuario pida crear un nuevo proyecto Nuxt, un "starter"/"boilerplate"/"plantilla base" de Nuxt, o replicar el patrón de arquitectura (config por stages, servicios con mock/real, theming vía Tailwind, dark mode PrimeVue+Tailwind sincronizado) de una app Nuxt existente, aunque no mencione "base_nuxt4" por su nombre.
---

# base_nuxt4

Genera proyectos Nuxt 4 nuevos que replican un patrón de arquitectura probado
(SPA con PrimeVue + Tailwind + Pinia, mock-first, theming configurable,
login + menú + topbar ya cableados) para usar como punto de partida de
cualquier app interna nueva.

## Cuándo usar esta skill

- El usuario pide crear un proyecto Nuxt nuevo, un starter/boilerplate/plantilla.
- Pide "la base de siempre", replicar el patrón de otra app Nuxt del usuario,
  o menciona explícitamente login + menú lateral + dark mode + theming.
- Quiere extender un proyecto YA generado por esta skill (añadir una pantalla,
  un service, un color al panel de ajustes) — en ese caso no vuelvas a copiar
  la plantilla entera, sigue los patrones descritos en "Arquitectura" abajo
  y edita solo lo necesario.

## Qué genera

Un proyecto Nuxt 4 completo y funcional (`npm run dev` levanta login → home
sin backend) con:

- **Login** mockeado (`app/pages/auth/login.vue`) + logout + guard global de rutas.
- **Layout principal** con menú lateral colapsable/responsive filtrado por
  rol (`app/config/menu.json`), topbar con datos del usuario, panel lateral
  de personalización de colores (fondo/menú/topbar/color primario) y botón
  flotante para abrirlo.
- **Dark/light mode** sincronizado entre Tailwind, PrimeVue y daisyUI con un
  solo toggle.
- **Config por stages** (`development`/`staging`/`preproduction`/`production`)
  vía `NUXT_ENV_STAGE`, con `urlService` por entorno.
- **Capa de servicios** con patrón mock/real: cada método mira
  `config.configuration.useMockUser`/`useMock` antes de decidir si pega a la
  API real o devuelve datos mock — cambiar de mock a real es un flag, no una
  reescritura.
- Stores de Pinia (`auth`, `color`, `navigation`), tipos TS, endpoints
  centralizados en un enum, logger condicionado por config.

## Cómo generarlo

1. **Reúne 2-3 datos del usuario** (pregunta solo si no los dio ya en el
   pedido): nombre visible de la app (ej. "Portal RRHH") y, si aplica, dónde
   quiere el proyecto (ruta destino). Si no especifica, usa el nombre como
   slug en minúsculas con guiones para `package.json`.

2. **Copia la plantilla completa** de `assets/template/` al directorio
   destino (todo el árbol: `app/`, `nuxt.config.ts`, `tailwind.config.js`,
   `package.json`, `tsconfig.json`, `.gitignore`, `README.md`). No la
   regeneres archivo por archivo escribiendo código nuevo — la plantilla ya
   está probada (build de producción limpio, sin warnings) y reescribirla
   desde cero solo introduce riesgo de romper algo que ya funciona.

3. **Reemplaza los placeholders** en todos los archivos copiados:
   - `__APP_NAME__` → nombre visible (ej. "Portal RRHH") — aparece en
     `config.json`, `middleware/auth.global.ts` y `README.md`.
   - `__APP_SLUG__` → slug en `package.json` (`name`).

   ```bash
   grep -rl "__APP_NAME__" <dest> | xargs sed -i '' "s/__APP_NAME__/Nombre Real/g"
   grep -rl "__APP_SLUG__" <dest> | xargs sed -i '' "s/__APP_SLUG__/nombre-slug/g"
   ```
   (en Linux, `sed -i` sin el `''`).

4. **Instala dependencias.** `npm install` a veces falla con
   `Cannot read properties of null (reading 'edgesOut')` — es un bug conocido
   del resolutor de npm (arborist) con este árbol de dependencias, no un
   problema de la plantilla. Si pasa, reintenta con:
   ```bash
   npm install --legacy-peer-deps
   ```

5. **Verifica que levanta** antes de darlo por terminado: `npm run dev` y
   confirma que carga `/auth/login`, que el login con cualquier
   usuario/contraseña no vacíos entra a `/home` (mock activo por defecto), y
   que no hay errores en consola. Si tienes herramientas de navegador
   disponibles, úsalas para esta verificación en vez de asumir que "debería
   funcionar".

6. **Dile al usuario qué sigue**: cómo conectar el backend real (apagar
   `useMock`/`useMockUser` en `config.json` + implementar los servicios), y
   que el `README.md` generado ya documenta esto.

## Arquitectura del patrón (para extender un proyecto ya generado)

Si el usuario pide añadir algo a un proyecto que ya usa esta base, sigue
estas convenciones — son las que hacen que el patrón se mantenga consistente
entre proyectos derivados:

- **Nunca hardcodees un color hex en un componente.** Todo color de
  menú/topbar/fondo es una clase Tailwind definida en `tailwind.config.js`
  (paletas `application`, `alternative`, `fondo`, `dark`) y referenciada por
  nombre desde `useColorStore`. Para dar una opción de color nueva: añade el
  tono en `tailwind.config.js` y agrégalo al array correspondiente en
  `settings-panel.vue`.
- **Todo servicio nuevo sigue el patrón mock/real** de
  `app/services/auth.service.ts` / `user.service.ts`: el método público mira
  `config.configuration.useMock(User)` primero, y solo entonces decide entre
  un método `__privado` que devuelve datos mock (mismo shape que la API real,
  `ResponseApi<T>`/`Transaction`) o la llamada real vía `$axiosApp` con el
  endpoint centralizado en `app/types/endpoint.enum.ts`.
- **Ítems de menú nuevos van en `app/config/menu.json`**, con `role: string[]`
  si deben restringirse. `menu-navigation.vue` ya filtra el árbol contra
  `authStore.user.roles` — no dupliques esa lógica en otro lado.
- **`.dark` en `<html>` es el único interruptor de tema.** Lo aplica un único
  `watch` en `layouts/default.vue` sobre `colorStore.settingApp.isDark`.
  Cualquier componente nuevo que necesite estilos de modo oscuro usa el
  prefijo `dark:` de Tailwind o las variables `--p-*` de PrimeVue (ver
  `app/assets/css/base.css`) — no leas `prefers-color-scheme` de nuevo en
  otro sitio.
- **`useRuntimeConfig().public.appConfig`** ya trae el bloque de
  `config.json` correspondiente al stage activo (resuelto una vez en
  `nuxt.config.ts`). Los `await import('@/config/config.json')` dispersos en
  componentes/servicios son deliberados solo donde se necesita releer
  `configuration.useMock*` en caliente; para todo lo demás (urlService,
  pathGlobal, chatBox...) usa `useRuntimeConfig()`.
- **daisyUI está fijado a v4** (`^4.12.x`), NO subas a v5 mientras el
  proyecto siga en Tailwind v3 (`tailwindcss: ^3.4`) — daisyUI v5 requiere
  Tailwind v4 y rompe el build de producción (`nuxt build`) con un error de
  `lightningcss` al minificar CSS, aunque `nuxt dev` no lo muestre. Si algún
  día se migra a Tailwind v4, daisyUI puede subir junto con eso.

## Notas de la plantilla

- `ssr: false` (SPA pura) — todo el código asume cliente; si conviertes esto
  a SSR en algún proyecto derivado, los `await import('@/config/config.json')`
  y accesos a `window`/`localStorage` en stores/plugins necesitan revisión.
- El build de producción de esta plantilla se validó sin warnings ni errores
  antes de publicar la skill (los warnings "Plugin has already been applied"
  que puedan verse en `npm run dev` son ruido de Nuxt DevTools en modo
  desarrollo, no aparecen en producción).
