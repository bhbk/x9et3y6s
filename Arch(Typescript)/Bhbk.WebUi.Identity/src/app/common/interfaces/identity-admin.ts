import { Observable } from "rxjs";

export interface IIdentityActivityList {
    list: IIdentityActivityModel[];
    count: number;
}

export interface IIdentityActivityModel {
    id: string;
    actorId: string;
    activityType: string;
    tableName: string;
    keyValues: string;
    originalValues: string;
    currentValues: string;
    created: Date;
    immutable: boolean;
}

/*
*/

export interface IIdentityClientList {
    list: IIdentityClientModel[];
    count: number;
}

interface IIdentityClientBase {
    issuerId: string;
    name: string;
    description: string;
    clientType: string;
    enabled: boolean;
    immutable: boolean;
}

export interface IIdentityClientCreate extends IIdentityClientBase { }

export interface IIdentityClientModel extends IIdentityClientBase {
    id: string;
    created: Date;
    lastUpdated: Date;
    roles: Observable<string[]>;
}

/*
*/

export interface IIdentityClaimList {
    list: IIdentityClaimModel[];
    count: number;
}

interface IIdentityClaimBase {
    issuerId: string;
    subject: string;
    type: string;
    value: string;
    valueType: string;
    immutable: boolean;
}

export interface IIdentityClaimCreate extends IIdentityClaimBase { }

export interface IIdentityClaimModel extends IIdentityClaimBase {
    id: string;
    created: Date;
    lastUpdated: Date;
}

/*
*/

export interface IIdentityIssuerList {
    list: IIdentityIssuerModel[];
    count: number;
}

interface IIdentityIssuerBase {
    name: string;
    description: string;
    issuerKey: string;
    enabled: boolean;
    immutable: boolean;
}

export interface IIdentityIssuerCreate extends IIdentityIssuerBase { }

export interface IIdentityIssuerModel extends IIdentityIssuerBase {
    id: string;
    created: Date;
    lastUpdated: Date;
    audiences: Observable<string[]>;
}

/*
*/

export interface IIdentityLoginSet {
    list: IIdentityLoginModel[];
    count: number;
}

interface IIdentityLoginBase {
    name: string;
    immutable: boolean;
}

export interface IIdentityLoginCreate extends IIdentityLoginBase { }

export interface IIdentityLoginModel extends IIdentityLoginBase {
    id: string;
    users: Observable<string[]>;
}

/*
*/

export interface IIdentityRoleList {
    list: IIdentityRoleModel[];
    count: number;
}

interface IIdentityRoleBase {
    audienceId: string;
    name: string;
    description: string;
    enabled: boolean;
    immutable: boolean;
}

export interface IIdentityRoleCreate extends IIdentityRoleBase { }

export interface IIdentityRoleModel extends IIdentityRoleBase {
    id: string;
    created: Date;
    lastUpdated: Date;
    users: Observable<string[]>;
}

/*
*/

export interface IIdentityUserList {
    list: IIdentityUserModel[];
    count: number;
}

interface IIdentityUserBase {
    issuerId: string;
    email: string;
    phoneNumber: string;
    firstName: string;
    lastName: string;
    created: Date;
    lockoutEnabled: boolean;
    humanBeing: boolean;
    immutable: boolean;
}

export interface IIdentityUserCreate extends IIdentityUserBase { }

export interface IIdentityUserModel extends IIdentityUserBase {
    id: string;
    emailConfirmed: boolean;
    phoneNumberConfirmed: boolean;
    lastUpdated: Date;
    lockoutEnd: Date;
    lastLoginFailure: Date;
    lastLoginSuccess: Date;
    accessFailedCount: number;
    accessSuccessCount: number;
    passwordConfirmed: boolean;
    twoFactorConfirmed: boolean;
    roles: Observable<string[]>;
}
