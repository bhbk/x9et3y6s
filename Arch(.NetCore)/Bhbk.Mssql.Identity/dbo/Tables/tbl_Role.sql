CREATE TABLE [dbo].[tbl_Role] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NOT NULL,
    [Name]           NVARCHAR (128)     NOT NULL,
    [Description]    NVARCHAR (256)     NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Role] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Role_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id])
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Role]
    ON [dbo].[tbl_Role]([Id] ASC);

