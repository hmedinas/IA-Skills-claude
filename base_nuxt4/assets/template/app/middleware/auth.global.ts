import navigationData from '@/config/menu.json';
import { UserService } from '~/services/user.service';
import { TypeTransaction } from '~/types/transaction.interface';
import type { ResponseApi } from '~/types/responseApi.interface';
import type { AccessUser } from '~/types/access.interface';

// Guard global de rutas. Corre en cada navegación (SPA: ssr:false, así que
// esto vive enteramente en el cliente). Cuatro responsabilidades, en orden:
// 1) redirigir raíz -> login, 2) dejar pasar rutas públicas, 3) exigir
// sesión, 4) si hay token pero no hay `user` en el store (recién logueado o
// F5), pedir el perfil UNA vez y cachearlo en Pinia; 5) actualizar el título
// del topbar según menu.json.
export default defineNuxtRouteMiddleware(async (to) => {
    const authStore = useAuthStore();
    const navStore = useNavigationStore();
    const currentPath = to.path;

    if (currentPath === '/') {
        return navigateTo('/auth/login');
    }

    const publicRoutes = ['/auth/login', '/auth/logout', '/errors/401', '/errors/403', '/errors/404', '/errors/500'];
    if (publicRoutes.includes(currentPath)) {
        return;
    }

    if (!authStore.isAuthenticated) {
        return navigateTo('/auth/login');
    }

    if (authStore.user === undefined) {
        try {
            const userService = await UserService();
            const result: ResponseApi<AccessUser> = await userService.GetProfile(authStore.idUser);

            if (result && result.type === TypeTransaction.OK) {
                const image = await userService.GetImageUser();
                authStore.completeImageUser(image);
                authStore.completeUser(result.data);
            } else {
                authStore.setError(401, 'No se pudo validar el perfil de acceso.');
                return navigateTo(authStore.redirectPageError);
            }
        } catch (error) {
            if (authStore.getError === null) {
                authStore.setError(403, typeof error === 'string' ? error : 'Error de sincronización de perfil');
            }
            return navigateTo(authStore.redirectPageError);
        }
    }

    for (const item of navigationData.menu as any[]) {
        if (item.to === currentPath) {
            navStore.setNavigation(item.label, '', '__APP_NAME__');
            break;
        }
        if (item.children) {
            const child = item.children.find((c: any) => c.to === currentPath);
            if (child) {
                navStore.setNavigation(child.label, item.label, '__APP_NAME__');
                break;
            }
        }
    }
});
