CREATE TABLE [dbo].[AppClient] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [Name]        NVARCHAR (256)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [ClientKey]   NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppClient_ID] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppClient_ID]
    ON [dbo].[AppClient]([Id] ASC);

