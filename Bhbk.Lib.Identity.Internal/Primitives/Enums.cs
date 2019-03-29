using System;

namespace Bhbk.Lib.Identity.Internal.Primitives.Enums
{
    public enum MsgType
    {
        PagerException,
        ClaimAlreadyExists,
        ClaimImmutable,
        ClaimInvalid,
        ClaimNotFound,
        ClientAlreadyExists,
        ClientImmutable,
        ClientInvalid,
        ClientNotFound,
        IssuerAlreadyExists,
        IssuerImmutable,
        IssuerInvalid,
        IssuerNotFound,
        LoginAlreadyExists,
        LoginImmutable,
        LoginInvalid,
        LoginNotFound,
        RoleAlreadyExists,
        RoleImmutable,
        RoleInvalid,
        RoleNotFound,
        UserAlreadyExists,
        UserImmutable,
        UserInvalid,
        UserNotFound,
    }

    //https://tools.ietf.org/html/rfc6749#section-2.1
    public enum ClientType
    {
        user_agent,
        native,
        server,
    }

    public enum LoginType
    {
        GenerateAccessTokenV1Legacy,
        GenerateAccessTokenV1,
        GenerateAccessTokenV2,
        GenerateAuthorizationCodeV1,
        GenerateAuthorizationCodeV2,
        GenerateClientCredentialV1,
        GenerateClientCredentialV2,
        GenerateRefreshTokenV1,
        GenerateRefreshTokenV2,
    }

    public enum TaskType
    {
        MaintainActivity,
        MaintainQuotes,
        MaintainTokens,
        MaintainUsers,
        QueueEmails,
        QueueTexts
    }
}
