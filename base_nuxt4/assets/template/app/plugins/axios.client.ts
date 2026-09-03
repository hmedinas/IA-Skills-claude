import axios from 'axios';

// Instancia única de axios, provista como $axiosApp en toda la app.
// La baseURL sale de runtimeConfig.public.appConfig.urlService, que a su
// vez depende del stage (development/staging/preproduction/production)
// resuelto en nuxt.config.ts a partir de NUXT_ENV_STAGE.
export default defineNuxtPlugin(async (nuxtApp) => {
    const configRuntime = useRuntimeConfig();
    const envConfig = configRuntime.public.appConfig;
    const authStore = useAuthStore();

    const axiosInstanceApp = axios.create({
        baseURL: envConfig.urlService,
        timeout: 60000,
    });

    axiosInstanceApp.interceptors.request.use(
        (requestConfig) => {
            requestConfig.headers['Authorization'] = `Bearer ${authStore.token}`;
            return requestConfig;
        },
        (error) => Promise.reject(error),
    );

    const handleAxiosError = (error: any) => {
        if (axios.isAxiosError(error)) {
            const status = error.response?.status;
            const data = error.response?.data;

            if (authStore.getError === null) {
                authStore.setError(status ?? 500, data);
            }

            return Promise.reject({ status, message: error.message, data });
        }
        return Promise.reject(error);
    };

    axiosInstanceApp.interceptors.response.use(
        (response) => response,
        (error) => handleAxiosError(error),
    );

    return {
        provide: {
            axiosApp: axiosInstanceApp,
        },
    };
});
