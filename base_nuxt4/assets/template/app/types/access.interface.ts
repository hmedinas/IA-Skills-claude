// Perfil de acceso del usuario autenticado. En el proyecto de referencia esto
// crece a un árbol empresa > cliente > almacén > roles; aquí se deja un
// roles:string[] plano como base — amplíalo si tu dominio necesita jerarquía de acceso.
export interface AccessUser {
    idUser: number;
    userName: string;
    displayName: string;
    email: string;
    roles: string[];
}
