import { useNuxtApp } from '#app';
import { EndPoint } from '~/types/endpoint.enum';
import type { ResponseApi } from '~/types/responseApi.interface';
import type { AccessUser } from '~/types/access.interface';
import { TypeTransaction } from '~/types/transaction.interface';
import mockAuth from '~/mock/auth.mock.json';

export const UserService = async () => {
    const { $axiosApp } = useNuxtApp();
    const configRuntime = useRuntimeConfig();

    const GetProfile = async (idUser: number): Promise<ResponseApi<AccessUser>> => {
        try {
            const config = await import('@/config/config.json');

            if (config.configuration.useMockUser) {
                await new Promise((resolve) => setTimeout(resolve, 300));
                return {
                    idResponse: 200,
                    type: TypeTransaction.OK,
                    message: 'OK',
                    totalRows: 1,
                    data: mockAuth.user as AccessUser,
                };
            }

            const url = EndPoint.Usuario.GetAccess.replace('{idUser}', idUser.toString());
            const response = await $axiosApp.get(url);
            return response.data;
        } catch (error) {
            logger.error('Error in GetProfile: ', error);
            throw error;
        }
    };

    // Placeholder de avatar: en la app real esto pega a un endpoint de
    // imagen y devuelve un blob URL. Aquí basta con la imagen estática.
    const GetImageUser = async (): Promise<string> => {
        return `${configRuntime.public.appConfig.pathGlobal}person.jpg`;
    };

    return { GetProfile, GetImageUser };
};
