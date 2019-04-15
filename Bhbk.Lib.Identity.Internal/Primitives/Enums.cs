using System;

namespace Bhbk.Lib.Identity.Internal.Primitives.Enums
{
    public enum ActionType
    {
        Allow,
        Deny
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
        CreateUserAccessTokenV1Legacy,
        CreateUserAccessTokenV1,
        CreateUserAccessTokenV2,
        CreateUserRefreshTokenV1,
        CreateUserRefreshTokenV2,
        CreateClientAccessTokenV1,
        CreateClientAccessTokenV2,
        CreateClientRefreshTokenV1,
        CreateClientRefreshTokenV2,
    }

    public enum MessageType
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
        StateDenied,
        StateInvalid,
        StateNotFound,
        StatePending,
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
