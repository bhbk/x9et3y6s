CREATE TABLE [dbo].[TUserClaims] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              NOT NULL,
    CONSTRAINT [PK_TUserClaims] PRIMARY KEY CLUSTERED ([UserId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_TUserClaims_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[TClaims] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_TUserClaims_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TUserClaims]
    ON [dbo].[TUserClaims]([UserId] ASC, [ClaimId] ASC);

