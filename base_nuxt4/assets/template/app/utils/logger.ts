export const logger = {
    log: (...args: any[]) => {
        const configRuntime = useRuntimeConfig();
        if (configRuntime.public.appConfig.isActiveConsole) {
            console.log(...args);
        }
    },
    warn: (...args: any[]) => {
        if (process.dev) console.warn(...args);
    },
    error: (...args: any[]) => {
        console.error(...args);
    },
};
