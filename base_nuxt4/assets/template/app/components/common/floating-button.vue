<template>
    <!-- Botón flotante que abre el panel de personalización de colores/tema -->
    <button
        type="button"
        :class="[color.buttonClassPrimary, 'fixed right-0 top-[40%] transform -translate-y-1/2 hover:opacity-90 p-2 pt-3 rounded-l-lg shadow-lg z-10']"
        @click="togglePanel"
    >
        <font-awesome icon="cog" class="w-6 h-6" />
    </button>
</template>

<script setup lang="ts">
const props = defineProps<{
    togglePanel: () => void;
}>();

const color = useColorStore().colorApplication;
</script>
