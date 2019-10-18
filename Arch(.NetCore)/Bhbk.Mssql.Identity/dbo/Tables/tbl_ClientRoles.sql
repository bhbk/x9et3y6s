CREATE TABLE [dbo].[tbl_ClientRoles] (
    [ClientId]  UNIQUEIDENTIFIER NOT NULL,
    [RoleId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]   UNIQUEIDENTIFIER NULL,
    [Created]   DATETIME2 (7)    NOT NULL,
    [Immutable] BIT              NOT NULL,
    CONSTRAINT [PK_ClientRoles] PRIMARY KEY CLUSTERED ([ClientId] ASC, [RoleId] ASC),
    CONSTRAINT [FK_ClientRoles_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[tbl_Clients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_ClientRoles_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[tbl_Roles] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_ClientRoles]
    ON [dbo].[tbl_ClientRoles]([ClientId] ASC, [RoleId] ASC);

