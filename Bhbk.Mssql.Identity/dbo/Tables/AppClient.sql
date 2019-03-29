CREATE TABLE [dbo].[AppClient] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [IssuerId]    UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [ClientKey]   NVARCHAR (MAX)   NOT NULL,
    [ClientType]  NVARCHAR (64)    NOT NULL,
    [Enabled]     BIT              CONSTRAINT [DF_AppAudience_Enabled] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              CONSTRAINT [DF_AppAudience_Immutable] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_AppClient_ID] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_AppClient_IssuerID] FOREIGN KEY ([IssuerId]) REFERENCES [dbo].[AppIssuer] ([Id])
);










GO



GO
CREATE NONCLUSTERED INDEX [IX_AppClient_IssuerID]
    ON [dbo].[AppClient]([IssuerId] ASC);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppClient_ID]
    ON [dbo].[AppClient]([Id] ASC);

