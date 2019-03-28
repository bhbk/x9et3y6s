CREATE TABLE [dbo].[AppRoleClaim] (
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ClaimId]   UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME         NOT NULL,
    [Immutable] BIT              NOT NULL,
    CONSTRAINT [PK_AppRoleClaim] PRIMARY KEY CLUSTERED ([RoleId] ASC, [ClaimId] ASC),
    CONSTRAINT [FK_AppRoleClaim_ClaimID] FOREIGN KEY ([ClaimId]) REFERENCES [dbo].[AppClaim] ([Id]),
    CONSTRAINT [FK_AppRoleClaim_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AppRole] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppRoleClaim]
    ON [dbo].[AppRoleClaim]([RoleId] ASC, [ClaimId] ASC);



