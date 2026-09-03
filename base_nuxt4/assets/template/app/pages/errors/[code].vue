<template>
  <div class="min-h-screen w-full flex flex-col items-center justify-center bg-fondo-100 dark:bg-dark-fondo gap-4">
    <span class="text-7xl font-black text-application-blue-600">{{ code }}</span>
    <p class="text-slate-500 dark:text-dark-textSecondary font-bold uppercase tracking-widest text-sm">{{ message }}</p>
    <NuxtLink to="/home" class="mt-4 px-6 py-3 bg-application-blue-600 text-white rounded-xl font-bold uppercase tracking-wider text-xs">
      Volver al inicio
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
    layout: 'blank',
});

const route = useRoute();
const code = computed(() => String(route.params.code ?? '500'));

const messages: Record<string, string> = {
    '401': 'Sesión no válida',
    '403': 'No tienes acceso a este recurso',
    '404': 'Página no encontrada',
    '500': 'Error del servidor',
};

const message = computed(() => messages[code.value] ?? 'Ocurrió un error inesperado');
</script>
