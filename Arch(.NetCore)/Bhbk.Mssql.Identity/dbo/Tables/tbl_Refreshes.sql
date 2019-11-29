CREATE TABLE [dbo].[tbl_Refreshes] (
    [Id]           UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]     UNIQUEIDENTIFIER NOT NULL,
    [ClientId]     UNIQUEIDENTIFIER NULL,
    [UserId]       UNIQUEIDENTIFIER NULL,
    [RefreshValue] NVARCHAR (MAX)   NOT NULL,
    [RefreshType]  NVARCHAR (64)    NOT NULL,
    [ValidFromUtc] DATETIME2 (7)    NOT NULL,
    [ValidToUtc]   DATETIME2 (7)    NOT NULL,
    [Created]      DATETIME2 (7)    NOT NULL,
    CONSTRAINT [PK_Refreshes] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_Refreshes_ClientID] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[tbl_Clients] ([Id]),
    CONSTRAINT [FK_Refreshes_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[tbl_Issuers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_Refreshes_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[tbl_Users] ([Id])
);






GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Refreshes]
    ON [dbo].[tbl_Refreshes]([Id] ASC);

