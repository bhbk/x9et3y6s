CREATE TABLE [dbo].[AppUserClaim] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              NOT NULL,
    CONSTRAINT [PK_AppUserClaim] PRIMARY KEY CLUSTERED ([UserId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_AppUserClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[AppClaim] ([Id]),
    CONSTRAINT [FK_AppUserClaim_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserClaim]
    ON [dbo].[AppUserClaim]([UserId] ASC, [ClaimId] ASC);



