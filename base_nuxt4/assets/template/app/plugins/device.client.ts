import type { Device } from '~/types/device.interface';

// Detección simple de dispositivo, usada por el layout para decidir si el
// menú lateral debe comportarse como drawer flotante (móvil) o fijo colapsable
// (escritorio/tablet).
export default defineNuxtPlugin(() => {
    const getDevice = (): Device => {
        const userAgent = navigator.userAgent || navigator.vendor || (window as any).opera;
        const width = window.innerWidth;

        const isIpad = /iPad/.test(userAgent) || (navigator.platform === 'MacIntel' && navigator.maxTouchPoints > 1);
        const isMobile = /Android|webOS|iPhone|iPod|BlackBerry|IEMobile|Opera Mini/i.test(userAgent) && width < 768;
        const isTablet = (isIpad || /Android/i.test(userAgent)) && width >= 768 && width <= 1024;

        return { isMobile, isTablet, isIpad };
    };

    return {
        provide: {
            device: getDevice(),
        },
    };
});
