CREATE TABLE [dbo].[TRoleClaims] (
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME         NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TRoleClaims_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_TRoleClaims] PRIMARY KEY CLUSTERED ([RoleId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_TRoleClaims_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[TClaims] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_TRoleClaims_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[TRoles] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TRoleClaims]
    ON [dbo].[TRoleClaims]([RoleId] ASC, [ClaimId] ASC);

