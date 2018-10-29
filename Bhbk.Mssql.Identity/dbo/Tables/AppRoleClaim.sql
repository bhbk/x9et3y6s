CREATE TABLE [dbo].[AppRoleClaim] (
    [Id]             INT              IDENTITY (1000, 1) NOT NULL,
    [RoleId]         UNIQUEIDENTIFIER NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER NULL,
    [ClaimType]      NVARCHAR (128)   NOT NULL,
    [ClaimValue]     VARCHAR (MAX)    NOT NULL,
    [ClaimTypeValue] NVARCHAR (50)    NULL,
    [Created]        DATETIME         NOT NULL,
    [LastUpdated]    DATETIME         NULL,
    [Immutable]      BIT              NOT NULL,
    CONSTRAINT [PK_AppRoleClaim] PRIMARY KEY CLUSTERED ([Id] ASC, [RoleId] ASC),
    CONSTRAINT [FK_AppRoleClaim_RoleID] FOREIGN KEY ([RoleId]) REFERENCES [dbo].[AppRole] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppRoleClaim]
    ON [dbo].[AppRoleClaim]([Id] ASC, [RoleId] ASC);

