/** @type {import('tailwindcss').Config} */
export default {
  content: [
    './app/**/*.{vue,js,ts,jsx,tsx}', // <-- VITAL en Nuxt 4 (código vive en /app)
    './components/**/*.{vue,js,ts,jsx,tsx}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './plugins/**/*.{js,ts}',
    './nuxt.config.{js,ts}',
    './app.vue',
    './error.vue',
    './node_modules/primevue/**/*.{vue,js,ts,jsx,tsx}',
  ],
  theme: {
    extend: {
      colors: {
        // ============================================================
        // PALETA DE DISEÑO BASE — punto único de verdad del theming.
        // Todo color usado por menú/topbar/paneles se referencia SIEMPRE
        // como clase Tailwind (bg-application-blue-600, text-primary...),
        // nunca como hex suelto en un componente. Así, cambiar el theme
        // de una app derivada de esta base es editar SOLO este archivo.
        // ============================================================

        // Color de acento "de marca" por defecto para botones/links propios
        // de la app (independiente del theme de PrimeVue, ver base.css).
        primary: '#3F51B5',

        // Paleta de selección libre que alimenta el panel de ajustes
        // (settings-panel.vue) para que el usuario elija color primario.
        alternative: {
          red: { 100: '#f43f5e', 200: '#E91E63' },
          purple: { 100: '#9C27B0', 200: '#673AB7', 300: '#3F51B5' },
          blue: { 100: '#2196F3', 200: '#03A9F4', 300: '#00BCD4' },
          green: { 100: '#10b981', 200: '#22c55e', 300: '#84cc16' },
          yellow: '#eab308',
          orange: { 100: '#f97316', 200: '#f97316' },
        },

        // Paleta "de aplicación": son los tonos que se usan de verdad en
        // menú lateral, topbar y estados. menuClass/topbarClass del
        // colorStore apuntan a nombres de esta paleta (ej: bg-application-blue-600).
        application: {
          blue: { 100: '#337ab7', 300: '#44658f', 500: '#154481', 600: '#133d74' },
          yellow: { 100: '#fde8d1', 500: '#e0b800' },
          silver: { 800: '#676a6c', 300: '#dfe4ed', 100: '#e7eaec' },
          green: '#1ab394',
        },

        // Paleta específica para el modo oscuro manual (clases dark:bg-dark-*),
        // complementaria a las variables --p-* de PrimeVue en base.css.
        dark: {
          panel: '#34343d',
          fondo: '#1d1e27',
          text: '#ffffff',
          textSecondary: '#a5a5a9',
        },

        // Fondos de página seleccionables desde el panel de ajustes.
        fondo: {
          100: '#dfe4ed',
          200: '#babec6',
          300: '#bac2c6',
          400: '#a7a0a66e',
          500: '#b1b4b078',
          600: '#808080a8',
          700: '#828f8fa8',
        },
      },
    },
  },
  plugins: [require('tailwindcss-primeui'), require('daisyui')],
  darkMode: 'class',
  daisyui: {
    themes: ['light', 'dark'],
    darkTheme: 'dark',
    base: true,
    utils: true,
    logs: false,
  },
}
