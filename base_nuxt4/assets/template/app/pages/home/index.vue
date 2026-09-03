<template>
  <div class="space-y-4">
    <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
      <div v-for="card in cards" :key="card.label" class="bg-white dark:bg-dark-panel rounded-2xl shadow p-5 flex items-center gap-4">
        <div :class="[card.class, 'w-12 h-12 rounded-xl flex items-center justify-center text-white text-xl']">
          <font-awesome :icon="card.icon" />
        </div>
        <div>
          <p class="text-2xl font-black text-slate-700 dark:text-white">{{ card.value }}</p>
          <p class="text-[11px] font-bold uppercase tracking-widest text-slate-400">{{ card.label }}</p>
        </div>
      </div>
    </div>

    <div class="bg-white dark:bg-dark-panel rounded-2xl shadow p-6">
      <h2 class="text-lg font-bold text-slate-700 dark:text-white uppercase tracking-tight mb-2">
        Bienvenido, {{ displayName }}
      </h2>
      <p class="text-sm text-slate-500 dark:text-dark-textSecondary">
        Esta es la pantalla de inicio de la base <strong>base_nuxt4</strong>: login mockeado, menú lateral
        colapsable con roles, topbar, panel de personalización de colores y modo oscuro/claro ya cableados.
        Sustituye este contenido por el dashboard real de tu app.
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { AccessUser } from '~/types/access.interface';

definePageMeta({
    layout: 'default',
});

const authStore = useAuthStore();
const displayName = ref('');

const cards = ref([
    { label: 'Usuarios', value: '1', icon: 'user', class: 'bg-application-blue-500' },
    { label: 'Roles', value: '1', icon: 'shield-halved', class: 'bg-alternative-green-200' },
    { label: 'Módulos', value: '2', icon: 'house', class: 'bg-alternative-purple-200' },
    { label: 'Config.', value: 'Mock', icon: 'cog', class: 'bg-alternative-orange-100' },
]);

onMounted(() => {
    const usuario = authStore.user as AccessUser;
    displayName.value = usuario?.displayName ?? '';
});
</script>
