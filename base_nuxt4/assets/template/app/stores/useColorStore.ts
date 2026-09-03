import { useLocalStorage } from '@vueuse/core';
import { defineStore } from 'pinia';
import { ref } from 'vue';
import type { PrimaryColors } from '~/types/primaryColors.interface';
import type { SettingApp } from '~/types/settingApp.interface';

// Estado inicial "de fábrica" del theming. Estas MISMAS clases Tailwind
// (bg-primary, bg-application-blue-600, bg-fondo-100...) deben existir en
// tailwind.config.js — el store solo guarda el NOMBRE de la clase, nunca
// un color hardcodeado, para que todo el theming viva en un solo sitio.
const initialPrimaryColor = (): PrimaryColors => ({
    buttonClassPrimary: 'bg-primary text-white',
    textClassPrimary: 'text-primary',
});

const initialSettingApp = (): SettingApp => ({
    isMenuCollapsed: false,
    isDark: false,
    isMenuHorizontal: false,
    menuClass: 'bg-application-blue-600',
    topbarClass: 'bg-application-silver-100',
    fondoClass: 'bg-fondo-100',
    primaryColor: 'bg-primary',
});

export const useColorStore = defineStore('color', () => {
    // Persistido en localStorage: el usuario elige su color primario una vez
    // y sobrevive a recargas/futuras sesiones (ver settings-panel.vue).
    const colorApplication = ref(useLocalStorage<PrimaryColors>('color', initialPrimaryColor()));
    const settingApp = ref<SettingApp>(initialSettingApp());

    const setPrimaryColor = (colorClass: string) => {
        colorApplication.value.buttonClassPrimary = `${colorClass} text-white`;
        colorApplication.value.textClassPrimary = colorClass.replace('bg-', 'text-');
    };

    const setDefaultColor = () => {
        settingApp.value = initialSettingApp();
        setPrimaryColor(settingApp.value.primaryColor);
    };

    // Único punto de entrada para alternar modo oscuro: quien llame a esto
    // (toggle manual, preferencia del SO, layout al montar) siempre pasa por
    // aquí, y un watch en el layout aplica la clase `.dark` al <html>.
    const setDarkMode = (value: boolean) => {
        settingApp.value.isDark = value;
        localStorage.setItem('settingApp', JSON.stringify(settingApp.value));
    };

    return {
        settingApp,
        colorApplication,
        setPrimaryColor,
        setDefaultColor,
        setDarkMode,
    };
});
