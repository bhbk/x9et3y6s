
export interface IIdentityJwtV1 {
    token_type: string;
    access_token: string;
    refresh_token: string;
    user_id: string;
    client_id: string;
    issuer_id: string;
}

export interface IIdentityJwtV2 {
    token_type: string;
    access_token: string;
    refresh_token: string;
    user: string;
    client: string[];
    issuer: string;
}

export interface IIdentityLoginV1 {
    issuer_id: string;
    client_id: string;
    username: string;
    password: string;
    grant_type: string;
}

export interface IIdentityLoginV2 {
    issuer: string;
    client: string[];
    user: string;
    password: string;
    grant_type: string;
}

export interface IIdentityRefreshV1 {
    issuer_id: string;
    client_id: string;
    refresh_token: string;
    grant_type: string;
}

export interface IIdentityRefreshV2 {
    issuer: string;
    client: string[];
    refresh_token: string;
    grant_type: string;
}
