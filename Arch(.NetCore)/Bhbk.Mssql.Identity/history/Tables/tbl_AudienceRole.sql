CREATE TABLE [history].[tbl_AudienceRole] (
    [AudienceId]      UNIQUEIDENTIFIER   NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [CreatedUtc]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStartUtc] DATETIME2 (7)      NOT NULL,
    [VersionEndUtc]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_AudienceRole]
    ON [history].[tbl_AudienceRole]([VersionEndUtc] ASC, [VersionStartUtc] ASC) WITH (DATA_COMPRESSION = PAGE);

