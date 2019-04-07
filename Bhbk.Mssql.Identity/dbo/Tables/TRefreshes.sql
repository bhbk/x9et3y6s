CREATE TABLE [dbo].[TRefreshes] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]     UNIQUEIDENTIFIER NOT NULL,
    [ClientId]     UNIQUEIDENTIFIER NULL,
    [UserId]       UNIQUEIDENTIFIER NULL,
    [RefreshValue] NVARCHAR (MAX)   NOT NULL,
    [RefreshType]  NVARCHAR (64)    NOT NULL,
    [ValidFromUtc] DATETIME2 (7)    NOT NULL,
    [ValidToUtc]   DATETIME2 (7)    NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_TRefreshes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TRefreshes_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[TClients] ([Id]),
    CONSTRAINT [FK_TRefreshes_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[TIssuers] ([Id]),
    CONSTRAINT [FK_TRefreshes_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TUsers] ([Id])
);








GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TRefreshes]
    ON [dbo].[TRefreshes]([Id] ASC);

