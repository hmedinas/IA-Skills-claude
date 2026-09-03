# __APP_NAME__

Proyecto generado con la skill `base_nuxt4`: Nuxt 4 (SPA) + PrimeVue + Tailwind +
Pinia, con login, menú lateral por roles, topbar, panel de personalización de
colores y modo oscuro/claro ya cableados, todo corriendo sobre datos mock.

## Arrancar

```bash
npm install
npm run dev      # NUXT_ENV_STAGE=development
```

Login: cualquier usuario/contraseña no vacíos (useMockUser=true en `app/config/config.json`).

## Stages disponibles

`npm run dev|stg|pre|prod` seleccionan el bloque `development|staging|preproduction|production`
de `app/config/config.json` vía la variable de entorno `NUXT_ENV_STAGE`.

## Conectar a un backend real

1. En `app/config/config.json`, pon `useMock` y `useMockUser` en `false`.
2. Ajusta `urlService` en el stage que vayas a usar.
3. Implementa las respuestas reales en tus `services/*.ts` — la forma
   `ResponseApi<T>` / `Transaction` ya está armada, y los endpoints van
   centralizados en `types/endpoint.enum.ts`.

## Personalizar el theme

Todos los colores del menú/topbar/fondo se definen como clases Tailwind en
`tailwind.config.js` (paletas `application`, `alternative`, `fondo`, `dark`).
`app/stores/useColorStore.ts` guarda qué clase está activa, y
`app/components/common/settings-panel.vue` es el panel donde el usuario final
elige entre esas clases — para dar más opciones de color, amplía la paleta
en `tailwind.config.js` y añade la clase al array correspondiente en
`settings-panel.vue`.

## Menú y roles

`app/config/menu.json` define el árbol de navegación; cada ítem/hijo puede
llevar `role: string[]`. `menu-navigation.vue` filtra ese árbol contra
`authStore.user.roles` en el montaje.
