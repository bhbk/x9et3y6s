CREATE TABLE [dbo].[tbl_UserClaims] (
    [UserId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TUserClaims_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_UserClaims] PRIMARY KEY CLUSTERED ([UserId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_UserClaims_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claims] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_UserClaims_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_UserClaims]
    ON [dbo].[tbl_UserClaims]([UserId] ASC, [ClaimId] ASC);

