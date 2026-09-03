<template>
    <div class="dark:text-black">
        <div v-if="isOpen" class="fixed inset-0 bg-black bg-opacity-50 backdrop-blur-sm transition-opacity duration-300 z-20" @click="closePanel"></div>

        <div :class="['fixed top-0 right-0 h-screen bg-white shadow-lg transition-transform duration-300 z-20', isOpen ? 'translate-x-0' : 'translate-x-full']" style="width: 300px">
            <div class="flex justify-between items-center p-4 border-b">
                <h2 class="text-xl font-bold">Configuración</h2>
                <button type="button" @click="closePanel" class="text-gray-600 hover:text-gray-800">
                    <font-awesome icon="xmark" class="w-5 h-5" />
                </button>
            </div>

            <div class="p-4 space-y-4">
                <!-- Color primario: botones/links propios de la app (bg-primary) -->
                <div>
                    <h3 class="text-lg font-semibold">Color Primario</h3>
                    <div class="grid grid-cols-7 gap-2 mt-4">
                        <div
                            v-for="(color, index) in primaryColors"
                            :key="'primary-' + index"
                            :class="[color, 'w-6 h-6 rounded-full cursor-pointer', color === settingApp.primaryColor ? 'border-4 border-alternative-yellow' : '']"
                            @click="setPrimaryColor(color)"
                        ></div>
                    </div>
                </div>

                <!-- Color de fondo de la página -->
                <div>
                    <h3 class="text-lg font-semibold">Color Fondo</h3>
                    <div class="grid grid-cols-7 gap-2 mt-4">
                        <div
                            v-for="(color, index) in fondoColor"
                            :key="'fondo-' + index"
                            :class="[color, 'w-6 h-6 rounded-full cursor-pointer', color === settingApp.fondoClass ? 'border-4 border-alternative-red-100' : '']"
                            @click="setFondoColor(color)"
                        ></div>
                    </div>
                </div>

                <!-- Color del menú lateral -->
                <div>
                    <h3 class="text-lg font-semibold">Menu Color</h3>
                    <div class="grid grid-cols-7 gap-2 mt-4">
                        <div
                            v-for="(color, index) in surfaceColors"
                            :key="'surface-' + index"
                            :class="[color, 'w-6 h-6 rounded-full cursor-pointer', color === settingApp.menuClass ? 'border-4 border-alternative-red-100' : '']"
                            @click="setMenuColor(color)"
                        ></div>
                    </div>
                </div>

                <!-- Modo oscuro/claro -->
                <div>
                    <h3 class="text-lg font-semibold">Modo Oscuro</h3>
                    <label class="swap swap-rotate">
                        <input type="checkbox" v-model="isDarkMode" />
                        <font-awesome icon="sun" class="swap-on h-8 w-8" />
                        <font-awesome icon="moon" class="swap-off h-8 w-8" />
                    </label>
                </div>

                <!-- Tipo de menú -->
                <div>
                    <h3 class="text-lg font-semibold">Tipo Menu</h3>
                    <label class="flex cursor-pointer gap-2 items-center">
                        <span class="label-text">Vertical</span>
                        <input type="checkbox" class="toggle" v-model="isHorizontal" @change="toggleIsHorizontal" />
                        <span class="label-text">Horizontal</span>
                    </label>
                </div>

                <!-- Color del topbar -->
                <div>
                    <h3 class="text-lg font-semibold">Topbar Theme</h3>
                    <div class="grid grid-cols-7 gap-2 mt-4">
                        <div
                            v-for="(colorClass, index) in topbarColorClasses"
                            :key="'topbar-' + index"
                            :class="[colorClass, 'w-6 h-6 rounded-full cursor-pointer border', colorClass === settingApp.topbarClass ? 'border-4 border-alternative-red-100' : '']"
                            @click="setTopbarColor(colorClass)"
                        ></div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script setup lang="ts">
interface Props {
    isOpen: boolean;
    closePanel: () => void;
}
defineProps<Props>();

// Cada bloque de swatches lee/escribe únicamente NOMBRES de clase Tailwind
// definidos en tailwind.config.js (paletas alternative/application/fondo).
// Para agregar más opciones de theming basta con: 1) añadir el tono en
// tailwind.config.js y 2) añadirlo a uno de estos arrays.
const colorStore = useColorStore();
const settingApp = colorStore.settingApp;

const emit = defineEmits(['updateTopbarClass', 'updateMenuType', 'updateMenuClass', 'updateFondoClass']);

const primaryColors = ref([
    'bg-alternative-red-100', 'bg-alternative-red-200',
    'bg-alternative-purple-100', 'bg-alternative-purple-200', 'bg-alternative-purple-300',
    'bg-alternative-blue-100', 'bg-alternative-blue-200', 'bg-alternative-blue-300',
    'bg-alternative-green-100', 'bg-alternative-green-200', 'bg-alternative-green-300',
    'bg-alternative-yellow',
    'bg-alternative-orange-100', 'bg-alternative-orange-200',
]);

const fondoColor = ref(['bg-fondo-100', 'bg-fondo-200', 'bg-fondo-300', 'bg-fondo-400', 'bg-fondo-500', 'bg-fondo-600', 'bg-fondo-700']);

const surfaceColors = ref(['bg-application-blue-100', 'bg-application-blue-300', 'bg-application-blue-500', 'bg-application-blue-600']);

const topbarColorClasses = ref(['bg-application-silver-800', 'bg-application-silver-300', 'bg-application-silver-100']);

const isDarkMode = computed({
    get: () => settingApp.isDark,
    set: (value: boolean) => colorStore.setDarkMode(value),
});

const isHorizontal = ref(settingApp.isMenuHorizontal);

const setPrimaryColor = (colorClass: string) => {
    settingApp.primaryColor = colorClass;
    colorStore.setPrimaryColor(colorClass);
};
const setFondoColor = (colorClass: string) => {
    settingApp.fondoClass = colorClass;
    emit('updateFondoClass', colorClass);
};
const setMenuColor = (colorClass: string) => {
    settingApp.menuClass = colorClass;
    emit('updateMenuClass', colorClass);
};
const setTopbarColor = (colorClass: string) => {
    settingApp.topbarClass = colorClass;
    emit('updateTopbarClass', colorClass);
};
const toggleIsHorizontal = () => {
    settingApp.isMenuHorizontal = isHorizontal.value;
    emit('updateMenuType', isHorizontal.value ? 'horizontal' : 'vertical');
};
</script>
