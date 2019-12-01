using System;

namespace Bhbk.Lib.Identity.Data.Primitives.Enums
{
    public enum ActionType
    {
        Allow,
        Deny
    }

    public enum LoginType
    {
        CreateAudienceAccessTokenV1,
        CreateAudienceAccessTokenV2,
        CreateAudienceRefreshTokenV1,
        CreateAudienceRefreshTokenV2,
        CreateUserAccessTokenV1Legacy,
        CreateUserAccessTokenV1,
        CreateUserAccessTokenV2,
        CreateUserRefreshTokenV1,
        CreateUserRefreshTokenV2,
    }

    public enum MessageType
    {
        ActivityAleadyExists,
        ActivityNotFound,
        AudienceAlreadyExists,
        AudienceImmutable,
        AudienceInvalid,
        AudienceNotFound,
        ClaimAlreadyExists,
        ClaimImmutable,
        ClaimInvalid,
        ClaimNotFound,
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
        MOTDAlreadyExists,
        MOTDNotFound,
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
        UriNotFound,
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
