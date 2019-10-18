CREATE TABLE [dbo].[tbl_RoleClaims] (
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME         NOT NULL,
    [Immutable] BIT              CONSTRAINT [DF_TRoleClaims_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_RoleClaims] PRIMARY KEY CLUSTERED ([RoleId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_RoleClaims_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claims] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_RoleClaims_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Roles] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RoleClaims]
    ON [dbo].[tbl_RoleClaims]([RoleId] ASC, [ClaimId] ASC);

