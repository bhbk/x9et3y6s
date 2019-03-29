CREATE TABLE [dbo].[AppClientRefresh] (
    [Id]              UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]        UNIQUEIDENTIFIER NOT NULL,
    [ClientId]        UNIQUEIDENTIFIER NOT NULL,
    [ProtectedTicket] NVARCHAR (MAX)   NOT NULL,
    [IssuedUtc]       DATETIME2 (7)    NOT NULL,
    [ExpiresUtc]      DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_AppClientRefresh] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppClientRefresh_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[AppClient] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_AppClientRefresh_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[AppIssuer] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppClientRefresh]
    ON [dbo].[AppClientRefresh]([Id] ASC);

