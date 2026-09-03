import { defineStore } from 'pinia';
import { ref } from 'vue';

// Alimenta el título/subtítulo del topbar (navbar.vue). El middleware
// auth.global.ts lo actualiza automáticamente en cada navegación según
// dónde encuentre la ruta actual dentro de menu.json.
export const useNavigationStore = defineStore('navigation', () => {
    const moduleName = ref<string>('Inicio');
    const areaName = ref<string>('');
    const tipoOperacion = ref<string>('');

    const setNavigation = (modulo: string, area: string, tipo: string = '') => {
        moduleName.value = modulo;
        areaName.value = area;
        tipoOperacion.value = tipo;
    };

    return { moduleName, areaName, tipoOperacion, setNavigation };
});
