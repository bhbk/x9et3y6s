CREATE TABLE [dbo].[tbl_AudienceRoles] (
    [AudienceId] UNIQUEIDENTIFIER NOT NULL,
    [RoleId]     UNIQUEIDENTIFIER NOT NULL,
    [ActorId]    UNIQUEIDENTIFIER NULL,
    [Created]    DATETIME2 (7)    NOT NULL,
    [Immutable]  BIT              NOT NULL,
    CONSTRAINT [PK_AudienceRoles] PRIMARY KEY CLUSTERED ([AudienceId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AudienceRoles_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audiences] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AudienceRoles_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Roles] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AudienceRoles]
    ON [dbo].[tbl_AudienceRoles]([AudienceId] ASC, [RoleId] ASC);

