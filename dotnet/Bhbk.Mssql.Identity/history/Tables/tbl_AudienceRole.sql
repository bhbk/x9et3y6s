CREATE TABLE [history].[tbl_AudienceRole] (
    [AudienceId]      UNIQUEIDENTIFIER   NOT NULL,
    [RoleId]          UNIQUEIDENTIFIER   NOT NULL,
    [IsDeletable]     BIT                NOT NULL,
    [Created]      DATETIMEOFFSET (7) NOT NULL,
    [VersionStart] DATETIME2 (7)      NOT NULL,
    [VersionEnd]   DATETIME2 (7)      NOT NULL
);


GO
CREATE CLUSTERED INDEX [ix_tbl_AudienceRole]
    ON [history].[tbl_AudienceRole]([VersionEnd] ASC, [VersionStart] ASC) WITH (DATA_COMPRESSION = PAGE);

