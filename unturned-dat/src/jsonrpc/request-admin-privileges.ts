import { NotificationType } from 'vscode-languageclient';

export const RequestAdminPrivileges = new NotificationType<RequestAdminPrivilegesParams>("unturnedDataFile/requestAdminPrivileges");
export const SendAdminPrivilegesResponse = new NotificationType<SendAdminPrivilegesResponseParams>("unturnedDataFile/sendAdminPrivilegesResponse");

export interface RequestAdminPrivilegesParams
{
    readonly message: string,
    readonly type: number;
}

export interface SendAdminPrivilegesResponseParams
{
    readonly allowed: boolean,
    readonly type: number;
}