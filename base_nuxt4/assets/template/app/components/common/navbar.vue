<template>
    <header :class="[topbarClass, 'h-16 flex items-center justify-between px-4 shadow-md w-auto', 'dark:bg-dark-panel dark:text-dark-text']">
        <!-- Izquierda: toggle del sidebar (o logo si el menú es horizontal) -->
        <div class="flex items-center flex-none">
            <button
                type="button"
                v-if="!isHorizontal"
                class="p-2 rounded-md hover:bg-black/5 hover:text-application-blue-500"
                @click="toggleMenu"
            >
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" class="inline-block h-5 w-5 stroke-current">
                    <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16"></path>
                </svg>
            </button>
            <span v-else class="text-lg font-black uppercase tracking-tighter">{{ appName }}</span>
        </div>

        <!-- Centro: título de módulo, alimentado por useNavigationStore -->
        <div class="flex-1 flex flex-col items-center justify-center mx-4">
            <h1 class="text-xl font-bold text-slate-700 dark:text-white uppercase">{{ navStore.moduleName }}</h1>
            <span v-if="navStore.areaName" class="text-[11px] font-extrabold uppercase tracking-[0.2em] text-application-blue-600">
                {{ navStore.areaName }}
            </span>
        </div>

        <!-- Derecha: usuario -->
        <div class="flex-none flex items-center gap-3">
            <div class="dropdown dropdown-end">
                <div tabindex="0" role="button" class="btn btn-ghost btn-circle avatar">
                    <div class="w-10 rounded-full">
                        <img alt="Avatar" :src="imageUser" />
                    </div>
                </div>
                <ul
                    tabindex="0"
                    :class="['bg-application-blue-600', 'menu menu-sm dropdown-content text-white rounded-box z-50 mt-3 w-64 p-2 shadow-lg dark:bg-dark-panel dark:text-dark-text']"
                >
                    <li>
                        <a><font-awesome icon="user" class="w-4 h-4" />{{ displayName }}</a>
                    </li>
                    <li>
                        <a><font-awesome icon="tag" class="w-4 h-4" />© v.{{ appVersion }}</a>
                    </li>
                    <li @click="logout">
                        <a class="text-application-yellow-500 font-bold">
                            <font-awesome icon="arrow-right-from-bracket" class="w-5 h-5 text-red-500" />
                            Cerrar Sesión
                        </a>
                    </li>
                </ul>
            </div>
        </div>
    </header>
</template>

<script setup lang="ts">
import { useNavigationStore } from '~/stores/useNavigationStore';
import type { AccessUser } from '~/types/access.interface';

interface Props {
    isHorizontal: boolean;
    topbarClass: string;
    toggleMenu: () => void;
}

withDefaults(defineProps<Props>(), {
    topbarClass: 'bg-application-silver-100',
});

const authStore = useAuthStore();
const navStore = useNavigationStore();
const displayName = ref<string>('');
const imageUser = ref<string>('');

const config = await import('@/config/config.json');
const appName = config.configuration.nameApplication as string;
const appVersion = config.configuration.version as string;

onMounted(() => {
    const usuario = authStore.user as AccessUser;
    displayName.value = usuario?.displayName ?? '';
    imageUser.value = authStore.imageUser;
});

const logout = async () => {
    await navigateTo('/auth/logout');
    authStore.logout();
};
</script>
