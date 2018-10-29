CREATE TABLE [dbo].[AppIssuer] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [IssuerKey]   NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_AppIssuer_ID] PRIMARY KEY CLUSTERED ([Id] ASC)
);




GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_AppIssuer_ID]
    ON [dbo].[AppIssuer]([Id] ASC);

