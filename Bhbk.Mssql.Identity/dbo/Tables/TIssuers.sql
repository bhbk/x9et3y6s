CREATE TABLE [dbo].[TIssuers] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (MAX)   NOT NULL,
    [Description] NVARCHAR (MAX)   NULL,
    [IssuerKey]   NVARCHAR (MAX)   NOT NULL,
    [Enabled]     BIT              NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    [Immutable]   BIT              NOT NULL,
    CONSTRAINT [PK_TIssuers] PRIMARY KEY CLUSTERED ([Id] ASC)
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_TIssuers]
    ON [dbo].[TIssuers]([Id] ASC);

