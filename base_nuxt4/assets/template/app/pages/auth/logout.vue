<template>
  <div />
</template>

<script setup lang="ts">
definePageMeta({
    layout: 'blank',
});

const authStore = useAuthStore();

onMounted(() => {
    authStore.logout();
    navigateTo('/auth/login');
});
</script>
