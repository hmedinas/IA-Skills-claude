// Namespace único de endpoints. Cada servicio referencia SIEMPRE
// EndPoint.<Recurso>.<Accion> en vez de escribir strings sueltos: mover
// la app de mock a API real es solo apagar useMockUser en config.json,
// nunca tocar los services.
export namespace EndPoint {
    export enum Auth {
        Login = '/identity/login',
        Refresh = '/identity/refresh',
        ForgotPassword = '/identity/forgot-password',
    }
    export enum Usuario {
        GetAccess = '/usuario/access/{idUser}',
        GetAll = '/usuario',
        Save = '/usuario',
        Delete = '/usuario/{id}',
    }
}
