using System;

namespace Bhbk.Lib.Identity.Internal.Primitives.Enums
{
    public enum MsgType
    {
        ClaimAlreadyExists,
        ClaimImmutable,
        ClaimInvalid,
        ClaimNotFound,
        ClientAlreadyExists,
        ClientImmutable,
        ClientInvalid,
        ClientNotFound,
        EmailDequeueError,
        EmailEnueueError,
        IssuerAlreadyExists,
        IssuerImmutable,
        IssuerInvalid,
        IssuerNotFound,
        LoginAlreadyExists,
        LoginImmutable,
        LoginInvalid,
        LoginNotFound,
        ParametersInvalid,
        ParseError,
        RoleAlreadyExists,
        RoleImmutable,
        RoleInvalid,
        RoleNotFound,
        TextDequeueError,
        TextEnqueueError,
        UriInvalid,
        UriNotFound,
        UserAlreadyExists,
        UserImmutable,
        UserInvalid,
        UserNotFound,
        TokenInvalid,
    }

    //https://tools.ietf.org/html/rfc6749#section-2.1
    public enum ClientType
    {
        user_agent,
        native,
        server,
    }

    public enum CodeType
    {
        Client,
        User,
    }

    public enum RefreshType
    {
        Client,
        Device,
        User,
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
