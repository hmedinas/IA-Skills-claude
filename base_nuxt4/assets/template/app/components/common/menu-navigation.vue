<template>
    <div>
        <!-- Backdrop del drawer móvil -->
        <div
            v-if="isFloatMenu && isOpen"
            class="fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm transition-opacity duration-300 z-20"
            @click="closePanel"
        ></div>

        <!-- Sidebar vertical: escritorio fijo + drawer móvil comparten el mismo markup -->
        <aside v-if="!isHorizontal">
            <div
                :class="[
                    menuClass,
                    'text-white transition-all duration-300 ease-in-out',
                    isFloatMenu
                        ? ['top-0 left-0 fixed h-screen shadow-lg z-30 w-64', isOpen ? 'translate-x-0' : '-translate-x-full']
                        : ['relative h-screen flex-shrink-0', menuCollapsed ? 'w-20' : 'w-64', 'dark:bg-dark-panel dark:text-dark-text'],
                ]"
            >
                <div class="flex flex-col h-full">
                    <!-- Header: logo + usuario -->
                    <div :class="['p-4 space-y-4 shrink-0', menuCollapsed ? 'flex flex-col items-center' : '']">
                        <span v-if="menuCollapsed" class="text-2xl font-black tracking-tighter">{{ appInitial }}</span>
                        <span v-else class="text-xl font-black tracking-tighter uppercase">{{ appName }}</span>

                        <div class="border-b border-white/10 w-full"></div>

                        <div v-if="!menuCollapsed" class="flex items-center gap-3">
                            <img :src="imageUser" class="w-12 h-12 rounded-full border-2 border-white/20 object-cover" alt="Avatar" />
                            <div class="overflow-hidden">
                                <p class="font-bold text-sm truncate">{{ displayName }}</p>
                                <p class="text-[10px] text-white/50 truncate uppercase">{{ roleLabel }}</p>
                            </div>
                        </div>
                        <div v-if="!menuCollapsed" class="border-b border-white/10 w-full"></div>
                    </div>

                    <!-- Ítems de menú (poblados desde config/menu.json filtrados por rol) -->
                    <ul class="flex-1 px-2 space-y-1 relative overflow-y-auto">
                        <li v-for="item in menuItems" :key="item.id" class="relative group">
                            <!-- Ítem simple -->
                            <div
                                v-if="!item.children"
                                @click="selectItem(item.id)"
                                :class="[
                                    'relative cursor-pointer flex items-center rounded-xl transition-all duration-300 hover:bg-white/10',
                                    menuCollapsed ? 'flex-col justify-center py-3' : 'flex-row p-3',
                                    selectedItem === item.id ? 'bg-application-blue-300 border-r-4 border-application-yellow-500' : '',
                                ]"
                            >
                                <font-awesome v-if="item.icon" :icon="item.icon" :class="menuCollapsed ? 'text-xl' : 'text-lg'" />
                                <span :class="[menuCollapsed ? 'text-[10px] mt-1 text-center leading-tight' : 'ml-4 text-sm flex-1']">{{ item.label }}</span>
                            </div>

                            <!-- Ítem con hijos -->
                            <div v-else>
                                <details v-if="!menuCollapsed" class="group/details" :open="selectedItem === item.id">
                                    <summary class="list-none cursor-pointer flex items-center p-3 rounded-xl transition-all duration-300 hover:bg-white/10">
                                        <font-awesome v-if="item.icon" :icon="item.icon" class="text-lg" />
                                        <span class="ml-4 text-sm flex-1">{{ item.label }}</span>
                                        <font-awesome icon="chevron-down" class="text-[10px] opacity-40 transition-transform group-open/details:rotate-180" />
                                    </summary>
                                    <ul class="mt-1 ml-4 border-l border-white/10 space-y-1">
                                        <li v-for="sub in item.children" :key="sub.id">
                                            <div
                                                @click="selectItem(sub.id)"
                                                :class="[
                                                    'pl-6 pr-4 py-2 text-xs cursor-pointer rounded-lg hover:bg-white/5 flex items-center gap-3 transition-colors',
                                                    selectedItem === sub.id ? 'text-application-yellow-500 bg-white/5 font-bold' : 'text-white/80',
                                                ]"
                                            >
                                                <font-awesome v-if="sub.icon" :icon="sub.icon" class="text-[14px] opacity-80" />
                                                <span class="text-[12px]">{{ sub.label }}</span>
                                            </div>
                                        </li>
                                    </ul>
                                </details>

                                <!-- Vista colapsada: submenú flotante al hover -->
                                <div v-else class="relative">
                                    <div class="relative flex flex-col items-center justify-center py-3 rounded-xl transition-all duration-300 group-hover:bg-white/10 cursor-pointer">
                                        <font-awesome :icon="item.icon" class="text-xl" />
                                        <span class="text-[10px] mt-1 text-center leading-tight">{{ item.label }}</span>
                                    </div>

                                    <div class="invisible opacity-0 group-hover:visible group-hover:opacity-100 absolute left-full top-0 ml-2 w-56 bg-application-blue-500 shadow-2xl rounded-xl border border-white/20 py-2 transition-all duration-200 z-50">
                                        <div class="px-4 py-2 mb-2 border-b border-white/10">
                                            <span class="font-bold text-xs text-application-yellow-500 uppercase tracking-wider">{{ item.label }}</span>
                                        </div>
                                        <ul>
                                            <li v-for="sub in item.children" :key="sub.id">
                                                <div
                                                    @click="selectItem(sub.id)"
                                                    class="px-4 py-2.5 text-xs hover:bg-white/10 cursor-pointer flex items-center justify-between text-white/90 hover:text-white transition-colors"
                                                >
                                                    {{ sub.label }}
                                                    <font-awesome icon="chevron-right" class="text-[8px] opacity-30" />
                                                </div>
                                            </li>
                                        </ul>
                                    </div>
                                </div>
                            </div>
                        </li>
                    </ul>

                    <!-- Footer: dark mode + logout -->
                    <div class="p-2 bg-black/10 shrink-0">
                        <div class="flex justify-around items-center">
                            <button type="button" @click="toggleDarkMode" class="p-2 hover:text-yellow-400 transition-colors">
                                <font-awesome :icon="isDarkMode ? 'sun' : 'moon'" />
                            </button>
                            <button type="button" @click="logout" class="p-2 hover:text-red-400 transition-colors">
                                <font-awesome icon="power-off" />
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </aside>

        <!-- Navegación horizontal (alternativa al sidebar, ver settings-panel.vue) -->
        <aside v-else>
            <nav :class="[menuClass, 'text-white transition-all duration-300 flex items-center px-6 py-2 h-14 z-30']">
                <ul class="flex items-center space-x-4">
                    <li v-for="item in menuItems" :key="item.id" class="relative group">
                        <div
                            :class="[
                                'cursor-pointer flex items-center gap-2 px-3 py-1.5 rounded-lg transition-all',
                                selectedItem === item.id ? 'bg-application-blue-300 border-b-2 border-application-yellow-500' : 'hover:bg-white/10',
                            ]"
                            @click="item.children ? null : selectItem(item.id)"
                        >
                            <font-awesome :icon="item.icon" class="text-sm" />
                            <span class="text-sm font-bold">{{ item.label }}</span>
                            <font-awesome v-if="item.children" icon="chevron-down" class="text-[9px] opacity-50" />
                        </div>

                        <ul v-if="item.children" class="absolute left-0 mt-1 w-56 bg-application-blue-600 shadow-2xl rounded-xl hidden group-hover:block z-50 border border-white/10 py-2">
                            <li v-for="sub in item.children" :key="sub.id">
                                <div @click="selectItem(sub.id)" class="px-4 py-2.5 text-xs hover:bg-white/10 cursor-pointer">
                                    {{ sub.label }}
                                </div>
                            </li>
                        </ul>
                    </li>
                </ul>
            </nav>
        </aside>
    </div>
</template>

<script setup lang="ts">
import type { MenuApplication } from '~/types/menuApplication.interface';
import type { AccessUser } from '~/types/access.interface';

interface Props {
    isCollapsed: boolean;
    isHorizontal: boolean;
    menuClass: string;
    isOpen: boolean;
    isFloatMenu: boolean;
    closePanel: () => void;
}

const props = withDefaults(defineProps<Props>(), {
    menuClass: 'bg-application-blue-600',
});

/** Móvil (drawer): siempre expandido. Escritorio: respeta isCollapsed. */
const menuCollapsed = computed(() => (props.isFloatMenu ? false : Boolean(props.isCollapsed)));

const authStore = useAuthStore();
const router = useRouter();
const colorStore = useColorStore();

const displayName = ref('');
const roleLabel = ref('');
const imageUser = ref<string>('');
const selectedItem = ref(1);

const menuItems = ref<MenuApplication[]>([]);
const isDarkMode = computed(() => colorStore.settingApp.isDark);

const config = await import('@/config/config.json');
const appName = config.configuration.nameApplication as string;
const appInitial = appName.charAt(0).toUpperCase();

onMounted(async () => {
    const menujson = await import('@/config/menu.json');
    const usuario = authStore.user as AccessUser;

    displayName.value = usuario?.displayName ?? '';
    roleLabel.value = usuario?.roles?.[0] ?? '';
    imageUser.value = authStore.imageUser;

    const rolesUsuario = (usuario?.roles ?? []).map((r) => (r || '').trim().toLowerCase());

    // Filtra el árbol de menu.json por los roles del usuario: si un ítem no
    // define `role`, es visible para todos. Un padre sin hijos visibles tras
    // el filtro tampoco se muestra.
    const tieneAcceso = (rolesMenu?: string[] | null) => {
        if (!rolesMenu?.length) return true;
        const rolesNorm = rolesMenu.map((r) => (r || '').trim().toLowerCase());
        return rolesUsuario.some((r) => rolesNorm.includes(r));
    };

    const tempMenu = menujson.default.menu as any[];
    const items: MenuApplication[] = [];

    tempMenu.forEach((m) => {
        if (!tieneAcceso(m.role)) return;

        let children: MenuApplication[] | null = null;
        if (m.children?.length) {
            children = m.children
                .filter((sub: any) => tieneAcceso(sub.role))
                .map((sub: any) => ({ id: sub.id, label: sub.label, icon: sub.icon, to: sub.to }));
            if (!children.length) return;
        }

        items.push({ id: m.id, label: m.label, icon: m.icon, to: m.to, children });
    });

    menuItems.value = items;
});

const selectItem = (id: number) => {
    selectedItem.value = id;
    const item = findMenuItem(menuItems.value, id);
    if (item?.to) {
        if (props.isFloatMenu) props.closePanel();
        router.push(item.to);
    }
};

const findMenuItem = (items: MenuApplication[], id: number): MenuApplication | undefined => {
    for (const item of items) {
        if (item.id === id) return item;
        if (item.children) {
            const found = item.children.find((c) => c.id === id);
            if (found) return found;
        }
    }
    return undefined;
};

const toggleDarkMode = () => {
    colorStore.setDarkMode(!colorStore.settingApp.isDark);
};

const logout = async () => {
    await navigateTo('/auth/logout');
    authStore.logout();
};
</script>
