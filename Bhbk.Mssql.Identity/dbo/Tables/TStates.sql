CREATE TABLE [dbo].[TStates] (
    [Id]            UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]      UNIQUEIDENTIFIER NOT NULL,
    [ClientId]      UNIQUEIDENTIFIER NULL,
    [UserId]        UNIQUEIDENTIFIER NULL,
    [NonceValue]    NVARCHAR (MAX)   NULL,
    [NonceType]     NVARCHAR (64)    NOT NULL,
    [NonceConsumed] BIT              CONSTRAINT [DF_TStates_NonceConsumed] DEFAULT ((0)) NOT NULL,
    [ValidFromUtc]  DATETIME2 (7)    NOT NULL,
    [ValidToUtc]    DATETIME2 (7)    NOT NULL,
    [Created]       DATETIME2 (7)    NOT NULL,
    [LastPolling]   DATETIME2 (7)    NULL,
    CONSTRAINT [PK_States] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_TStates_TClients] FOREIGN KEY ([ClientId]) REFERENCES [dbo].[TClients] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_TStates_TIssuers] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[TIssuers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_TStates_UserID] FOREIGN KEY ([UserId]) REFERENCES [dbo].[TUsers] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_States]
    ON [dbo].[TStates]([Id] ASC);

