<template>
    <section :class="[fondoClass, 'flex h-screen w-full', 'dark:bg-dark-fondo dark:text-dark-text']">
        <MenuNavigation
            v-if="!isMenuHorizontal"
            :isCollapsed="isFloatMenu ? false : isResponsive ? true : isMenuCollapsed"
            :isHorizontal="isMenuHorizontal"
            :menuClass="menuClass"
            :isFloatMenu="isFloatMenu"
            :isOpen="isMenuCollapsed"
            :closePanel="closeMenu"
        />

        <div class="flex flex-col flex-1 transition-all duration-300 w-full h-screen overflow-hidden">
            <NavBar :toggleMenu="toggleMenu" :topbarClass="topbarClass" :isHorizontal="isMenuHorizontal" />

            <!-- Menú horizontal: se pinta debajo del topbar en vez de a la izquierda -->
            <MenuNavigation
                v-if="isMenuHorizontal"
                :isCollapsed="false"
                :isHorizontal="isMenuHorizontal"
                :menuClass="menuClass"
                :isFloatMenu="false"
                :isOpen="false"
                :closePanel="closeMenu"
            />

            <div class="flex-1 p-2 md:p-3 lg:p-4 overflow-y-auto">
                <NuxtPage />
            </div>

            <FloatingButton v-if="toolsView" :togglePanel="toggleSettingsPanel" />
            <SettingsPanel
                :isOpen="isSettingsPanelOpen"
                :closePanel="closeSettingsPanel"
                @updateTopbarClass="updateTopbarClass"
                @updateMenuType="updateMenuType"
                @updateMenuClass="updateMenuClass"
                @updateFondoClass="updateFondoClass"
            />
        </div>
    </section>

    <Teleport to="body">
        <Toast position="top-center" group="tc" :autoZIndex="false" :baseZIndex="99999" class="custom-toast-over-all" />
        <ConfirmDialog
            :autoZIndex="false"
            :baseZIndex="999999"
            class="custom-confirm-over-all"
            :pt="{ root: { style: 'z-index: 999999' }, mask: { style: 'z-index: 999998' } }"
        />
    </Teleport>
</template>

<script setup lang="ts">
import FloatingButton from '~/components/common/floating-button.vue';
import SettingsPanel from '~/components/common/settings-panel.vue';
import NavBar from '~/components/common/navbar.vue';
import MenuNavigation from '~/components/common/menu-navigation.vue';
import type { Device } from '~/types/device.interface';
import type { SettingApp } from '~/types/settingApp.interface';
import { usePrimeVue } from 'primevue/config';

const nuxtApp = useNuxtApp();
const device = nuxtApp.$device as Device;
const colorStore = useColorStore();

// SPA (ssr:false): estos imports dinámicos son seguros porque el layout
// solo se monta en cliente. Si algún día activas SSR, esto debe pasar a
// useRuntimeConfig()/fetch server-safe.
const mock = await import('@/mock/auth.mock.json');
const config = await import('@/config/config.json');

const primevue = usePrimeVue();
primevue.config.locale = {
    firstDayOfWeek: 1,
    dayNames: ['domingo', 'lunes', 'martes', 'miércoles', 'jueves', 'viernes', 'sábado'],
    dayNamesShort: ['dom', 'lun', 'mar', 'mié', 'jue', 'vie', 'sáb'],
    dayNamesMin: ['D', 'L', 'M', 'X', 'J', 'V', 'S'],
    monthNames: ['Enero', 'Febrero', 'Marzo', 'Abril', 'Mayo', 'Junio', 'Julio', 'Agosto', 'Septiembre', 'Octubre', 'Noviembre', 'Diciembre'],
    monthNamesShort: ['Ene', 'Feb', 'Mar', 'Abr', 'May', 'Jun', 'Jul', 'Ago', 'Sep', 'Oct', 'Nov', 'Dic'],
    today: 'Hoy',
    clear: 'Limpiar',
};

const toolsView = ref<boolean>(true);

// Estado de layout: todo controlado por settingApp (colorStore), que a su
// vez puede venir del mock, de localStorage o de los defaults de fábrica.
const isMenuCollapsed = ref(false);
const isSettingsPanelOpen = ref(false);
const menuClass = ref('bg-application-blue-600');
const topbarClass = ref('bg-application-silver-100');
const isMenuHorizontal = ref(false);
const fondoClass = ref('bg-fondo-100');

const screenWidth = ref(typeof window !== 'undefined' ? window.innerWidth : 1280);
const isResponsive = ref(screenWidth.value <= 768);
const isFloatMenu = ref(false);

const updateScreenSize = () => {
    if (device.isMobile === true) {
        isFloatMenu.value = true;
    } else {
        screenWidth.value = window.innerWidth;
        if (device.isTablet === true || device.isIpad === true) {
            isResponsive.value = screenWidth.value <= 1024;
        }
    }
};

onMounted(async () => {
    toolsView.value = config.configuration.toolsView;

    let settingAppTemp: SettingApp;
    if (config.configuration.useMock) {
        settingAppTemp = mock.settingApp.configScreem as SettingApp;
    } else {
        colorStore.setDefaultColor();
        settingAppTemp = colorStore.settingApp;
    }

    isMenuCollapsed.value = settingAppTemp.isMenuCollapsed;
    isMenuHorizontal.value = settingAppTemp.isMenuHorizontal;
    menuClass.value = settingAppTemp.menuClass;
    topbarClass.value = settingAppTemp.topbarClass;
    fondoClass.value = settingAppTemp.fondoClass;

    window.addEventListener('resize', updateScreenSize);
    updateScreenSize();

    if (colorStore.settingApp.isDark === false) {
        const prefersDark = window.matchMedia('(prefers-color-scheme: dark)').matches;
        colorStore.setDarkMode(prefersDark);
    }
});

onUnmounted(() => {
    window.removeEventListener('resize', updateScreenSize);
});

// Único lugar donde se toca document.documentElement: PrimeVue (vía
// darkModeSelector:'.dark') y Tailwind (darkMode:'class') reaccionan solos.
watch(
    () => colorStore.settingApp.isDark,
    (isDark) => {
        if (isDark) document.documentElement.classList.add('dark');
        else document.documentElement.classList.remove('dark');
    },
    { immediate: true },
);

const toggleMenu = () => {
    isMenuCollapsed.value = !isMenuCollapsed.value;
};
const closeMenu = () => {
    isMenuCollapsed.value = !isMenuCollapsed.value;
};
const toggleSettingsPanel = () => {
    isSettingsPanelOpen.value = !isSettingsPanelOpen.value;
};
const closeSettingsPanel = () => {
    isSettingsPanelOpen.value = false;
};
const updateMenuClass = (colorClass: string) => {
    menuClass.value = colorClass;
};
const updateTopbarClass = (colorClass: string) => {
    topbarClass.value = colorClass;
};
const updateMenuType = (type: 'horizontal' | 'vertical') => {
    isMenuHorizontal.value = type === 'horizontal';
};
const updateFondoClass = (colorClass: string) => {
    fondoClass.value = colorClass;
};
</script>
