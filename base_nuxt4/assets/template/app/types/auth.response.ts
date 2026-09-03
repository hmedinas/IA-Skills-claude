export interface AuthResponse {
    idUser: number;
    accessToken: string;
    refresh_token: string;
    expiresIn: number;
    isAuthenticated: boolean;
}
