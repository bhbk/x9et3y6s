CREATE TABLE [dbo].[tbl_Url] (
    [Id]              UNIQUEIDENTIFIER                                   NOT NULL,
    [AudienceId]      UNIQUEIDENTIFIER                                   NOT NULL,
    [UrlHost]         NVARCHAR (1024)                                    NULL,
    [UrlPath]         NVARCHAR (1024)                                    NULL,
    [IsEnabled]       BIT                                                NOT NULL,
    [IsDeletable]     BIT                                                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7)                                 NOT NULL,
    [VersionStartUtc] DATETIME2 (7) GENERATED ALWAYS AS ROW START HIDDEN DEFAULT (GETUTCDATE()) NOT NULL,
    [VersionEndUtc]   DATETIME2 (7) GENERATED ALWAYS AS ROW END HIDDEN   DEFAULT (CONVERT([datetime2],'9999-12-31 23:59:59.9999999')) NOT NULL,
    CONSTRAINT [PK_tbl_Url] PRIMARY KEY CLUSTERED ([Id] ASC),
    CONSTRAINT [FK_tbl_Url_AudienceID] FOREIGN KEY ([AudienceId]) REFERENCES [dbo].[tbl_Audience] ([Id]) ON DELETE CASCADE ON UPDATE CASCADE,
    PERIOD FOR SYSTEM_TIME ([VersionStartUtc], [VersionEndUtc])
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE=[history].[tbl_Url], DATA_CONSISTENCY_CHECK=ON));


















GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_tbl_Url]
    ON [dbo].[tbl_Url]([Id] ASC);

