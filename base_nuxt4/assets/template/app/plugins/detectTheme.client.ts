import { defineNuxtPlugin } from '#app';

// Detecta la preferencia de dark mode del SO al arrancar y se suscribe a
// cambios en vivo (el usuario cambia de claro a oscuro en su OS sin recargar
// la pestaña). El layout default.vue es quien realmente aplica/quita la
// clase `.dark` en <html> a partir de colorStore.settingApp.isDark.
export default defineNuxtPlugin(() => {
    const colorStore = useColorStore();

    const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
    colorStore.setDarkMode(prefersDark);

    window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', (e) => {
        colorStore.setDarkMode(e.matches);
    });
});
