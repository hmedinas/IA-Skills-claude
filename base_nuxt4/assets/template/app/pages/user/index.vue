<template>
  <div class="bg-white dark:bg-dark-panel rounded-2xl shadow p-6 space-y-4">
    <div class="flex items-center justify-between">
      <h2 class="text-lg font-bold text-slate-700 dark:text-white uppercase tracking-tight">Usuarios</h2>
      <Button label="Nuevo" icon="pi pi-plus" size="small" />
    </div>

    <DataTable :value="usuarios" size="small" stripedRows>
      <Column field="displayName" header="Nombre"></Column>
      <Column field="email" header="Correo"></Column>
      <Column field="roles" header="Roles">
        <template #body="{ data }">{{ data.roles.join(', ') }}</template>
      </Column>
    </DataTable>
  </div>
</template>

<script setup lang="ts">
definePageMeta({
    layout: 'default',
});

// Ejemplo de pantalla CRUD-listado; sustituye por el service/endpoint real
// cuando exista. De momento reutiliza el mismo mock que el login.
const authStore = useAuthStore();
const usuarios = ref([
    { displayName: authStore.user?.displayName ?? 'Usuario Demo', email: authStore.user?.email ?? 'demo@example.com', roles: authStore.user?.roles ?? ['admin'] },
]);
</script>
