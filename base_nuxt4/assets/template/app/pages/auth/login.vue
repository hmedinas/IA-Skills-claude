<template>
  <div class="relative min-h-screen w-full flex items-center justify-center lg:justify-end font-sans overflow-hidden bg-gradient-to-br from-application-blue-600 via-application-blue-500 to-slate-900">
    <!-- Fondo decorativo: gradiente + patrón, sin depender de imágenes externas -->
    <div class="absolute inset-0 opacity-20" style="background-image: radial-gradient(circle at 1px 1px, rgba(255,255,255,0.5) 1px, transparent 0); background-size: 24px 24px;"></div>
    <div class="absolute inset-0 bg-[radial-gradient(circle,_transparent_20%,_rgba(15,23,42,0.7)_100%)]"></div>

    <div class="absolute bottom-12 left-12 z-20 hidden lg:block">
      <div class="bg-white/10 backdrop-blur-md border border-white/10 p-6 rounded-2xl shadow-2xl max-w-xs">
        <p class="text-white font-black text-xl uppercase tracking-tight">{{ appName }}</p>
        <div class="h-[1px] w-full bg-white/20 my-3"></div>
        <p class="text-white/60 text-[10px] font-bold uppercase tracking-[0.3em]">
          Base Nuxt4 · login + menú + dark mode
        </p>
      </div>
    </div>

    <div class="relative z-30 mr-0 lg:mr-16 xl:mr-32 w-full max-w-[420px] px-4">
      <div class="bg-white p-10 rounded-[2.5rem] shadow-[0_20px_50px_rgba(0,0,0,0.3)] border border-slate-100">
        <h2 class="text-3xl font-bold text-slate-800 text-center uppercase tracking-tighter mb-2">
          {{ appName }}
        </h2>
        <p class="text-center text-slate-400 text-xs font-bold uppercase tracking-widest mb-8">Iniciar Sesión</p>

        <div class="space-y-5">
          <div class="relative">
            <span class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400">
              <font-awesome icon="user" />
            </span>
            <input
              v-model="userField"
              type="text"
              placeholder="USUARIO"
              class="w-full pl-14 pr-5 py-4 bg-slate-50 border-2 border-transparent rounded-2xl focus:border-primary focus:bg-white outline-none transition-all font-bold text-slate-700 placeholder:text-slate-300"
            />
          </div>

          <div class="relative">
            <span class="absolute left-5 top-1/2 -translate-y-1/2 text-slate-400">
              <font-awesome icon="shield-halved" />
            </span>
            <input
              v-model="passField"
              type="password"
              placeholder="CONTRASEÑA"
              class="w-full pl-14 pr-5 py-4 bg-slate-50 border-2 border-transparent rounded-2xl focus:border-primary focus:bg-white outline-none transition-all font-bold text-slate-700 placeholder:text-slate-300"
            />
          </div>

          <button
            type="button"
            @click="login"
            :disabled="isProcessing"
            class="w-full mt-6 py-4 bg-application-blue-600 hover:bg-application-blue-500 text-white font-bold rounded-2xl shadow-lg active:scale-95 transition-all flex justify-center items-center gap-3 uppercase tracking-wider disabled:opacity-60"
          >
            <font-awesome v-if="isProcessing" icon="spinner" class="animate-spin" />
            <span>{{ isProcessing ? 'Verificando...' : 'Iniciar Sesión' }}</span>
          </button>
        </div>

        <transition name="fade">
          <div v-if="errorMessage" class="mt-6 p-4 bg-red-50 text-red-600 font-bold rounded-xl border border-red-100 flex items-center gap-2">
            <font-awesome icon="circle-exclamation" />
            {{ errorMessage }}
          </div>
        </transition>

        <p class="mt-10 text-center text-[9px] text-slate-400 font-bold uppercase tracking-tighter">
          © {{ appName }} — v.{{ appVersion }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router';
import { TypeResponse, type ResponseApi } from '~/types/responseApi.interface';
import { AuthService } from '~/services/auth.service';
import type { AuthResponse } from '~/types/auth.response';

definePageMeta({
    layout: 'session',
});

const router = useRouter();
const authStore = useAuthStore();
const authService = AuthService();

const config = await import('@/config/config.json');
const appName = config.configuration.nameApplication as string;
const appVersion = config.configuration.version as string;

const userField = ref('');
const passField = ref('');
const isProcessing = ref(false);
const errorMessage = ref('');

const login = async () => {
    if (!userField.value || !passField.value) {
        errorMessage.value = 'Por favor, completa todos los campos.';
        return;
    }

    isProcessing.value = true;
    errorMessage.value = '';

    try {
        const response: ResponseApi<AuthResponse> = await authService.Login(userField.value, passField.value);

        if (response.idResponse === TypeResponse.OK && response.data.isAuthenticated) {
            authStore.login(response.data.idUser, response.data.accessToken, response.data.refresh_token, response.data.expiresIn);
            router.push('/home');
        } else {
            errorMessage.value = 'Usuario o contraseña no válidos.';
        }
    } catch (err) {
        logger.error('Error during login:', err);
        errorMessage.value = 'Error crítico en el servidor de autenticación.';
    } finally {
        isProcessing.value = false;
    }
};

const handleKeyDown = (e: KeyboardEvent) => {
    if (e.key === 'Enter' && !isProcessing.value) login();
};

onMounted(() => {
    authStore.logout();
    window.addEventListener('keydown', handleKeyDown);
});

onUnmounted(() => {
    window.removeEventListener('keydown', handleKeyDown);
});
</script>

<style scoped>
.fade-enter-active, .fade-leave-active { transition: opacity 0.3s ease; }
.fade-enter-from, .fade-leave-to { opacity: 0; }
</style>
