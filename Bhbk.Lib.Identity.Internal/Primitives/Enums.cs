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
        StateAuthPending,
        StateInvalid,
        StateSlowDown,
        TextDequeueError,
        TextEnqueueError,
        TokenInvalid,
        UriInvalid,
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

    public enum RefreshType
    {
        Client,
        Device,
        User,
    }

    public enum StateType
    {
        Client,
        Device,
        User,
    }

    public enum LoginType
    {
        AskAuthorizationCodeV1,
        AskAuthorizationCodeV2,
        AskDeviceCodeV1,
        AskDeviceCodeV2,
        CreateUserAccessTokenV1Legacy,
        CreateUserAccessTokenV1,
        CreateUserAccessTokenV2,
        CreateUserRefreshTokenV1,
        CreateUserRefreshTokenV2,
        CreateClientAccessTokenV1,
        CreateClientAccessTokenV2,
        CreateClientRefreshTokenV1,
        CreateClientRefreshTokenV2,
        CreateDeviceAccessTokenV1,
        CreateDeviceAccessTokenV2,
        CreateDeviceRefreshTokenV1,
        CreateDeviceRefreshTokenV2,
    }

    public enum TaskType
    {
        MaintainActivity,
        MaintainQuotes,
        MaintainRefreshes,
        MaintainStates,
        MaintainUsers,
        QueueEmails,
        QueueTexts
    }
}
