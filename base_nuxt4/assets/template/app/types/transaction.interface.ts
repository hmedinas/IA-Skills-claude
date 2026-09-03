export enum TypeTransaction {
    ERR = 'ERR',
    OK = 'OK',
    NOT_FOUND = 'NOTFOUND',
}

export interface Transaction {
    Type: TypeTransaction;
    Message: string;
    Key?: string;
    Output?: any;
}

export const mapToTransaction = (data: any): Transaction => {
    return {
        Type: data.type,
        Message: data.message,
        Key: data.key,
        Output: data.output,
    };
};
