using System;

namespace Bhbk.Lib.Identity.Primitives.Enums
{
    //https://tools.ietf.org/html/rfc6749#section-2.1
    public enum AudienceType
    {
        user_agent,
        native,
        server,
    }

    public enum ActionType
    {
        Allow,
        Deny,
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
        BasicAuthV1,
        BasicAuthV2,
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
        EmailInvalid,
        EmailNotFound,
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
        PhoneNumberInvalid,
        RoleAlreadyExists,
        RoleImmutable,
        RoleInvalid,
        RoleNotFound,
        StateDenied,
        StateInvalid,
        StateNotFound,
        StatePending,
        StateSlowDown,
        TextNotFound,
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
}
