CREATE TABLE [dbo].[tbl_RoleClaim] (
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Immutable] BIT              CONSTRAINT [DF_tbl_RoleClaim_Immutable] DEFAULT ((0)) NOT NULL,
    [Created]   DATETIME         NOT NULL,
    CONSTRAINT [PK_tbl_RoleClaim] PRIMARY KEY CLUSTERED ([RoleId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_tbl_RoleClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[tbl_Claim] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbl_RoleClaim_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Role] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_RoleClaims]
    ON [dbo].[tbl_RoleClaim]([RoleId] ASC, [ClaimId] ASC);

