CREATE TABLE [dbo].[tbl_Role] (
    [Id]          UNIQUEIDENTIFIER NOT NULL,
    [AudienceId]  UNIQUEIDENTIFIER NOT NULL,
    [ActorId]     UNIQUEIDENTIFIER NULL,
    [Name]        NVARCHAR (128)   NOT NULL,
    [Description] NVARCHAR (256)   NULL,
    [Enabled]     BIT              CONSTRAINT [DF_tbl_Role_Enabled] DEFAULT ((0)) NOT NULL,
    [Immutable]   BIT              CONSTRAINT [DF_tbl_Role_Immutable] DEFAULT ((0)) NOT NULL,
    [Created]     DATETIME2 (7)    NOT NULL,
    [LastUpdated] DATETIME2 (7)    NULL,
    CONSTRAINT [PK_tbl_Role] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Role_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Role]
    ON [dbo].[tbl_Role]([Id] ASC);

