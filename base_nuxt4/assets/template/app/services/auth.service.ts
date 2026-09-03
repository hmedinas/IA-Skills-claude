import { useNuxtApp } from '#app';
import { EndPoint } from '~/types/endpoint.enum';
import type { ResponseApi } from '~/types/responseApi.interface';
import type { AuthResponse } from '~/types/auth.response';
import { TypeTransaction } from '~/types/transaction.interface';
import mockAuth from '~/mock/auth.mock.json';

// Patrón de servicio: cada método público primero mira
// config.configuration.useMockUser. Si está en true, resuelve con el mock
// local (misma forma de respuesta que la API real) sin tocar la red.
// Cuando el backend esté listo, apagar ese flag en config.json activa las
// llamadas reales SIN cambiar una sola línea de los componentes que llaman
// a este servicio.
export const AuthService = () => {
    const { $axiosApp } = useNuxtApp();

    const Login = async (user: string, pass: string): Promise<ResponseApi<AuthResponse>> => {
        try {
            const config = await import('@/config/config.json');

            if (config.configuration.useMockUser) {
                return await __Login(user, pass);
            }

            const response = await $axiosApp.post(EndPoint.Auth.Login, { userName: user, password: pass });
            return response.data;
        } catch (error) {
            logger.error('Error in Login: ', error);
            throw error;
        }
    };

    // Mock privado: simula latencia de red y devuelve el mismo shape
    // ResponseApi<AuthResponse> que devolvería el backend real.
    const __Login = async (_user: string, _pass: string): Promise<ResponseApi<AuthResponse>> => {
        await new Promise((resolve) => setTimeout(resolve, 600));
        return {
            idResponse: 200,
            type: TypeTransaction.OK,
            message: 'OK',
            totalRows: 1,
            data: mockAuth.authResponse as AuthResponse,
        };
    };

    return { Login };
};
