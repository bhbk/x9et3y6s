CREATE TABLE [dbo].[AppUserClaim] (
    [Id]             INT              IDENTITY (1000, 1) NOT NULL,
    [UserId]         UNIQUEIDENTIFIER NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER NULL,
    [ClaimType]      VARCHAR (MAX)    NOT NULL,
    [ClaimValue]     VARCHAR (MAX)    NOT NULL,
    [ClaimValueType] VARCHAR (MAX)    NULL,
    [Created]        DATETIME2 (7)    NOT NULL,
    [LastUpdated]    DATETIME2 (7)    NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_AppUserClaim] PRIMARY KEY CLUSTERED ([Id] ASC, [UserId] ASC),
    CONSTRAINT [FK_AppUserClaim_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_AppUserClaim]
    ON [dbo].[AppUserClaim]([Id] ASC, [UserId] ASC);

