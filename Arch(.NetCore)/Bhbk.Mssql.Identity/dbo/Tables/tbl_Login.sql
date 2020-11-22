CREATE TABLE [dbo].[tbl_Login] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [Name]           NVARCHAR (128)     NOT NULL,
    [Description]    NVARCHAR (256)     NULL,
    [LoginKey]       NVARCHAR (256)     NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Login] PRIMARY KEY CLUSTERED ([Id] ASC)
);
















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Login]
    ON [dbo].[tbl_Login]([Id] ASC);

