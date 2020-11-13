CREATE TABLE [dbo].[tbl_Url] (
    [Id]             UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]     UNIQUEIDENTIFIER   NOT NULL,
    [ActorId]        UNIQUEIDENTIFIER   NULL,
    [UrlHost]        NVARCHAR (1024)    NULL,
    [UrlPath]        NVARCHAR (1024)    NULL,
    [IsEnabled]      BIT                NOT NULL,
    [IsDeletable]    BIT                NOT NULL,
    [CreatedUtc]     DATETIMEOFFSET (7) NOT NULL,
    [LastUpdatedUtc] DATETIMEOFFSET (7) NULL,
    CONSTRAINT [PK_tbl_Url] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Url_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE
);












GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Url]
    ON [dbo].[tbl_Url]([Id] ASC);

