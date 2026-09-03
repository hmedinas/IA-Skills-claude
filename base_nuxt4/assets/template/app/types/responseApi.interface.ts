import type { TypeTransaction } from './transaction.interface';

export enum TypeResponse {
    OK = 200,
    Error = 400,
}

export interface ResponseApi<T> {
    idResponse: TypeResponse;
    message: string;
    totalRows: number;
    data: T;
    type: TypeTransaction;
}
