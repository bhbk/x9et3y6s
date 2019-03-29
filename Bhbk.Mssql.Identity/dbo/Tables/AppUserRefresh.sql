CREATE TABLE [dbo].[AppUserRefresh] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER NOT NULL,
    [UserId]          UNIQUEIDENTIFIER NOT NULL,
    [ProtectedTicket] NVARCHAR (MAX)   NOT NULL,
    [IssuedUtc]       DATETIME2 (7)    NOT NULL,
    [ExpiresUtc]      DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_AppUserRefresh] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppUserRefresh_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[AppIssuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AppUserRefresh_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[AppUser] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppUserRefresh]
    ON [dbo].[AppUserRefresh]([Id] ASC);

