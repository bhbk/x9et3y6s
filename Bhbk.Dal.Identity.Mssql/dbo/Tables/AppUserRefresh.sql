CREATE TABLE [dbo].[AppUserRefresh] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [ClientId]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [ProtectedTicket] VARCHAR (MAX)    NOT NULL,
    [IssuedUtc]       DATETIME2 (7)    NOT NULL,
    [ExpiresUtc]      DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_AppUserRefresh_ID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppUserRefresh_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[AppClient] ([Id]),
    CONSTRAINT [FK_AppUserRefresh_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserRefresh_ID]
    ON [dbo].[AppUserRefresh]([Id] ASC);

