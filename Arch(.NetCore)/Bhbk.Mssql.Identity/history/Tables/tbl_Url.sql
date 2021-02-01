CREATE TABLE [history].[tbl_Url] (
    [Id]              UNIQUEIDENTIFIER   NOT NULL,
    [AudienceId]      UNIQUEIDENTIFIER   NOT NULL,
    [UrlHost]         NVARCHAR (1024)    NULL,
    [UrlPath]         NVARCHAR (1024)    NULL,
    [IsEnabled]       BIT                NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_Url]
    ON [history].[tbl_Url]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

