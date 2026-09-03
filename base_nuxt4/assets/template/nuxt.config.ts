// https://nuxt.com/docs/api/configuration/nuxt-config

import { readFileSync } from 'node:fs'
import { resolve } from 'node:path'
import Aura from '@primeuix/themes/aura'

// La app es SPA pura (ssr:false), así que el config.json se lee en build-time
// aquí en Node y se inyecta como runtimeConfig.public.appConfig. Todo el resto
// del código de la app (stores, services, componentes) usa SIEMPRE
// `useRuntimeConfig().public.appConfig`, nunca importa config.json en server-side.
const configPath = resolve(process.cwd(), 'app/config/config.json')
const config = JSON.parse(readFileSync(configPath, 'utf-8'))

// El stage decide qué bloque de config.json (urlService, chatBox, etc.) se usa.
// Se selecciona con la variable de entorno NUXT_ENV_STAGE, seteada por los
// scripts de package.json (dev/stg/pre/prod -> development/staging/preproduction/production).
const stage = process.env.NUXT_ENV_STAGE || 'development'
console.log('[DEBUG] NUXT_ENV_STAGE:', stage)
const appConfig = config[stage]

export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: stage !== 'production' },

  ssr: false, // SPA mode

  modules: ['@pinia/nuxt', '@nuxtjs/tailwindcss', '@primevue/nuxt-module', '@vesp/nuxt-fontawesome'],

  future: {
    compatibilityVersion: 4,
  },

  // Set mínimo de iconos usados por la base (menú, topbar, login, dark mode).
  // Amplía esta lista según lo que tu app necesite.
  fontawesome: {
    icons: {
      solid: [
        'house', 'cog', 'user', 'moon', 'sun', 'power-off',
        'chevron-down', 'chevron-right', 'key', 'tag',
        'arrow-right-from-bracket', 'person-walking-arrow-right',
        'shield-halved', 'spinner', 'circle-exclamation', 'bars', 'xmark',
      ],
    },
  },

  primevue: {
    components: {
      include: [
        'Button', 'InputText', 'Password', 'Card',
        'DataTable', 'Column', 'Select',
        'Toast', 'ConfirmDialog', 'ProgressSpinner',
      ],
    },
    options: {
      ripple: true,
      theme: {
        preset: Aura,
        options: {
          // El selector de dark mode de PrimeVue está atado a la misma clase
          // `.dark` que usa Tailwind (darkMode:'class'), así que un solo toggle
          // (colorStore.setDarkMode) sincroniza PrimeVue + Tailwind + daisyUI.
          darkModeSelector: '.dark',
          cssLayer: false,
        },
      },
    },
  },

  css: [
    'primeicons/primeicons.css',
    '~/assets/css/main.css',
    '~/assets/css/base.css',
  ],

  postcss: {
    plugins: {
      tailwindcss: {},
      autoprefixer: {},
    },
  },

  vite: {
    clearScreen: false,
  },

  runtimeConfig: {
    public: {
      envStage: stage,
      appConfig,
    },
  },
})
