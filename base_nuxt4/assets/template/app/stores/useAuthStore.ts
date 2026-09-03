import { useLocalStorage } from '@vueuse/core';
import { defineStore } from 'pinia';
import { computed, ref } from 'vue';
import type { AccessUser } from '~/types/access.interface';
import type { ErrorResponse } from '~/types/error.interface';

export const useAuthStore = defineStore('auth', () => {
    const user = ref<AccessUser | undefined>();
    const imageUser = ref<string>('');
    // token/refresh_token/idUser en localStorage: sobreviven a un F5.
    // authStore.user en cambio NO se persiste — se vuelve a pedir al
    // middleware en cada arranque de la app (ver auth.global.ts).
    const token = ref(useLocalStorage<string>('token', ''));
    const refresh_token = ref(useLocalStorage<string>('refresh_token', ''));
    const idUser = ref(useLocalStorage<number>('idUser', 0));
    const errorResponse = ref<ErrorResponse | null>(null);
    const tokenExpirationTime = ref<number>(0);

    const login = (idUsuario: number, accessToken: string, refreshToken: string, expiresIn: number = 3600) => {
        token.value = accessToken;
        refresh_token.value = refreshToken;
        idUser.value = idUsuario;
        tokenExpirationTime.value = new Date().getTime() + expiresIn * 1000;
    };

    const logout = () => {
        errorResponse.value = null;
        token.value = '';
        refresh_token.value = '';
        user.value = undefined;
        idUser.value = 0;
        imageUser.value = '';
        tokenExpirationTime.value = 0;
    };

    const completeUser = (userAccess: AccessUser) => {
        user.value = token.value ? userAccess : undefined;
    };

    const completeImageUser = (imageUrl: string) => {
        imageUser.value = token.value ? imageUrl : '';
    };

    const setError = (type: number, error: string) => {
        errorResponse.value = {
            type,
            fecha: new Date(),
            descripcion: error ?? '',
        };
    };

    const cleanError = () => {
        errorResponse.value = null;
    };

    // Mapea el tipo de error HTTP a la página de error correspondiente.
    // authStore.redirectPageError se usa tanto en el middleware como en
    // el interceptor de axios (axios.client.ts).
    const pageError = computed(() => {
        switch (errorResponse.value?.type) {
            case 401: return '/errors/401';
            case 403: return '/errors/403';
            case 404: return '/errors/404';
            default: return '/errors/500';
        }
    });

    return {
        user: computed(() => user.value),
        token: computed(() => token.value),
        idUser: computed(() => idUser.value),
        imageUser: computed(() => imageUser.value),
        isAuthenticated: computed(() => token.value !== ''),
        getError: computed(() => errorResponse.value),
        redirectPageError: pageError,
        login,
        logout,
        completeUser,
        completeImageUser,
        setError,
        cleanError,
    };
});
