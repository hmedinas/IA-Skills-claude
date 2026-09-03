<template>
  <div />
</template>

<script setup lang="ts">
// Ruta "/" real y registrada (evita el warning VUE_ROUTER_R0004 de
// vue-router al no encontrar match antes de que corra el middleware).
// El redirect efectivo a /auth/login ya lo hace middleware/auth.global.ts.
definePageMeta({
    layout: 'blank',
});
</script>
