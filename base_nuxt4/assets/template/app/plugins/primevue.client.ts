import { defineNuxtPlugin } from '#app';
import ToastService from 'primevue/toastservice';
import ConfirmationService from 'primevue/confirmationservice';

// El módulo @primevue/nuxt-module (ver nuxt.config.ts) ya hace
// app.use(PrimeVue, {...}) con el theme/preset Y auto-registra como
// globales todos los componentes listados en primevue.components.include
// (Button, Toast, ConfirmDialog, ...) — por eso NO hay que volver a hacer
// nuxtApp.vueApp.component('Button', Button) aquí: eso duplicaba el
// registro y Vue tiraba warning "already been registered".
// Lo único que el módulo NO cubre son los *servicios* (composables que se
// activan con app.use), así que este plugin se limita a ellos.
export default defineNuxtPlugin((nuxtApp) => {
    if (!nuxtApp.vueApp._primeVueServicesApplied) {
        nuxtApp.vueApp.use(ToastService);
        nuxtApp.vueApp.use(ConfirmationService);
        nuxtApp.vueApp._primeVueServicesApplied = true;
    }
    // El locale en español se configura en layouts/default.vue vía
    // usePrimeVue(), porque ese composable necesita contexto de componente
    // (inject) y aquí, dentro del plugin, todavía no lo hay.
});
